using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.Look.VisionSpeed;

[DataContract]
public class VisionSpeedSettings : SAINSettingsBase<VisionSpeedSettings>, ISAINSettings
{
    [DataMember]
    public ElevationVisionSettings Elevation = new();

    [DataMember]
    public MovementVisibilitySettings Movement = new();

    [DataMember]
    public PartsVisibilitySettings PartsVisibility = new();

    [DataMember]
    public PeripheralVisionSettings Peripheral = new();

    [DataMember]
    public PoseVisibilitySettings Pose = new();

    [DataMember]
    public ThirdPartySettings ThirdParty = new();

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
        list.Add(Elevation);
        list.Add(Movement);
        list.Add(PartsVisibility);
        list.Add(Peripheral);
        list.Add(Pose);
        list.Add(ThirdParty);
    }
}

[DataContract]
public class PeripheralVisionSettings : SAINSettingsBase<PeripheralVisionSettings>, ISAINSettings
{
    [DataMember]
    public string Description =
        "Adds additional vision speed reduction to targets in a bot's peripheral vision."
        + "Scales with the angle from their look direction.";

    [DataMember]
    public bool Enabled = true;

    [DataMember]
    [MinMax(5f, 60f, 1f)]
    [Advanced]
    public float PERIPHERAL_VISION_START_ANGLE = 30;

    [DataMember]
    [MinMax(1f, 3f, 100f)]
    [Advanced]
    public float PERIPHERAL_VISION_MAX_REDUCTION_COEF = 2f;
}

[DataContract]
public class ThirdPartySettings : SAINSettingsBase<ThirdPartySettings>, ISAINSettings
{
    [DataMember]
    public string Description =
        "When an enemy is a certain angle away from their active enemies last known position, "
        + "this will reduce their vision speed of that target up to the maximum set amount.";

    [DataMember]
    public bool Enabled = true;

    [DataMember]
    [MinMax(5f, 60f, 1f)]
    [Advanced]
    public float THIRDPARTY_VISION_START_ANGLE = 30;

    [DataMember]
    [MinMax(1f, 3f, 100f)]
    [Advanced]
    public float THIRDPARTY_VISION_MAX_COEF = 1.5f;
}

[DataContract]
public class PartsVisibilitySettings : SAINSettingsBase<PartsVisibilitySettings>, ISAINSettings
{
    [DataMember]
    public string Description =
        "Scales vision speed based on the number of body parts that are within line of sight to their enemy. "
        + "Only applies to Non-AI targets.";

    [DataMember]
    public bool Enabled = true;

    [DataMember]
    [MinMax(1f, 3f, 100f)]
    [Advanced]
    public float PARTS_VISIBLE_MAX_COEF = 2f;

    [DataMember]
    [MinMax(0.25f, 1f, 100f)]
    [Advanced]
    public float PARTS_VISIBLE_MIN_COEF = 0.9f;
}

[DataContract]
public class MovementVisibilitySettings : SAINSettingsBase<MovementVisibilitySettings>, ISAINSettings
{
    [DataMember]
    public string Description =
        "Scales vision speed based on the movement speed of their enemy. " + "Faster movement = faster vision speed.";

    [DataMember]
    public bool Enabled = true;

    [DataMember]
    [Name("Movement Vision Modifier")]
    [Description(
        "Bots will see moving players this much faster, at any range."
            + "Higher is slower speed, so 0.66 would result in bots spotting an enemy who is moving 0.66x faster. So if they usually would take 10 seconds to spot someone, it would instead take around 6.6 seconds."
    )]
    [MinMax(0.01f, 1f, 100f)]
    [Advanced]
    public float MOVEMENT_VISION_MULTIPLIER = 0.5f;
}

[DataContract]
public class PoseVisibilitySettings : SAINSettingsBase<PoseVisibilitySettings>, ISAINSettings
{
    [DataMember]
    public string Description = "Scales vision speed based on the pose of their enemy. " + "Only applies to Non-AI targets.";

    [DataMember]
    public bool Enabled = true;

    [DataMember]
    [MinMax(1f, 3f, 100f)]
    [Advanced]
    public float PRONE_VISION_SPEED_COEF = 1.75f;

    [DataMember]
    [MinMax(1f, 3f, 100f)]
    [Advanced]
    public float DUCK_VISION_SPEED_COEF = 1.25f;
}
