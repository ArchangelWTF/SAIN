using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared;

[DataContract]
public class DifficultySettings : SAINSettingsBase<DifficultySettings>, ISAINSettings
{
    [DataMember]
    [Name("Vision Distance Multiplier")]
    [Description("Higher is more difficult.")]
    [DifficultyModAttribute]
    public float VisibleDistCoef = 1f;

    [DataMember]
    [Name("Vision Speed Multiplier")]
    [Description("Higher Is More Difficult. A value of 2 means bots will spot enemies twice as fast.")]
    [DifficultyModAttribute]
    public float GainSightCoef = 1f;

    [DataMember]
    [Name("Scatter Multiplier")]
    [Description("Lower is more difficult.")]
    [DifficultyModAttribute]
    public float ScatteringCoef = 1f;

    [DataMember]
    [Name("Hearing Distance Multiplier")]
    [Description("Higher is more difficult.")]
    [DifficultyModAttribute]
    public float HearingDistanceCoef = 1f;

    [DataMember]
    [Name("Aggression Multiplier")]
    [Description(
        "Higher is more difficult. Affects how long bots wait before entering search and how long they stand their ground to return fire when spotting an enemy."
    )]
    [DifficultyModAttribute]
    public float AggressionCoef = 1f;

    [DataMember]
    [Name("Precision Speed Multiplier")]
    [Description("Higher is more difficult.")]
    [DifficultyModAttribute]
    public float PRECISION_SPEED_COEF = 1f;

    [DataMember]
    [Name("Accuracy Speed Multiplier")]
    [Description("Lower is more difficult. Affects how long it takes for a bot to line up a shot when aiming.")]
    [DifficultyModAttribute]
    public float ACCURACY_SPEED_COEF = 1f;

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }
}
