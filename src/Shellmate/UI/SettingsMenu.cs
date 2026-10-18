using Shellmate.Commands;
using Shellmate.Models;
using Shellmate.Services;

namespace Shellmate.UI;

public static class SettingsMenu
{
    public static void Run(AppConfig config, GeminiService gemini)
    {
        bool inSettings = true;     // Variable, um die Menü-Schleife zu steuern
        string? statusMessage = null; // Rückmeldung, die nach dem Console.Clear() angezeigt wird

        while (inSettings)
        {
            Console.Clear();
            ShowMenu(config, statusMessage);
            statusMessage = null; // Meldung nur einmal anzeigen

            Console.Write("Auswahl: ");
            string choice = Console.ReadLine()?.Trim() ?? "";

            if (choice == "1")
            {
                statusMessage = ChangeModel(config, gemini);
            }
            else if (choice == "2")
            {
                statusMessage = ChangeApiKey(config, gemini);
            }
            else if (choice == "3")
            {
                if (ResetShellmate(config, gemini))
                {
                    return; // Shellmate wurde neu eingerichtet, Chat ist leer -> direkt zurück
                }
                statusMessage = "Zurücksetzen abgebrochen.";
            }
            else if (choice == "4")
            {
                inSettings = false; // Schleife beenden
            }
            else
            {
                statusMessage = "Ungültige Auswahl. Bitte 1, 2, 3 oder 4 eingeben.";
            }
        }

        CommandHandler.RedrawChat(gemini); // Chat wiederherstellen
    }

    private static void ShowMenu(AppConfig config, string? statusMessage)
    {
        Console.WriteLine();
        Console.WriteLine("Shellmate - Einstellungen");
        Console.WriteLine();
        Console.WriteLine($"Modell:  {config.Model}");
        Console.WriteLine($"API Key: {MaskKey(config.ApiKey)}");
        Console.WriteLine();
        Console.WriteLine("1 - Modell ändern");
        Console.WriteLine("2 - API Key ändern");
        Console.WriteLine("3 - Shellmate zurücksetzen");
        Console.WriteLine("4 - Zurück zum Chat");
        Console.WriteLine();

        if (statusMessage != null) // Rückmeldung der letzten Aktion
        {
            Console.WriteLine(statusMessage);
            Console.WriteLine();
        }
    }

    // Zeigt nur die ersten und letzten 4 Zeichen des Keys an
    private static string MaskKey(string apiKey)
    {
        if (apiKey.Length <= 8) // zu kurz zum Maskieren -> gar nichts verraten
        {
            return "****";
        }

        return $"{apiKey.Substring(0, 4)}...{apiKey.Substring(apiKey.Length - 4)}";
    }

    private static string ChangeModel(AppConfig config, GeminiService gemini)
    {
        Console.WriteLine();
        Console.WriteLine("Beispiele: gemini-3.5-flash, gemini-3.5-pro");
        Console.Write("Neues Modell (leer = abbrechen): ");
        string newModel = Console.ReadLine()?.Trim().ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(newModel))
        {
            return "Abgebrochen. Das Modell wurde nicht geändert.";
        }

        config.Model = newModel;
        ConfigService.SaveConfig(config); // dauerhaft speichern
        gemini.ApplyConfig(config);       // sofort übernehmen

        return $"Modell geändert zu {newModel}. Bei einem falschen Namen erscheint beim nächsten Prompt ein Fehler.";
    }

    private static string ChangeApiKey(AppConfig config, GeminiService gemini)
    {
        Console.WriteLine();
        Console.Write("Neuer API Key (leer = abbrechen): ");
        string newKey = Console.ReadLine()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(newKey))
        {
            return "Abgebrochen. Der API Key wurde nicht geändert.";
        }

        if (newKey.Length < 20) // gleiche Prüfung wie im Setup Wizard
        {
            return "Der eingegebene API Key ist zu kurz. Er wurde nicht gespeichert.";
        }

        config.ApiKey = newKey;
        ConfigService.SaveConfig(config); // dauerhaft speichern
        gemini.ApplyConfig(config);       // sofort übernehmen

        return "API Key wurde geändert.";
    }

    // Gibt true zurück, wenn wirklich zurückgesetzt wurde
    private static bool ResetShellmate(AppConfig config, GeminiService gemini)
    {
        Console.WriteLine();
        Console.WriteLine("Achtung: Dabei werden der API Key, die Einstellungen und der Chatverlauf gelöscht.");
        Console.Write("Zum Bestätigen \"ja\" eingeben: ");
        string confirmation = Console.ReadLine()?.Trim().ToLower() ?? "";

        if (confirmation != "ja")
        {
            return false;
        }

        ConfigService.DeleteConfig(); // config.json löschen
        gemini.ClearHistory();        // Verlauf löschen

        Console.Clear();
        CommandHandler.ShowWelcome();
        AppConfig newConfig = SetupWizard.Run(); // wie beim ersten Start (speichert selbst)

        // Neue Werte ins bestehende Objekt übernehmen, damit Program.cs sie auch hat
        config.ApiKey = newConfig.ApiKey;
        config.Model = newConfig.Model;
        gemini.ApplyConfig(config);

        return true;
    }
}
