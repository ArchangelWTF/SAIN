using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.BotSettings.SAINSettings;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.GearStealthValues;
using SAIN.Preset.Shared.GlobalSettings;
using SAIN.Preset.Shared.Models.Preset.Personalities;
using SAIN.Preset.Shared.Personalities.BasePersonality;

namespace SAIN.Preset.Shared.Preset;

[DataContract]
public sealed class SAINPresetBundle
{
    [DataMember]
    public SAINPresetDefinition Info;

    [DataMember]
    public GlobalSettingsClass GlobalSettings;

    [DataMember]
    public Dictionary<ESainWildSpawnType, SAINSettingsGroupClass> BotSettings = [];

    [DataMember]
    public Dictionary<EPersonality, PersonalitySettingsClass> Personalities = [];

    [DataMember]
    public Dictionary<EEquipmentType, List<ItemStealthValue>> GearStealthValues = [];
}
