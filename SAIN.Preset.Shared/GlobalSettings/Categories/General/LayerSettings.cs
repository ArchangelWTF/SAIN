using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.General;

[DataContract]
public class LayerSettings : SAINSettingsBase<LayerSettings>, ISAINSettings
{
    [DataMember]
    [Description("Requires Restart. Dont touch unless you know what this is")]
    [DeveloperOption]
    [MinMax(0, 100)]
    public int SAINCombatSquadLayerPriority = 22;

    [DataMember]
    [Description("Requires Restart. Dont touch unless you know what this is")]
    [DeveloperOption]
    [MinMax(0, 100)]
    public int SAINExtractLayerPriority = 24;

    [DataMember]
    [Description("Requires Restart. Dont touch unless you know what this is")]
    [DeveloperOption]
    [MinMax(0, 100)]
    public int SAINCombatSoloLayerPriority = 20;
}
