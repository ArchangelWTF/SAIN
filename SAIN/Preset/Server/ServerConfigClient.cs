using System;
using SPT.Common.Http;
using SPT.Common.Utils;

namespace SAIN.Preset.Server;

public static class ServerConfigClient
{
    public static string ForcedPresetName { get; private set; }
    public static bool EditingAllowed { get; private set; } = true;

    public static bool ForcedActive
    {
        get { return !string.IsNullOrEmpty(ForcedPresetName); }
    }

    /// <summary>Selection is locked (players can't switch presets) when the server forces one.</summary>
    public static bool PresetLocked
    {
        get { return ForcedActive; }
    }

    public static void Fetch()
    {
        try
        {
            string json = RequestHandler.GetJson("/sain/config");
            if (string.IsNullOrEmpty(json) || json == "null")
            {
                return;
            }

            var config = Json.Deserialize<ClientConfig>(json);
            if (config != null)
            {
                ForcedPresetName = config.ForcedPresetName;
                EditingAllowed = config.EditingAllowed;
                Logger.LogInfo($"[SAIN] Server config: forced='{ForcedPresetName}', editingAllowed={EditingAllowed}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"[SAIN] Failed to fetch server config: {ex.Message}");
        }
    }

    private sealed class ClientConfig
    {
        public string ForcedPresetName;
        public bool EditingAllowed = true;
    }
}
