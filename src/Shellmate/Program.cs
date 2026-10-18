using Shellmate.Commands;
using Shellmate.Services;
using Shellmate.Models;
using Shellmate.UI;

Console.WriteLine();
Console.WriteLine("Herzlich willkommen bei Shellmate!");
Console.WriteLine("Mit /help können Sie sich die verfügbaren Befehle anzeigen lassen.");
Console.WriteLine();


// Config laden, beim ersten Start den Setup Wizard starten
AppConfig? config = ConfigService.LoadConfig();

if (config == null || string.IsNullOrWhiteSpace(config.ApiKey)) // Keine Config oder kein Key
{
    config = SetupWizard.Run();
}

GeminiService gemini = new GeminiService(config.ApiKey); // Service einmal erstellen


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
            Console.WriteLine(answer.Trim());
            Console.WriteLine();
        }
    }
}