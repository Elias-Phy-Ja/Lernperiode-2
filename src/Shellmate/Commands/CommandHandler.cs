using Shellmate.Models;
using Shellmate.Services;
using Shellmate.UI;
using Spectre.Console;

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
            ClearChat(gemini, config);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine($"Unbekannter Befehl: {command}");
            ShowHelp();
        }

        return true; // Gibt true zurück, um isRunning in Programm.cs zu setzen
    }

    public static void ShowWelcome(AppConfig? config = null)
    {
        string model = config?.Model ?? "Setup nötig";
        string qualle = "[deepskyblue2]  ▄▀▀▀▀▀▀▀▄[/]\n" + "[deepskyblue2]  █ •   • █[/]\n" + "[deepskyblue2]  ▀▄▄▄▄▄▄▄▀[/]\n" + "[deepskyblue4]   ╎ ┊ ┊ ╎[/]\n" + "[deepskyblue4]   ┊ ╎ ╎ ┊[/]";
        // Version wie bei "shellmate --version" (z.B. 1.0.1)
        string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "?";

        // die 4 Info-Zeilen rechts neben der Qualle
        string infos =
            "[bold deepskyblue2]Willkommen bei Shellmate[/] [grey]v" + version + "[/]\n" +
            "[grey]Modell:[/] " + Markup.Escape(model) + "\n" +
            "[grey]" + Markup.Escape(Environment.CurrentDirectory) + "[/]\n" +
            "[grey]/help für alle Befehle[/]";

        // Grid = unsichtbare Tabelle mit 2 Spalten: links die Qualle, rechts die Infos
        Grid grid = new Grid();
        grid.AddColumn(new GridColumn { Padding = new Padding(0, 0, 4, 0) }); // Abstand rechts (links, oben, rechts, unten)
        grid.AddColumn();
        grid.AddRow(new Markup(qualle), new Markup(infos));

        // Rahmen um alles, wie bei Claude Code
        Panel panel = new Panel(grid)
            .RoundedBorder()                   // runde Ecken
            .BorderColor(Color.DeepSkyBlue2);  // gleiches Blau wie die Qualle

        Console.WriteLine();
        AnsiConsole.Write(panel);
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

    public static void ClearChat(GeminiService? gemini, AppConfig? config)
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
            ShowWelcome(config);   // wie beim Start
        }
    }

    // Zeichnet den ganzen Chat aus dem Verlauf neu, als wäre man nie weg gewesen
    public static void RedrawChat(GeminiService gemini, AppConfig config)
    {
        Console.Clear();
        ShowWelcome(config);

        foreach (ChatMessage message in gemini.History) // jede Nachricht einzeln durchgehen
        {
            if (message.Role == "user")
            {
                Console.WriteLine($"\x1b[38;5;39m> {message.Text}\x1b[0m"); // blau, so wie es beim Tippen aussah
            }
            else if (message.Role == "model")
            {
                Console.WriteLine();
                MarkdownRenderer.Render(message.Text.Trim()); // so wie in Program.cs
                Console.WriteLine();
            }
        }
    }
}