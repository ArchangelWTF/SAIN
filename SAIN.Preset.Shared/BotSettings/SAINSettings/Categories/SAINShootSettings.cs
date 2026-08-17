using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.BotSettings.SAINSettings.Categories;

[DataContract]
public class SAINShootSettings : SAINSettingsBase<SAINShootSettings>, ISAINSettings
{
    [DataMember]
    [Category("Recoil")]
    [NameAndDescription(
        "Recoil Scatter Multiplier",
        "Modifies how much recoil a bot gets from firing their weapon. " + "Higher = more recoil, more kick. 1.5 = 1.5x more recoil"
    )]
    [MinMax(0.25f, 3f, 100f)]
    public float RecoilMultiplier = 1f;

    [DataMember]
    [Category("Firerate")]
    [NameAndDescription(
        "Burst Length Multiplier",
        "Modifies how long bots shoot a burst during full auto fire. " + "Higher = longer full auto time. 1.5 = 1.5x longer bursts"
    )]
    [MinMax(0.25f, 3f, 100f)]
    public float BurstMulti = 1.5f;

    [DataMember]
    [Category("Firerate")]
    [NameAndDescription(
        "Semiauto Firerate Multiplier",
        "Modifies the time a bot waits between semiauto fire. " + "Higher = faster firerate. 1.5 = 1.5x more shots per second"
    )]
    [MinMax(0.25f, 3f, 100f)]
    public float FireratMulti = 1.5f;

    [DataMember]
    [Category("Aim")]
    [MinMax(50f, 500f, 1f)]
    public float MaxPointFireDistance = 150f;

    [DataMember]
    [Category("Firerate")]
    [Name("Full Auto Scatter Multiplier")]
    [MinMax(1f, 5f, 100f)]
    [Advanced]
    [CopyValue]
    public float AUTOMATIC_FIRE_SCATTERING_COEF = 1.4f;
}
