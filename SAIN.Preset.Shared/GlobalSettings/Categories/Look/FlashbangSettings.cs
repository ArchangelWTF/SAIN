using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.Look;

[DataContract]
public class FlashbangSettings : SAINSettingsBase<FlashbangSettings>, ISAINSettings
{
    [DataMember]
    [Name("Flash Duration Multiplier")]
    [Description("Scales how long bots stay blinded")]
    [MinMax(0.5f, 3f, 100f)]
    public float DurationMultiplier = 1.2f;

    [DataMember]
    [Name("Night Vision Multiplier")]
    [Description("Extra blindness for bots wearing night vision when the flash goes off.")]
    [MinMax(1f, 4f, 100f)]
    public float NightVisionMultiplier = 2f;

    [DataMember]
    [Name("Max Flash Duration")]
    [Description("Ceiling in seconds, applied after every other multiplier. Night vision is doubled")]
    [MinMax(2f, 30f, 10f)]
    public float MaxDuration = 30f;

    [DataMember]
    [Name("Look Around Chance")]
    [Description(
        "Chance a flashed bot spends the next moment looking around at nothing in particular."
    )]
    [Percentage]
    public float SearchChance = 45f;

    [DataMember]
    [Name("Track Chance")]
    [Description("Chance a flashed bot turns toward where it last saw its enemy without firing.")]
    [Percentage]
    public float TrackChance = 30f;

    [DataMember]
    [Name("Panic Movement Speed")]
    [Description("How fast a flashed bot staggers around.")]
    [MinMax(0.1f, 1f, 100f)]
    public float PanicMovementSpeed = 0.45f;

    [DataMember]
    [Name("Panic Movement")]
    [Description("Flashed bots stagger around instead of standing still.")]
    public bool PanicMovement = true;

    [DataMember]
    [Name("Blind Fire Delay")]
    [Description("Seconds before a flashed bot will fire at where it last saw its enemy.")]
    [MinMax(0f, 20f, 100f)]
    public float BlindFireDelay = 4f;

    [DataMember]
    [Name("Partial Recovery Point")]
    [Description(
        "The blinding effect is applied twice, and the second copy expires at this fraction of the duration. Bots shoot noticeably straighter past this point."
    )]
    [Percentage0to1]
    public float RecoveryPoint = 0.7f;
}
