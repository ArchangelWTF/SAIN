using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.BotSettings.SAINSettings.Categories;

[DataContract]
public class SAINScatterSettings : SAINSettingsBase<SAINScatterSettings>, ISAINSettings
{
    [DataMember]
    [Name("Arm Injury Scatter Multiplier")]
    [Description("Increase scatter when a bots arms are injured.")]
    [MinMax(1f, 5f, 100f)]
    [Advanced]
    public float HandDamageScatteringMinMax = 1.5f;

    [DataMember]
    [Name("Arm Injury Aim Speed Multiplier")]
    [Description("Increase scatter when a bots arms are injured.")]
    [MinMax(1f, 5f, 100f)]
    [Advanced]
    public float HandDamageAccuracySpeed = 1.5f;

    [IgnoreDataMember]
    [Hidden]
    public float DIST_NOT_TO_SHOOT = 0f;

    [IgnoreDataMember]
    [Hidden]
    public float FromShot = 0.002f;
}
