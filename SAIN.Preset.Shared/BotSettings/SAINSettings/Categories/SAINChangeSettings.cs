using System.Runtime.Serialization;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.BotSettings.SAINSettings.Categories;

[DataContract]
public class SAINChangeSettings : SAINSettingsBase<SAINChangeSettings>, ISAINSettings { }
