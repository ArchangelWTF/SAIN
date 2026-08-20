using System.Runtime.Serialization;

namespace SAIN.Preset.Shared.Models;

[DataContract]
public class SuppressionConfig
{
    public bool IsActive(float num)
    {
        return num >= Threshold;
    }

    [DataMember]
    public float Threshold;

    [DataMember]
    public float PrecisionSpeedCoef;

    [DataMember]
    public float AccuracySpeedCoef;

    [DataMember]
    public float VisibleDistCoef;

    [DataMember]
    public float GainSightCoef;

    [DataMember]
    public float ScatteringCoef;

    [DataMember]
    public float HearingDistCoef;
}
