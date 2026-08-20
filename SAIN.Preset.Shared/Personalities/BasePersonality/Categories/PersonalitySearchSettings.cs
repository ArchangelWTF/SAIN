using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.Personalities.BasePersonality.Categories;

[DataContract]
public class PersonalitySearchSettings : SAINSettingsBase<PersonalitySearchSettings>, ISAINSettings
{
    [DataMember]
    [Advanced]
    public bool WillSearchForEnemy = true;

    [DataMember]
    [Advanced]
    public bool WillSearchFromAudio = true;

    [DataMember]
    [Name("Heard From Peace Behavior")]
    [Description("When a bot hears an enemy, and was previously at peace, so had no enemy and was in patrol, what is their reaction?")]
    public EHeardFromPeaceBehavior HeardFromPeaceBehavior = EHeardFromPeaceBehavior.Freeze;

    [DataMember]
    [Description("Will this personality type hear and chase after distant gunshots if they aren't fired at them?")]
    public bool WillChaseDistantGunshots = true;

    [DataMember]
    [Description(
        "If a sound is further than this, it will be considered chasing a gunshot sound, and will be ignored if WillChaseDistantGunshots is set to off, unless the gunshot is fired at them."
    )]
    public float AudioStraightDistanceToIgnore = 100f;

    [DataMember]
    [Name("Start Search Base Time")]
    [Description("The base time, before modifiers, that a personality will usually start searching for their enemy.")]
    [MinMax(0.1f, 500f)]
    public float SearchBaseTime = 40;

    [DataMember]
    [Name("Search Wait Multiplier")]
    [Description("Linearly increases or decreases the time a bot pauses while searching.")]
    [MinMax(0.01f, 5f, 100)]
    public float SearchWaitMultiplier = 1f;

    [DataMember]
    [Percentage]
    public float SprintWhileSearchChance = 25f;

    [DataMember]
    [Advanced]
    public bool Sneaky = false;

    [DataMember]
    [Percentage0to1]
    [Advanced]
    public float SneakySpeed = 1f;

    [DataMember]
    [Percentage0to1]
    [Advanced]
    public float SneakyPose = 1f;

    [DataMember]
    [Advanced]
    public bool SlowAtCorners = true;
}
