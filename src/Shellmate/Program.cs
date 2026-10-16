using Shellmate.Commands;
using Shellmate.Services;

Console.WriteLine();
Console.WriteLine("Herzlich willkommen bei Shellmate!");
Console.WriteLine("Mit /help können Sie sich die verfügbaren Befehle anzeigen lassen.");
Console.WriteLine();

// API Key einmal beim Start prüfen
string? apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

if (string.IsNullOrEmpty(apiKey)) // Fallback bei keinem gesetzten API-Key
{
    Console.WriteLine("Fehler: GEMINI_API_KEY ist nicht gesetzt. Bitte setzen Sie die Umgebungsvariable und starten Sie das Programm neu.");
    return; // Programm beenden
}

GeminiService gemini = new GeminiService(apiKey); // Service einmal erstellen

bool isRunning = true; // Variable, um die Schleife zu steuern


while (isRunning)
{
    Console.Write("> ");
    string? input = Console.ReadLine()?.Trim(); // Eingabe lesen und trimmen

    if (string.IsNullOrWhiteSpace(input)) // Fallback wenn input leer ist
    {
        Console.WriteLine();
        Console.WriteLine("Dieser Input ist leer.");
        Console.WriteLine("Bitte geben Sie einen Text für Gemini ein oder einen Befehl für Shellmate.");
        CommandHandler.ShowHelp();
        continue; // Zurück zum Anfang der Schleife
    }

    if (input.StartsWith("/")) // Überprüfen, ob die Eingabe ein Befehl ist
    {
        isRunning = CommandHandler.Handle(input);
    }

    else // wird an Gemini weitergeleitet
    {
        string? answer = await gemini.AskAsync(input);

        if (answer != null)
        {
            Console.WriteLine();
            Console.WriteLine(answer);
            Console.WriteLine();
        }
    }
}