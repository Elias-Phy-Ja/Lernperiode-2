// Numberguesser by Elias

using System.Threading;


Random random = new Random();
int numberToGuess = random.Next(1, 101);
int attempts = 0;
bool guessedCorrectly = false;

 // Begrüssung
Console.WriteLine("Welcome to Number Guesser!");
Thread.Sleep(1000);
Console.WriteLine("Um das Programm vorzeitig zu beenden, drücke Strg + C.");
Thread.Sleep(1000);
Console.WriteLine("Ich habe eine Zahl zwischen 1 und 100 ausgewählt.");



// Schleife für die Benutzereingabe
while (!guessedCorrectly) 
{
    Thread.Sleep(1000);
    Console.Write("Dein Tipp: ");
    string input = Console.ReadLine();

    if (int.TryParse(input, out int guessedNumber)) // Formatiert wenn möglich und bei error stürtzt das Programm nicht ab sondern gibt false zurück
    {
        attempts++;

        if (guessedNumber == numberToGuess)
        {
            Console.WriteLine($"Herzlichen Glückwunsch! Du hast die Zahl {numberToGuess} in {attempts} Versuchen erraten!");
            guessedCorrectly = true;
        }
        else if (guessedNumber < numberToGuess)
        {
            Console.WriteLine("Höher!");
        }
        else
        {
            Console.WriteLine("Tiefer!");
        }
    }
    else
    {
        Console.WriteLine("Ungültige Eingabe. Bitte gib eine ganze Zahl ein.");
    }
}