using System.Text.RegularExpressions;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Shellmate.UI;

// Wandelt die Markdown-Antwort von Gemini in farbige Terminal-Ausgabe um.
// Ablauf: Text Zeile für Zeile durchgehen und erkennen, was die Zeile ist
// (Überschrift, Liste, Codeblock, Tabelle, ...). Innerhalb einer Zeile werden
// dann **fett**, *kursiv*, `code` und [Links](url) umgewandelt.
public static class MarkdownRenderer
{
    // Wohin geschrieben wird (die Konsole)
    private static IAnsiConsole output = AnsiConsole.Console;

    public static void Render(string markdown)
    {
        string[] lines = markdown.Replace("\r\n", "\n").Split('\n');

        bool inCodeBlock = false;          // sind wir gerade zwischen ``` und ``` ?
        string codeLanguage = "";          // z.B. "csharp" nach ```csharp
        List<string> codeLines = new();    // gesammelte Zeilen des Codeblocks
        List<string> tableLines = new();   // gesammelte Zeilen einer Tabelle

        foreach (string line in lines)
        {
            // Codeblöcke
            if (line.TrimStart().StartsWith("```"))
            {
                if (!inCodeBlock) // Codeblock beginnt
                {
                    FlushTable(tableLines);
                    inCodeBlock = true;
                    codeLanguage = line.Trim().Substring(3).Trim();
                    codeLines.Clear();
                }
                else // Codeblock endet
                {
                    RenderCodeBlock(codeLines, codeLanguage);
                    inCodeBlock = false;
                }
                continue;
            }

            if (inCodeBlock)
            {
                codeLines.Add(line);
                continue;
            }

            // Tabellen (Zeilen, die mit | beginnen)
            if (line.TrimStart().StartsWith("|"))
            {
                tableLines.Add(line.Trim());
                continue;
            }
            FlushTable(tableLines); // Tabelle ist zu Ende, falls es eine gab

            RenderLine(line);
        }

        // Falls Gemini einen Codeblock nicht geschlossen hat
        if (inCodeBlock)
        {
            RenderCodeBlock(codeLines, codeLanguage);
        }
        FlushTable(tableLines);
    }

    // Eine normale Zeile (kein Code, keine Tabelle)
    private static void RenderLine(string line)
    {
        string trimmed = line.TrimStart();
        string indent = new string(' ', line.Length - trimmed.Length);

        // Überschriften: #, ##, ### ...
        Match heading = Regex.Match(trimmed, @"^(#{1,6})\s+(.*)$");
        if (heading.Success)
        {
            int level = heading.Groups[1].Value.Length;
            string text = FormatInline(heading.Groups[2].Value);
            string color = level == 1 ? "deepskyblue1" : level == 2 ? "skyblue1" : "lightskyblue1";
            WriteMarkupLine($"[bold {color}]{text}[/]", line);
            return;
        }

        // Trennlinie: --- oder ***
        if (Regex.IsMatch(trimmed, @"^(\*\s*\*\s*\*|-\s*-\s*-|_\s*_\s*_)[\s*_-]*$"))
        {
            output.Write(new Rule { Style = Style.Parse("grey") });
            return;
        }

        // Aufzählung: - text, * text, + text
        Match bullet = Regex.Match(trimmed, @"^[-*+]\s+(.*)$");
        if (bullet.Success)
        {
            WriteMarkupLine($"{indent}  [deepskyblue1]•[/] {FormatInline(bullet.Groups[1].Value)}", line);
            return;
        }

        // Nummerierte Liste: 1. text
        Match numbered = Regex.Match(trimmed, @"^(\d+)[.)]\s+(.*)$");
        if (numbered.Success)
        {
            WriteMarkupLine($"{indent}  [deepskyblue1]{numbered.Groups[1].Value}.[/] {FormatInline(numbered.Groups[2].Value)}", line);
            return;
        }

        // Zitat: > text
        if (trimmed.StartsWith(">"))
        {
            string quote = trimmed.TrimStart('>').TrimStart();
            WriteMarkupLine($"[grey]│[/] [italic grey]{FormatInline(quote)}[/]", line);
            return;
        }

        // Normaler Text
        WriteMarkupLine($"{indent}{FormatInline(trimmed)}", line);
    }

    // Wandelt **fett**, *kursiv*, `code` und [Link](url) innerhalb einer Zeile um.
    private static string FormatInline(string text)
    {
        // Gemini schreibt manchmal LaTeX wie $\rightarrow$ -> in echte Zeichen umwandeln
        text = Regex.Replace(text, @"\$\\(rightarrow|to|Rightarrow|leftarrow|Leftarrow|times|cdot|le|leq|ge|geq|neq|approx|pm)\$", m => m.Groups[1].Value switch
        {
            "rightarrow" or "to" => "→",
            "Rightarrow" => "⇒",
            "leftarrow" => "←",
            "Leftarrow" => "⇐",
            "times" => "×",
            "cdot" => "·",
            "le" or "leq" => "≤",
            "ge" or "geq" => "≥",
            "neq" => "≠",
            "approx" => "≈",
            "pm" => "±",
            _ => m.Value
        });

        // Inline-Code zuerst abtrennen, damit darin nichts als fett/kursiv erkannt wird.
        // Split mit Klammer im Regex behält die `code`-Teile in der Liste.
        string[] parts = Regex.Split(text, @"(`[^`]+`)");
        var result = new System.Text.StringBuilder();

        foreach (string part in parts)
        {
            if (part.Length >= 2 && part.StartsWith("`") && part.EndsWith("`"))
            {
                string code = part.Substring(1, part.Length - 2);
                result.Append($"[orange1]{Markup.Escape(code)}[/]");
                continue;
            }

            // WICHTIG: zuerst escapen! Sonst hält Spectre eckige Klammern aus
            // Geminis Text für Markup und stürzt ab.
            string escaped = Markup.Escape(part);

            // Links: aus [text](url) wurde durch Escape [[text]](url)
            escaped = Regex.Replace(escaped, @"\[\[(.+?)\]\]\((https?://[^)\s]+)\)",
                m => $"[link={m.Groups[2].Value}]{m.Groups[1].Value}[/]");

            escaped = Regex.Replace(escaped, @"\*\*(.+?)\*\*", "[bold]$1[/]");              // **fett**
            escaped = Regex.Replace(escaped, @"__(.+?)__", "[bold]$1[/]");                  // __fett__
            escaped = Regex.Replace(escaped, @"(?<![\w*])\*(?!\s)(.+?)(?<!\s)\*(?![\w*])", "[italic]$1[/]"); // *kursiv*
            escaped = Regex.Replace(escaped, @"(?<!\w)_(?!\s)(.+?)(?<!\s)_(?!\w)", "[italic]$1[/]");         // _kursiv_
            escaped = Regex.Replace(escaped, @"~~(.+?)~~", "[strikethrough]$1[/]");         // ~~durchgestrichen~~

            result.Append(escaped);
        }

        return result.ToString();
    }

    private static void RenderCodeBlock(List<string> codeLines, string language)
    {
        string code = string.Join("\n", codeLines);

        Panel panel = new Panel(new Text(code, new Style(Color.Orange1)))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Grey),
            Padding = new Padding(1, 0, 1, 0),
        };

        if (!string.IsNullOrWhiteSpace(language))
        {
            panel.Header = new PanelHeader($"[grey] {Markup.Escape(language)} [/]");
        }

        output.Write(panel);
    }

    // Gibt eine gesammelte Markdown-Tabelle als Spectre-Tabelle aus
    private static void FlushTable(List<string> tableLines)
    {
        if (tableLines.Count == 0)
        {
            return;
        }

        // Zellen einer Zeile: | a | b | -> ["a", "b"]
        List<string[]> rows = tableLines
            .Select(l => l.Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray())
            .ToList();

        // Die Trennzeile |---|---| überspringen
        rows = rows.Where(r => !r.All(c => Regex.IsMatch(c, @"^:?-{2,}:?$"))).ToList();

        if (rows.Count == 0)
        {
            tableLines.Clear();
            return;
        }

        Table table = new Table { Border = TableBorder.Rounded, BorderStyle = new Style(Color.Grey) };

        foreach (string header in rows[0])
        {
            table.AddColumn(new TableColumn(SafeMarkup($"[bold]{FormatInline(header)}[/]", header)));
        }

        foreach (string[] row in rows.Skip(1))
        {
            // Zeile auf die Anzahl Spalten auffüllen bzw. kürzen
            IEnumerable<IRenderable> cells = Enumerable.Range(0, rows[0].Length)
                .Select(i => SafeMarkup(i < row.Length ? FormatInline(row[i]) : "", i < row.Length ? row[i] : ""));
            table.AddRow(cells);
        }

        output.Write(table);
        tableLines.Clear();
    }

    // Falls die Umwandlung kaputtes Markup erzeugt (z.B. bei verschachteltem **fett *kursiv**),
    // würde Spectre abstürzen. Dann geben wir die Zeile einfach als normalen Text aus.
    private static void WriteMarkupLine(string markup, string plainFallback)
    {
        try
        {
            output.MarkupLine(markup);
        }
        catch (Exception)
        {
            output.WriteLine(plainFallback);
        }
    }

    private static IRenderable SafeMarkup(string markup, string plainFallback)
    {
        try
        {
            return new Markup(markup);
        }
        catch (Exception)
        {
            return new Text(plainFallback);
        }
    }
}
