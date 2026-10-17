using Shellmate.Models;
using Shellmate.Commands;
using Shellmate.Services;

namespace Shellmate.UI;

public static class SetupWizard
{
    public static AppConfig Run()
    {
        bool CorrectAPIKey = false; // Variable, um zu überprüfen, ob der API Key korrekt ist
        string APIKey = ""; // Variable, um den API Key zu speichern

        Console.WriteLine();
        Console.WriteLine("Willkommen zum Shellmate Setup Wizard!");
        Console.WriteLine("Bitte geben Sie Ihren Google Gemini API Key ein, um fortzufahren.");
        Console.WriteLine("Falls Sie Hilfe benötigen, geben Sie /help-setup ein.");

        do
        {
            Console.WriteLine();
            Console.Write("API Key: ");
            APIKey = Console.ReadLine()?.Trim() ?? ""; // Bei null wird ein leerer Text verwendet

            if (string.IsNullOrWhiteSpace(APIKey)) // Fallback wenn input leer ist
            {
                Console.WriteLine();
                Console.WriteLine("Dieser Input ist leer.");
                Console.WriteLine("Bitte geben Sie den API Key ein oder /help-setup für weitere Informationen.");
            }
            else if (APIKey.StartsWith("/")) // Überprüfen, ob die Eingabe ein Befehl ist
            {
                CommandHandler.Handle(APIKey); // Befehl verarbeiten
            }
            else if (APIKey.Length < 20) // Überprüfen, ob der API Key zu kurz ist
            {
                Console.WriteLine();
                Console.WriteLine("Der eingegebene API Key ist zu kurz. Bitte überprüfen Sie den Key und versuchen Sie es erneut.");
            }
            else // Key ist lang genug
            {
                CorrectAPIKey = true; // beendet die Schleife
            }
        } while (!CorrectAPIKey); // Wiederholen, bis der API Key korrekt ist

        AppConfig config = new AppConfig { ApiKey = APIKey }; // Config-Objekt erstellen und auffangen
        ConfigService.SaveConfig(config); // Speichern der Konfiguration

        Console.WriteLine();
        Console.WriteLine("Vielen Dank! Der API Key wurde erfolgreich gespeichert.");
        Console.WriteLine("Sie können nun Shellmate verwenden:");
        Console.WriteLine();

        return config; // dasselbe Objekt zurückgeben
    }
}