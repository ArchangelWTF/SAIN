using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings;
using SAIN.Preset.Shared.GlobalSettings.Categories;

namespace SAIN.Preset.Shared.BotSettings.SAINSettings.Categories;

[DataContract]
public class SAINAimingSettings : SAINSettingsBase<SAINAimingSettings>, ISAINSettings
{
    [DataMember]
    [Category("Aim Target")]
    [Name("Override Global Part Weights")]
    [Description("Use this bot type's own aim target weights instead of the global ones.")]
    public bool OverrideAimTargetWeights = false;

    [DataMember]
    [Category("Aim Target")]
    [Name("Part Weights")]
    [Description("Only used when Override Global Part Weights is enabled.")]
    public AimTargetWeights AimTargetWeights = new();

    [DataMember]
    [Name("Can Aim for Headshots")]
    [Category("Aim Target")]
    public bool AimForHead = false;

    [DataMember]
    [Category("Aim Target")]
    [Name("Can Aim for Headshots - Percentage Chance")]
    [Percentage]
    public float AimForHeadChance = 33f;

    [DataMember]
    [Category("Time to Aim")]
    [Name("Distance Aim Time Multiplier")]
    [Description(
        "Multiplies the time a bot takes to aim based on distance. So higher values will cause bots to take longer to aim depending on distance."
    )]
    [MinMax(0.1f, 5f, 100f)]
    public float DistanceAimTimeMultiplier = 1f;

    [DataMember]
    [Category("Time to Aim")]
    [Name("Angle Aim Time Multiplier")]
    [Description(
        "Multiplies the time a bot takes to aim based on the angle they have to turn to line up a shot. So higher values will cause bots to take longer to aim depending on the angle to turn."
    )]
    [MinMax(0.1f, 5f, 100f)]
    public float AngleAimTimeMultiplier = 1f;

    [DataMember]
    [Category("Time to Aim")]
    [Name("Faster CQB Reactions")]
    [Description("Sets whether this bot reacts and aims faster before being able to shoot at close ranges")]
    public bool FasterCQBReactions = true;

    [DataMember]
    [Category("Time to Aim")]
    [Name("Faster CQB Reactions Max Distance")]
    [Description(
        "Max distance a bot can react faster for Faster CQB Reactions. Scales with distance."
            + "Example: If Max distance is set to 20 meters, and an enemy is 10 meters away. they will react 2x as fast as usual, "
            + "or if an enemy is 15 meters away, they will react 1.5x as fast as usual. "
            + "If the enemy is at 20 meters or further, nothing will happen."
    )]
    [NameAndDescription(
        "Faster CQB Reactions Max Distance",
        "Max distance a bot can react faster for Faster CQB Reactions. Scales with distance."
    )]
    [MinMax(5f, 100f)]
    public float FasterCQBReactionsDistance = 30f;

    [DataMember]
    [Category("Time to Aim")]
    [Name("Faster CQB Reactions Minimum Speed")]
    [Description("Absolute minimum speed (in seconds) that bot can react and shoot")]
    [MinMax(0.05f, 0.75f, 100f)]
    public float FasterCQBReactionsMinimum = 0.33f;

    //[Name("Accuracy Spread Multiplier")]
    //[Description("Higher = less accurate. Modifies a bot's base accuracy and spread. 1.5 = 1.5x higher accuracy spread")]
    //[MinMax(0.1f, 10f, 10f)]
    //public float AccuracySpreadMulti = 1f;

    [DataMember]
    [Category("Time to Aim")]
    [Name("Max Aiming Upgrade By Time")]
    [Description(
        "VANILLA EFT CONFIG VALUE : Lower = Better. How much to cap this bots aim improvement based on time. 0.25 means they will be able to multiply their aim offset by 0.25x when aiming. Making their shots more accurate"
    )]
    [MinMax(0.01f, 0.99f, 100f)]
    [Advanced]
    [CopyValue]
    public float MAX_AIMING_UPGRADE_BY_TIME = 0.25f;

    [DataMember]
    [Category("Scatter Modifiers")]
    [Name("Distance To Not Scatter Aim")]
    [Description("VANILLA EFT CONFIG VALUE : If an enemy is closer than this distance, ignore scatter.")]
    [MinMax(0.1f, 30f, 100f)]
    [Advanced]
    public float DIST_TO_SHOOT_NO_OFFSET = 3f;

    [DataMember]
    [Category("Scatter Modifiers")]
    [Name("VANILLA EFT CONFIG VALUE : Scatter Multiplier - Moving")]
    [MinMax(0.1f, 6f, 100f)]
    [Advanced]
    public float COEF_IF_MOVE = 1.5f;

    [Category("Time to Aim")]
    [Name("VANILLA EFT CONFIG VALUE : Aim Time Multiplier - Moving")]
    [Hidden]
    [IgnoreDataMember]
    public float TIME_COEF_IF_MOVE = 1.5f;

    [DataMember]
    [Category("Time to Aim")]
    [Name("Max Aim Time")]
    [Description("VANILLA EFT CONFIG VALUE :  A cap on how long it will take this bot to finish aiming at a target, and begin shooting.")]
    [MinMax(0.01f, 4f, 1000f)]
    [Advanced]
    [CopyValue]
    public float MAX_AIM_TIME = 2f;

    [Hidden]
    [IgnoreDataMember]
    public int AIMING_TYPE = 1;

    //[Name("Friendly Fire Spherecast Size")]
    //[Description("")]
    //[MinMax(0f, 0.5f, 100f)]
    //[Advanced]
    //public float SHPERE_FRIENDY_FIRE_SIZE = 0.15f;

    [DataMember]
    [Hidden]
    public float DAMAGE_TO_DISCARD_AIM_0_100 = 100;

    [DataMember]
    [Category("Time to Aim")]
    [NameAndDescription(
        "VANILLA EFT CONFIG VALUE : Hit Reaction Recovery Time",
        "How much time it takes to recover a bot's aim when they get hit by a bullet"
    )]
    [MinMax(0.1f, 0.99f, 100f)]
    [Advanced]
    public float BASE_HIT_AFFECTION_DELAY_SEC = 0.65f;

    [DataMember]
    [Category("Time to Aim")]
    [Name("VANILLA EFT CONFIG VALUE : Hit Aim Time Penalty Seconds - Min")]
    [MinMax(0f, 1f, 100f)]
    [Advanced]
    public float MIN_TIME_DISCARD_AIM_SEC = 0.5f;

    [DataMember]
    [Category("Time to Aim")]
    [Name("VANILLA EFT CONFIG VALUE : Hit Aim Time Penalty Seconds - Max")]
    [MinMax(0f, 2f, 100f)]
    [Advanced]
    public float MAX_TIME_DISCARD_AIM_SEC = 1.5f;

    [Hidden]
    [IgnoreDataMember]
    public float ANY_PART_SHOOT_TIME = 2f;

    [DataMember]
    [Category("Time to Aim")]
    [Name("VANILLA EFT CONFIG VALUE : Enemy First Contact Reaction Delay")]
    [MinMax(0f, 1f, 100f)]
    [Advanced]
    public float FIRST_CONTACT_ADD_SEC = 0.2f;

    [Hidden]
    [IgnoreDataMember]
    public float FIRST_CONTACT_ADD_CHANCE_100 = 100f;

    [Hidden]
    [IgnoreDataMember]
    public float OFFSET_RECAL_ANYWAY_TIME = 30f;

    //[Hidden]
    //[JsonIgnore]
    //public float RECALC_SQR_DIST = 2f * 2f;
}
