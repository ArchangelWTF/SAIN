using System.Collections.Generic;
using SPT.Common.Http;
using SPT.Common.Utils;

namespace SAIN.Preset.Server;

public static class ServerPresetClient
{
    public static string GetGeneratedDefaults()
    {
        return RequestHandler.GetJson("/sain/presets/defaults");
    }

    public static List<string> ListCustom()
    {
        string json = RequestHandler.GetJson("/sain/presets/custom/list");
        return Json.Deserialize<List<string>>(json) ?? new List<string>();
    }

    public static string GetCustom(string name)
    {
        string body = Json.Serialize(new PresetNameBody { Name = name });
        string json = RequestHandler.PutJson("/sain/presets/custom/get", body);
        return json == "null" ? null : json;
    }

    public static void SaveCustom(string name, string presetJson)
    {
        string body = Json.Serialize(new PresetSaveBody { Name = name, PresetJson = presetJson });
        RequestHandler.PutJson("/sain/presets/custom/save", body);
    }

    public static bool DeleteCustom(string name)
    {
        string body = Json.Serialize(new PresetNameBody { Name = name });
        string json = RequestHandler.PutJson("/sain/presets/custom/delete", body);
        return json != null && json.Contains("true");
    }

    private sealed class PresetNameBody
    {
        public string Name;
    }

    private sealed class PresetSaveBody
    {
        public string Name;
        public string PresetJson;
    }
}
