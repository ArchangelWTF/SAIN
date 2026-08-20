using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.BotSettings.SAINSettings.Categories;

[DataContract]
public class SAINBossSettings : SAINSettingsBase<SAINBossSettings>, ISAINSettings
{
    [DataMember]
    [Hidden]
    public bool SET_CHEAT_VISIBLE_WHEN_ADD_TO_ENEMY = false;
}
