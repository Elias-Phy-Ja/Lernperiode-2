using System.Text;
using System.Text.Json;

namespace Shellmate.Services;

public class GeminiService
{
    private readonly HttpClient client;
    private const string Url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent";

    // Einmal beim Start: Client einrichten
    public GeminiService(string apiKey)
    {
        client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
    }

    // Bei jeder Frage
    public async Task<string?> AskAsync(string prompt) // Prompt für JSON-Formatierung vorbereiten
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

        HttpResponseMessage response = await client.PostAsync(Url, content); // Anfrage an die Gemini-API senden

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

        return doc.RootElement // Navigieren durch die JSON-Antwort, um den Text zu extrahieren
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();
    }
}