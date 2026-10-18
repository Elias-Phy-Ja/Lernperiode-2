using System.Text;
using System.Text.Json;
using Shellmate.Models;

namespace Shellmate.Services;

public class GeminiService
{
    private readonly HttpClient client; // Schreib geschützt
    private readonly string url; // Schreib geschützt

    // Einmal beim Start: Client einrichten
    public GeminiService(AppConfig config)
    {
        client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-goog-api-key", config.ApiKey); // API-Key aus Config in Header einfügen
        url = $"https://generativelanguage.googleapis.com/v1beta/models/{config.Model}:generateContent"; //Url und Modell aus Config einfügen
    }

    // Schickt einen Prompt an Gemini und gibt die Antwort zurück
    public async Task<string?> AskAsync(string prompt)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        string json = JsonSerializer.Serialize(requestBody); // Prompt in JSON umwandeln
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response; // vor dem try deklarieren, damit sie danach noch existiert

        try // Anfrage an die Gemini-API senden
        {
            response = await client.PostAsync(url, content);
        }
        catch (HttpRequestException e) // Fallback bei Netzwerkproblemen
        {
            Console.WriteLine();
            Console.WriteLine("Netzwerkfehler: Es konnte keine Verbindung zur Google Gemini API hergestellt werden.");
            Console.WriteLine($"Details: {e.Message}");
            Console.WriteLine();
            return null;
        }

        if (!response.IsSuccessStatusCode) // Fallback bei Errors
        {
            string errorMessage = await response.Content.ReadAsStringAsync();
            Console.WriteLine();
            Console.WriteLine($"Fehler: {response.StatusCode}");
            Console.WriteLine("Leider konnte die Anfrage nicht erfolgreich verarbeitet werden.");
            Console.WriteLine($"Details: {errorMessage.Trim()}");
            Console.WriteLine();
            return null;
        }

        string responseText = await response.Content.ReadAsStringAsync(); // Antworttext auslesen

        using JsonDocument doc = JsonDocument.Parse(responseText); // JSON-Dokument parsen

        try // die ganze Kette steht im try
        {
            return doc.RootElement // Navigieren durch die JSON-Antwort, um den Text zu extrahieren
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();
        }
        catch (KeyNotFoundException) // ein Feld fehlt
        {
            ShowEmptyAnswerError();
            return null;
        }
        catch (IndexOutOfRangeException) // eine Liste ist leer
        {
            ShowEmptyAnswerError();
            return null;
        }
    }

    private static void ShowEmptyAnswerError()
    {
        Console.WriteLine();
        Console.WriteLine("Fehler: Gemini hat keine Antwort geliefert.");
        Console.WriteLine("Möglicherweise wurde die Anfrage blockiert. Versuchen Sie es mit einer anderen Formulierung.");
        Console.WriteLine();
    }
}