using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.Enums;

namespace SAIN.Preset.Shared.BotSettings.SAINSettings;

[DataContract]
public class SAINSettingsGroupClass
{
    public SAINSettingsGroupClass() { }

    public SAINSettingsGroupClass(ESainBotDifficulty[] difficulties)
    {
        foreach (var difficulty in difficulties)
        {
            Settings.Add(difficulty, new SAINSettingsClass());
        }
    }

    public void Init()
    {
        foreach (var settings in Settings.Values)
        {
            settings.Init();
        }
    }

    public void UpdateDefaults()
    {
        foreach (var settings in Settings.Values)
        {
            settings.UpdateDefaults();
        }
    }

    [DataMember]
    public string Name;

    [DataMember]
    public ESainWildSpawnType WildSpawnType;

    [DataMember]
    [NameAndDescription(
        "Difficulty Modifier",
        "How much to improve this bot type's recoil handling, fire-rate, and full auto burst length, reaction time, general stats that are used in SAIN."
    )]
    [DefaultValue(0.5f)]
    [MinMax(0.01f, 1f)]
    public float DifficultyModifier = 0.5f;

    [DataMember]
    public Dictionary<ESainBotDifficulty, SAINSettingsClass> Settings = new();
}
