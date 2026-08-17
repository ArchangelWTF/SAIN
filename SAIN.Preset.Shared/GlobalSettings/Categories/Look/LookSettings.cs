using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings.Categories.Look.VisionDistance;
using SAIN.Preset.Shared.GlobalSettings.Categories.Look.VisionSpeed;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.Look;

[DataContract]
public class LookSettings : SAINSettingsBase<LookSettings>, ISAINSettings
{
    [DataMember]
    [Category("Core Settings")]
    [Name("Vision Speed Settings")]
    public VisionSpeedSettings VisionSpeed = new();

    [DataMember]
    [Category("Core Settings")]
    [Name("Vision Distance Settings")]
    public VisionDistanceSettings VisionDistance = new();

    [DataMember]
    [Category("Core Settings")]
    [Name("Time Settings")]
    public TimeSettings Time = new();

    [DataMember]
    [Category("Core Settings")]
    [Name("Flashlights and NVGs Settings")]
    public LightNVGSettings Light = new();

    [DataMember]
    [Category("Extra")]
    [Name("Not Looking At Bot Settings")]
    public NotLookingSettings NotLooking = new();

    public override void Init(List<ISAINSettings> list)
    {
        VisionSpeed.Init(list);
        list.Add(VisionDistance);
        list.Add(NotLooking);
        list.Add(Time);
        list.Add(Light);
    }
}
