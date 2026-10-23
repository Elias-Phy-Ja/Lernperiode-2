using System.Text;
using System.Text.Json;
using Shellmate.Models;

namespace Shellmate.Services;

public class GeminiService
{
    private readonly HttpClient client; // Schreibgeschützt
    private string url = ""; // nicht readonly, weil ApplyConfig die URL später ändern kann
    private string model = ""; // aktuelles Modell, für verständliche Fehlermeldungen
    private readonly List<ChatMessage> history = new List<ChatMessage>(); // alle bisherigen Nachrichten (erstellt Liste)
    public IReadOnlyList<ChatMessage> History => history; // Verlauf nur lesbar nach aussen

    // Einmal beim Start: Client einrichten
    public GeminiService(AppConfig config)
    {
        client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(90); // nicht ewig warten, falls Gemini hängt
        ApplyConfig(config); // Key und Modell übernehmen
    }

    // Übernimmt Key und Modell aus der Config (beim Start und nach Änderungen in /settings)
    public void ApplyConfig(AppConfig config)
    {
        client.DefaultRequestHeaders.Remove("x-goog-api-key"); // alten Key entfernen (falls vorhanden)
        client.DefaultRequestHeaders.Add("x-goog-api-key", config.ApiKey); // API-Key aus Config in Header einfügen
        model = config.Model;
        url = $"https://generativelanguage.googleapis.com/v1beta/models/{config.Model}:generateContent"; // Url und Modell aus Config einfügen
    }

    //Verlauf leeren (für /clear)
    public void ClearHistory()
    {
        history.Clear();
    }

    // Schickt einen Prompt mit dem ganzen Verlauf an Gemini und gibt die Antwort zurück.
    // Geht etwas schief, wird eine GeminiException mit einer verständlichen Meldung geworfen.
    // Der Service gibt selbst nichts aus, das macht Program.cs (wegen dem Spinner).
    public async Task<string> AskAsync(string prompt)
    {
        // neue Nachricht erstellen, aber noch NICHT in den Verlauf als sicherheit bei einem Netzwerk fehler
        // nicht dass mehrere user Nachrichten ohne Antwort von Model da stehen.
        ChatMessage userMessage = new ChatMessage { Role = "user", Text = prompt };

        // Kopie vom Verlauf + neue Nachricht
        List<ChatMessage> messagesToSend = new List<ChatMessage>(history);
        messagesToSend.Add(userMessage);

        var requestBody = new
        {
            // aus jeder ChatMessage wird ein JSON-Eintrag mit role und parts
            contents = messagesToSend.Select(message => new
            {
                role = message.Role,
                parts = new[]
                {
                    new { text = message.Text }
                }
            })
        };

        string json = JsonSerializer.Serialize(requestBody); // Verlauf in JSON umwandeln
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response; // vor dem try deklarieren, damit sie danach noch existiert

        try // Anfrage an die Gemini-API senden
        {
            response = await client.PostAsync(url, content);
        }
        catch (HttpRequestException) // kein Internet, DNS-Problem, ...
        {
            throw new GeminiException("Keine Verbindung zu Google Gemini. Bitte überprüfen Sie Ihre Internetverbindung.");
        }
        catch (TaskCanceledException) // Timeout
        {
            throw new GeminiException("Gemini hat zu lange nicht geantwortet. Bitte versuchen Sie es nochmal.");
        }

        if (!response.IsSuccessStatusCode) // Fehler von Google -> verständliche Meldung
        {
            string errorBody = await response.Content.ReadAsStringAsync();
            throw new GeminiException(TranslateError((int)response.StatusCode, errorBody));
        }

        string responseText = await response.Content.ReadAsStringAsync(); // Antworttext auslesen

        using JsonDocument doc = JsonDocument.Parse(responseText); // JSON-Dokument parsen

        string? answer; // vor dem try deklarieren (Scope)

        try
        {
            answer = doc.RootElement // Navigieren durch die JSON-Antwort, um den Text zu extrahieren
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
        }
        catch (Exception e) when (e is KeyNotFoundException || e is IndexOutOfRangeException) // Feld fehlt oder Liste leer
        {
            answer = null;
        }

        if (string.IsNullOrWhiteSpace(answer)) // keine Antwort, meistens blockiert
        {
            throw new GeminiException("Gemini hat keine Antwort geliefert. Möglicherweise wurde die Anfrage blockiert. Versuchen Sie es mit einer anderen Formulierung.");
        }

        // erst jetzt, wo alles geklappt hat, beide Nachrichten in den Verlauf
        history.Add(userMessage);
        history.Add(new ChatMessage { Role = "model", Text = answer });

        return answer;
    }

    // Macht aus dem HTTP-Statuscode eine Meldung, mit der der Benutzer etwas anfangen kann
    private string TranslateError(int statusCode, string errorBody)
    {
        if (errorBody.Contains("API_KEY_INVALID") || errorBody.Contains("API key not valid"))
        {
            return "Der API Key ist ungültig. Ändern Sie ihn mit /settings.";
        }

        return statusCode switch
        {
            400 => "Die Anfrage wurde von Gemini abgelehnt. Versuchen Sie es mit einer anderen Formulierung oder starten Sie mit /clear neu.",
            401 or 403 => "Zugriff verweigert. Überprüfen Sie Ihren API Key mit /settings.",
            404 => $"Das Modell \"{model}\" wurde nicht gefunden. Ändern Sie es mit /settings.",
            429 => "Zu viele Anfragen oder das Gratis-Kontingent ist aufgebraucht. Warten Sie kurz und versuchen Sie es nochmal.",
            >= 500 => "Gemini ist gerade überlastet oder nicht erreichbar. Versuchen Sie es in ein paar Sekunden nochmal.",
            _ => $"Unerwarteter Fehler von Gemini (Code {statusCode})."
        };
    }
}
