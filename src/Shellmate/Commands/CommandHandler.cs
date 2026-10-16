namespace Shellmate.Commands;

public class CommandHandler
{

    public static bool Handle(string command)
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
        else
        {
            Console.WriteLine();
            Console.WriteLine($"Unbekannter Befehl: {command}");
            ShowHelp();
        }

        return true; // Gibt true zurück, um isRunning in Programm.cs zu setzen
    }

    public static void ShowHelp()
    {
        Console.WriteLine();
        Console.WriteLine("Shellmate - Verfügbare Befehle:");
        Console.WriteLine("/help - Zeigt diese Hilfe an");
        Console.WriteLine("/exit - Beendet das Programm");
        Console.WriteLine();
    }

    public static bool ExitProgram()
    {
        Console.WriteLine();
        Console.WriteLine("Programm wird beendet...");
        return false; // Gibt false zurück, um isRunning in Programm.cs zu setzen
    }
}
