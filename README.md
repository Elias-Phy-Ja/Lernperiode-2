<div align="center">


# KI Chatbot in Terminal - Shellmate

**Um schnell Antworten auf Prompts zu kriegen**


</div>


Shellmate soll per eigener Befehl im Terminal startbar sein. Bei 1. Nutzung soll man begleitet werden durch den Prozess einen Gemini API Key zu generieren und einzutragen.

Bei 2.+ Aktivierung soll man direkt in den Chat kommen und durch `/Commands` spezielle Aktionen ausführen können wie z.B in Einstellungen zu kommen.

> [!NOTE]
> Random Number guesser als Übung unter [RandomNumber_guesser](src/Uebungen)

## Features

- KI Chat Bot im Terminal
- Verschlüsselter HTTPS Verkehr
- Komplette Markedown Library im Terminal verfügbar für Formatierungen.
- (ev. GUI wie Winforms)


## Changelog

### next session
- [x] Setup-Assistent: API Key beim 1. Start abfragen und in Config-Datei speichern (Etappe 3)
- [x] Chat-Verlauf, damit Gemini sich an das Gespräch erinnert (Etappe 4)
- [ ] Markdown-Ausgabe im Terminal mit Spectre.Console (Etappe 5)


### 18.10.2026
- [x] `AppConfig` (Model + ApiKey) als JSON in `%APPDATA%\Shellmate\config.json`
- [x] `ConfigService` zum Laden und Speichern der Config
- [x] Setup Wizard beim ersten Start (inkl. `/help-setup`)
- [x] Neue Befehle `/help-setup` und `/how-to-use`
- [x] Modell aus der Config statt fest im Code
- [x] Fehlerbehandlung bei Netzwerkfehlern und blockierten Antworten (`try`/`catch`)
- [x] Leerzeilen in der Ausgabe vereinheitlicht
- [x] Chat-Verlauf: Gemini erinnert sich an das Gespräch (`ChatMessage`, `history`)
- [x] `/clear`: Verlauf und Terminal leeren, Neustart des Chats
- [x] `/settings`: Modell und API Key ändern (ohne Neustart), Key maskiert angezeigt
- [x] Shellmate zurücksetzen mit Bestätigung
- [x] Chat wird nach den Einstellungen wiederhergestellt
- [x] `/exit` funktioniert auch im Setup Wizard


### 16.10.2026
- [x] Create Repository
- [x] Create Readme
- [x] Number guesser als Übung
- [x] Create .gitignore for API keys
- [x] Namen finden → **Shellmate**
- [x] Ordner Struktur anlegen (`src/Shellmate` mit `Commands`, `Models`, `Services`, `UI`) + Solution-Datei
- [x] Chat-Schleife mit Eingabe, Trim und Fallback bei leerer Eingabe
- [x] Commands `/help` und `/exit` (Gross-/Kleinschreibung egal)
- [x] `CommandHandler` in eigene Klasse ausgelagert
- [x] API Key als Umgebungsvariable `GEMINI_API_KEY` eingerichtet
- [x] Erster Request an die Gemini API (`HttpClient`, JSON mit `JsonSerializer`)
- [x] Antworttext aus dem JSON herausgelesen (`JsonDocument`)
- [x] Fehlerbehandlung bei fehlgeschlagenen Anfragen (z.B. 503)
- [x] `GeminiService` in eigene Klasse ausgelagert
