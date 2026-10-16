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
        ShowHelp();
        continue; // Zurück zum Anfang der Schleife, um eine neue Eingabe zu erhalten
    }

    if (input.StartsWith("/")) // Überprüfen, ob die Eingabe ein Befehl ist
    {
        string loweredCommand = input.ToLower(); // Befehl in Kleinbuchstaben umwandeln

        if (loweredCommand == "/help")
        {
            ShowHelp();
        }
        else if (loweredCommand == "/exit")
        {
            isRunning = false;
            ExitProgram();
        }
        else
        {
            Console.WriteLine($"Unbekannter Befehl: {input}");
            ShowHelp();
        }
    }

    else // wird an Gemini weitergeleitet
    {
        Console.WriteLine();
        Console.WriteLine("Eingabe wird als normaler Text interpretiert und an Gemini weitergeleitet.");
        Console.WriteLine();
    }
}


static void ShowHelp()
{
    Console.WriteLine();
    Console.WriteLine("Shellmate - Verfügbare Befehle:");
    Console.WriteLine("/help - Zeigt diese Hilfe an");
    Console.WriteLine("/exit - Beendet das Programm");
    Console.WriteLine();
}

static void ExitProgram()
{
    Console.WriteLine();
    Console.WriteLine("Programm wird beendet...");
}