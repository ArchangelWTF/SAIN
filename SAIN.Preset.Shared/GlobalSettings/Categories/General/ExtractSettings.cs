using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.General;

[DataContract]
public class ExtractSettings : SAINSettingsBase<ExtractSettings>, ISAINSettings
{
    [DataMember]
    [Name("SAIN Extract Behavior")]
    [Description("REQUIRES GAME RESTART. Disable vanilla bot extract behavior and use SAIN decision making instead.")]
    public bool SAIN_EXTRACT_TOGGLE = false;
}
