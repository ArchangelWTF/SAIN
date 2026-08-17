using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.General;

[DataContract]
public class PerformanceSettings : SAINSettingsBase<PerformanceSettings>, ISAINSettings
{
    [DataMember]
    [Name("Performance Mode")]
    [Description(
        "Limits the cover finder to maximize performance. Reduces frequency on some raycasts. "
            + "If your PC is CPU limited, this might let you regain some frames lost while using SAIN. Can cause bots to take too long to find cover to go to."
    )]
    public bool PerformanceMode = false;
}
