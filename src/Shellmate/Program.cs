using Shellmate.Commands;
using Shellmate.Services;
using Shellmate.Models;
using Shellmate.UI;
using Spectre.Console;

Console.OutputEncoding = System.Text.Encoding.UTF8; // damit Sonderzeichen wie • richtig angezeigt werden
_ = AnsiConsole.Profile; // aktiviert Farb-Codes (ANSI) auch in älteren Windows-Konsolen

// "shellmate --version" zeigt nur die Version an
if (args.Contains("--version") || args.Contains("-v"))
{
    Console.WriteLine($"Shellmate {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}");
    return;
}

// Config laden, beim ersten Start den Setup Wizard starten
AppConfig? config = ConfigService.LoadConfig();

Console.Clear();
CommandHandler.ShowWelcome(config); // Begrüssung mit Qualle, Version und Modell

if (config == null || string.IsNullOrWhiteSpace(config.ApiKey)) // Keine Config oder kein Key
{
    config = SetupWizard.Run();
}

GeminiService gemini = new GeminiService(config); // Service einmal erstellen


bool isRunning = true; // Variable, um die Schleife zu steuern


while (isRunning)
{
    Console.Write("\x1b[38;5;39m> "); // ab hier alles blau (auch was der Benutzer tippt)
    string? input = Console.ReadLine()?.Trim(); // Eingabe lesen und trimmen
    Console.Write("\x1b[0m"); // Farbe zurücksetzen

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
        isRunning = CommandHandler.Handle(input, gemini, config);
    }

    else // wird an Gemini weitergeleitet
    {
        try
        {
            // Spinner anzeigen, solange auf Gemini gewartet wird
            string answer = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("deepskyblue1")) // gleiche Farbe wie der Input
                .StartAsync("Shellmate denkt nach...", async ctx => await gemini.AskAsync(input));

            Console.WriteLine();
            MarkdownRenderer.Render(answer.Trim()); // Markdown schön ausgeben
            Console.WriteLine();
        }
        catch (GeminiException e) // verständliche Fehlermeldung in Rot
        {
            Console.WriteLine();
            AnsiConsole.MarkupLine($"[red]Fehler:[/] {Markup.Escape(e.Message)}");
            Console.WriteLine();
        }
    }
}