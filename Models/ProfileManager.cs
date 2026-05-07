using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace CrosshairOverlay.Models;

public class AppState
{
    public string ActiveProfile { get; set; } = "Default";
    public Dictionary<string, CrosshairConfig> Profiles { get; set; } = new() 
    { 
        { "Default", new CrosshairConfig() } 
    };
}

public static class ProfileManager
{
    private static readonly string ConfigPath = "appstate.json";

    public static void Save(AppState state)
    {
        try
        {
            string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
        catch { }
    }

    public static AppState Load()
    {
        if (File.Exists(ConfigPath))
        {
            try
            {
                string json = File.ReadAllText(ConfigPath);
                var state = JsonSerializer.Deserialize<AppState>(json);
                if (state != null && state.Profiles.Count > 0)
                {
                    if (!state.Profiles.ContainsKey(state.ActiveProfile)) 
                        state.ActiveProfile = state.Profiles.Keys.First();
                    return state;
                }
            }
            catch { }
        }
        
        var legacyPath = "config.json";
        if (File.Exists(legacyPath))
        {
            try {
                var json = File.ReadAllText(legacyPath);
                var conf = JsonSerializer.Deserialize<CrosshairConfig>(json);
                if (conf != null) {
                    var s = new AppState();
                    s.Profiles["Default"] = conf;
                    return s;
                }
            } catch { }
        }

        return new AppState();
    }
}
