using System;
using System.Collections.Generic;
using EFT;
using Newtonsoft.Json;
using SAIN.Extensions;
using SAIN.Preset.Server;
using SAIN.Preset.Shared.BotSettings;

namespace SAIN.Preset;

public sealed class BotType
{
    public string Name;
    public string Description;
    public string Section;
    public WildSpawnType WildSpawnType;
    public List<string> BrainsToApply;
    public List<string> LayersToRemove;
    public string BaseBrain;
}

public class BotTypeDefinitions
{
    public static Dictionary<WildSpawnType, BotType> BotTypes { get; private set; } = [];
    public static List<BotType> BotTypesList { get; private set; }
    public static List<string> BotTypesNames { get; private set; } = [];

    static BotTypeDefinitions()
    {
        BotTypesList = ImportBotTypes();

        for (int i = 0; i < BotTypesList.Count; i++)
        {
            BotType botType = BotTypesList[i];
            WildSpawnType wildSpawn = botType.WildSpawnType;

            if (BotTypes.ContainsKey(wildSpawn))
            {
                continue;
            }

            BotTypesNames.Add(botType.Name);
            BotTypes.Add(wildSpawn, botType);
        }
    }

    public static List<BotType> ImportBotTypes()
    {
        try
        {
            string json = ServerDataClient.GetBotTypes();

            if (!string.IsNullOrEmpty(json))
            {
                var shared = JsonConvert.DeserializeObject<List<BotTypeInfo>>(json);
                if (shared != null && shared.Count > 0)
                {
                    return ToBotTypes(shared);
                }
            }

            Logger.LogError("[SAIN] Server returned no bot-type registry; SAIN cannot run without the server mod!");
        }
        catch (Exception ex)
        {
            Logger.LogError($"[SAIN] Failed to import bot types from server: {ex.Message}");
        }

        return [];
    }

    private static List<BotType> ToBotTypes(List<BotTypeInfo> shared)
    {
        var list = new List<BotType>(shared.Count);

        foreach (BotTypeInfo info in shared)
        {
            list.Add(
                new BotType
                {
                    Name = info.Name,
                    Description = info.Description,
                    Section = info.Section,
                    WildSpawnType = info.WildSpawnType.ToEft(),
                    BrainsToApply = info.BrainsToApply,
                    LayersToRemove = info.LayersToRemove,
                    BaseBrain = info.BaseBrain,
                }
            );
        }

        return list;
    }
}
