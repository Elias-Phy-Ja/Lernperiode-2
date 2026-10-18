using System.Text.Json;
using Shellmate.Models;

namespace Shellmate.Services;

public class ConfigService
{
    private static readonly string ConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Shellmate", "config.json");


    public static AppConfig? LoadConfig()
    {
        if (!File.Exists(ConfigFilePath))
        {
            // Wenn die Datei nicht existiert
            return null;
        }

        string json = File.ReadAllText(ConfigFilePath);
        return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
    }

    public static void SaveConfig(AppConfig config)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath)!);
        string json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigFilePath, json); // Konfiguration in die Datei schreiben
    }

    // Löscht die Config-Datei (für "Shellmate zurücksetzen")
    public static void DeleteConfig()
    {
        if (File.Exists(ConfigFilePath))
        {
            File.Delete(ConfigFilePath);
        }
    }
}
