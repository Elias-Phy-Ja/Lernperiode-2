namespace Shellmate.Models;

public class ChatMessage
{
    public string Role { get; set; } = "user"; // "user" = du, "model" = Gemini
    public string Text { get; set; } = "";     // Inhalt der Nachricht
}