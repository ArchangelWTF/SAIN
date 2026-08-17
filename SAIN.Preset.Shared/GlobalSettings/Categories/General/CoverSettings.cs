using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.General;

[DataContract]
public class CoverSettings : SAINSettingsBase<CoverSettings>, ISAINSettings
{
    [DataMember]
    [MinMax(20f, 100f, 10f)]
    [Advanced]
    public float MaxCoverPathLength = 60f;

    [DataMember]
    [MinMax(1f, 30f, 1f)]
    [Advanced]
    public float ShiftCoverChangeDecisionTime = 6f;

    [DataMember]
    [MinMax(2f, 120f, 1f)]
    [Advanced]
    public float ShiftCoverTimeSinceSeen = 30f;

    [DataMember]
    [MinMax(1f, 120f, 1f)]
    [Advanced]
    public float ShiftCoverTimeSinceEnemyCreated = 30f;

    [DataMember]
    [MinMax(1f, 120f, 1f)]
    [Advanced]
    public float ShiftCoverNoEnemyResetTime = 10f;

    [DataMember]
    [MinMax(1f, 120f, 1f)]
    [Advanced]
    public float ShiftCoverNewCoverTime = 10f;

    [DataMember]
    [MinMax(1f, 120f, 1f)]
    [Advanced]
    public float ShiftCoverResetTime = 10f;

    [DataMember]
    [MinMax(0.5f, 1.5f, 100f)]
    [Advanced]
    public float CoverMinHeight = 0.75f;

    [DataMember]
    [MinMax(0f, 30f, 1f)]
    [Advanced]
    public float CoverMinEnemyDistance = 8f;

    [DataMember]
    [Advanced]
    public bool DebugCoverFinder = false;
}
