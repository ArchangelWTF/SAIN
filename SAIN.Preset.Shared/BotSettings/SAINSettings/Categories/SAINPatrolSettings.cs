using System.Runtime.Serialization;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.BotSettings.SAINSettings.Categories;

[DataContract]
public class SAINPatrolSettings : SAINSettingsBase<SAINPatrolSettings>, ISAINSettings { }
