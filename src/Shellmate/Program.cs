using System.Text;
using System.Text.Json;
using Shellmate.Commands;

Console.WriteLine();
Console.WriteLine("Herzlich willkommen bei Shellmate!");
Console.WriteLine("Mit /help können Sie sich die verfügbaren Befehle anzeigen lassen.");
Console.WriteLine();

bool isRunning = true; // Variable, um die Schleife zu steuern

while (isRunning)
{
    Console.Write("> ");
    string? rawInput = Console.ReadLine(); // ? Um Nullwert-Warnungen zu akzeptieren
    string? input = rawInput?.Trim(); // Eingabe trimmen, um führende und nachfolgende Leerzeichen zu entfernen

    if (string.IsNullOrWhiteSpace(input)) // Fallback wenn input leer ist
    {
        Console.WriteLine();
        Console.WriteLine("Dieser Input ist leer.");
        Console.WriteLine("Bitte geben Sie einen Text für Gemini ein oder einen Befehl für Shellmate.");
        CommandHandler.ShowHelp();
        continue; // Zurück zum Anfang der Schleife, um eine neue Eingabe zu erhalten
    }

    if (input.StartsWith("/")) // Überprüfen, ob die Eingabe ein Befehl ist
    {
        isRunning = CommandHandler.Handle(input);
    }

    else // wird an Gemini weitergeleitet
    {
        Console.WriteLine();
        string? API_Key = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (string.IsNullOrEmpty(API_Key)) // Fallback bei keiem gesetzten API-Key
        {
            Console.WriteLine("Fehler: GEMINI_API_KEY ist nicht gesetzt. Bitte setzen Sie die Umgebungsvariable und starten Sie das Programm neu.");
            Console.WriteLine();
            continue; // Zurück zum Anfang der Schleife, um eine neue Eingabe zu erhalten
        }

        string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent";   // die Adresse aus der Doku

        var prompt = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = input }
                    }
                }
            }
        };

        string json = JsonSerializer.Serialize(prompt);

        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("x-goog-api-key", API_Key);

        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(url, content);

        if (!response.IsSuccessStatusCode) // Fallback bei einem Fehler in der Anfrage
        {
            string errorMessage = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Fehler: {response.StatusCode}");
            Console.WriteLine("Leider konnte die Anfrage nicht erfolgreich verarbeitet werden.");
            Console.WriteLine($"Details: {errorMessage}");
            Console.WriteLine();
            continue; // Zurück zum Anfang der Schleife, um eine neue Eingabe zu machen
        }

        string responseText = await response.Content.ReadAsStringAsync();

        using JsonDocument doc = JsonDocument.Parse(responseText);
        JsonElement root = doc.RootElement;


        string? answer = root
            .GetProperty("candidates")
            [0]
            .GetProperty("content")
            .GetProperty("parts")
            [0]
            .GetProperty("text")
            .GetString();

        Console.WriteLine(answer);
        Console.WriteLine();
    }
}
