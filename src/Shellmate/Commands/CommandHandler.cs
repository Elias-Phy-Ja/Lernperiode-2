using Shellmate.Models;
using Shellmate.Services;
using Shellmate.UI;

namespace Shellmate.Commands;

public class CommandHandler
{
    public static bool Handle(string command, GeminiService? gemini = null, AppConfig? config = null)
    {
        string loweredCommand = command.ToLower(); // Befehl in Kleinbuchstaben umwandeln

        if (loweredCommand == "/help")
        {
            ShowHelp();
        }
        else if (loweredCommand == "/exit")
        {
            return ExitProgram(); // "return" ist hier um die schleife nach ExitProgram direkt zu beenden
        }
        else if (loweredCommand == "/settings")
        {
            OpenSettings(gemini, config);
        }
        else if (loweredCommand == "/how-to-use")
        {
            HowToUse();
        }
        else if (loweredCommand == "/help-setup")
        {
            ShowSetupHelp();
        }
        else if (loweredCommand == "/clear")
        {
            ClearChat(gemini);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine($"Unbekannter Befehl: {command}");
            ShowHelp();
        }

        return true; // Gibt true zurück, um isRunning in Programm.cs zu setzen
    }

    public static void ShowWelcome()
    {
        Console.WriteLine();
        Console.WriteLine("Herzlich willkommen bei Shellmate!");
        Console.WriteLine("Mit /help können Sie sich die verfügbaren Befehle anzeigen lassen.");
        Console.WriteLine();
    }

    public static void ShowHelp()
    {
        Console.WriteLine();
        Console.WriteLine("Shellmate - Verfügbare Befehle:");
        Console.WriteLine("/help - Zeigt diese Hilfe an");
        Console.WriteLine("/exit - Beendet das Programm");
        Console.WriteLine("/help-setup - Zeigt Hilfe für den Setup Wizard an");
        Console.WriteLine("/how-to-use - Zeigt eine Anleitung zur Nutzung an");
        Console.WriteLine("/clear - Löscht den Chatverlauf und startet einen neuen Chat");
        Console.WriteLine("/settings - Öffnet die Einstellungen");
        Console.WriteLine();
    }

    public static bool ExitProgram()
    {
        Console.WriteLine();
        Console.WriteLine("Programm wird beendet...");
        return false; // Gibt false zurück, um isRunning in Programm.cs zu setzen
    }

    public static void OpenSettings(GeminiService? gemini, AppConfig? config)
    {
        if (gemini == null || config == null) // im Setup Wizard gibt es noch kein gemini und keine Config
        {
            Console.WriteLine();
            Console.WriteLine("Dieser Befehl ist erst im Chat verfügbar!");
            Console.WriteLine();
            return;
        }

        SettingsMenu.Run(config, gemini);
    }

    public static void ShowSetupHelp()
    {
        Console.WriteLine();
        Console.WriteLine("Shellmate Setup Wizard - Hilfe:");
        Console.WriteLine("Sie benötigen einen API Key von Google Gemini, um Shellmate nutzen zu können.");
        Console.WriteLine("Dafür melden Sie sich bei Google Gemini an");
        Console.WriteLine("> https://aistudio.google.com/");
        Console.WriteLine("Generieren Sie einen neuen API-Key und geben Sie ihn im Setup Wizard ein.");
        Console.WriteLine();
    }

    public static void HowToUse()
    {
        Console.WriteLine();
        Console.WriteLine("Shellmate - Anleitung zur Nutzung:");
        Console.WriteLine();
        Console.WriteLine("1. Geben Sie Ihren Google Gemini API-Key im Setup Wizard ein, falls noch nicht getan.");
        Console.WriteLine("2. In Shellmate können Sie bei dem > Zeichen etwas eingeben.");
        Console.WriteLine("3. Um Befehle zu verwenden, geben Sie diese mit einem / davor ein.");
        Console.WriteLine("4. Um alle Befehle anzuzeigen, geben Sie /help ein.");
        Console.WriteLine("5. Auf jeden anderen Text antwortet Shellmate mit Hilfe von Google Gemini.");
        Console.WriteLine();
    }

    public static void ClearChat(GeminiService? gemini)
    {
        if (gemini == null) // im Setup Wizard gibt es noch kein gemini
        {
            Console.WriteLine();
            Console.WriteLine("Dieser Befehl ist erst im Chat verfügbar!");
            Console.WriteLine();
        }
        else
        {
            gemini.ClearHistory(); // Gemini vergisst alles
            Console.Clear();       // Terminal leeren
            ShowWelcome();         // wie beim Start
        }
    }

    // Zeichnet den ganzen Chat aus dem Verlauf neu, als wäre man nie weg gewesen
    public static void RedrawChat(GeminiService gemini)
    {
        Console.Clear();
        ShowWelcome();

        foreach (ChatMessage message in gemini.History) // jede Nachricht einzeln durchgehen
        {
            if (message.Role == "user")
            {
                Console.WriteLine($"> {message.Text}"); // so wie es beim Tippen aussah
            }
            else if (message.Role == "model")
            {
                Console.WriteLine();
                Console.WriteLine(message.Text.Trim()); // so wie in Program.cs
                Console.WriteLine();
            }
        }
    }
}