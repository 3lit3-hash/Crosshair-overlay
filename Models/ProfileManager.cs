using System.IO;
using System.Text.Json;

namespace CrosshairOverlay.Models;

public static class ProfileManager
{
    private static readonly string ConfigPath = "config.json";

    public static void Save(CrosshairConfig config)
    {
        var json = JsonSerializer.Serialize(config);
        File.WriteAllText(ConfigPath, json);
    }

    public static CrosshairConfig Load()
    {
        if (!File.Exists(ConfigPath)) return new CrosshairConfig();
        var json = File.ReadAllText(ConfigPath);
        return JsonSerializer.Deserialize<CrosshairConfig>(json) ?? new CrosshairConfig();
    }
}
