namespace Shellmate.Services;

// Eigener Fehlertyp für alles, was bei einer Anfrage an Gemini schiefgehen kann.
// Die Message ist so geschrieben, dass sie direkt dem Benutzer angezeigt werden kann.
public class GeminiException : Exception
{
    public GeminiException(string message) : base(message)
    {
    }
}
