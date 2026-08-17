using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.Enums;
using SAIN.Preset.Shared.Models;
using SAIN.Preset.Shared.Models.Enums;
using SAIN.Preset.Shared.Models.Preset.Personalities;

namespace SAIN.Preset.Shared.GlobalSettings.Categories;

[DataContract]
public class MindSettings : SAINSettingsBase<MindSettings>, ISAINSettings
{
    public override void Update() { }

    [DataMember]
    [Name("Enemy Suppression Toggle")]
    [Category("Enemy Suppression")]
    public bool TARGET_SUPPRESS_TOGGLE = true;

    [DataMember]
    [Name("Force Single Personality For All Bots")]
    [Description("All Spawned SAIN bots will be assigned the selected Personality, if any are set to true, no matter what.")]
    [Category("Personality")]
    public Dictionary<EPersonality, bool> ForcePersonality = new()
    {
        { EPersonality.Wreckless, false },
        { EPersonality.GigaChad, false },
        { EPersonality.Chad, false },
        { EPersonality.SnappingTurtle, false },
        { EPersonality.Rat, false },
        { EPersonality.Coward, false },
        { EPersonality.Timmy, false },
        { EPersonality.Normal, false },
    };

    [DataMember]
    [Name("Boss Personalities")]
    [Description("Sets the pesonality that a boss will always use.")]
    [Category("Personality")]
    [Hidden]
    public Dictionary<ESainWildSpawnType, EPersonality> PERS_BOSSES = new()
    {
        { ESainWildSpawnType.bossKilla, EPersonality.Wreckless },
        { ESainWildSpawnType.bossKillaAgro, EPersonality.Wreckless },
        { ESainWildSpawnType.bossTagilla, EPersonality.Wreckless },
        { ESainWildSpawnType.bossTagillaAgro, EPersonality.Wreckless },
        { ESainWildSpawnType.bossKolontay, EPersonality.Wreckless },
        { ESainWildSpawnType.bossKnight, EPersonality.GigaChad },
        { ESainWildSpawnType.followerBigPipe, EPersonality.GigaChad },
        { ESainWildSpawnType.followerBirdEye, EPersonality.SnappingTurtle },
        { ESainWildSpawnType.bossGluhar, EPersonality.SnappingTurtle },
        { ESainWildSpawnType.bossKojaniy, EPersonality.Rat },
        { ESainWildSpawnType.bossPartisan, EPersonality.Rat },
        { ESainWildSpawnType.bossBully, EPersonality.Coward },
        { ESainWildSpawnType.bossSanitar, EPersonality.Coward },
        { ESainWildSpawnType.bossBoar, EPersonality.Coward },
        { ESainWildSpawnType.gifter, EPersonality.GigaChad },
    };

    [DataMember]
    [MinMax(0.1f, 5f, 100f)]
    [Category("Personality")]
    [Name("Global Aggression")]
    [Description("Higher = More aggressive bots, less time before seeking enemies. 2x = half the wait time.")]
    public float GlobalAggression = 1f;

    [DataMember]
    [Name("Bots can use Stealth Search")]
    [Description(
        "If a bot thinks he was not heard, and isn't currently fighting an enemy, they can decide to be stealthy while they seek out an enemy, if they are inside a building."
    )]
    [Category("Personality")]
    public bool SneakyBots = true;

    [DataMember]
    [Name("Only Sneaky Personalities can be Stealthy")]
    [Description(
        "Only allow sneaky personality types (rat, snapping turtle) to be stealthy while searching for an enemy, ignored if Stealth Search is disabled above"
    )]
    [Category("Personality")]
    public bool OnlySneakyPersonalitiesSneaky = true;

    [DataMember]
    [Description("The distance from a bot's search destination that they will begin to be stealthy, if enabled.")]
    [Category("Personality")]
    [Advanced]
    [MinMax(5f, 200f, 10f)]
    public float MaximumDistanceToBeSneaky = 80f;

    [DataMember]
    [Name("Bot Suppression")]
    [Description("Toggles whether bots get suppressed or not. If disabled, all options below will do nothing.")]
    [Category("Suppression")]
    public bool SUPP_TOGGLE = true;

    [DataMember]
    [Name("Suppression Distance Scale Start")]
    [Description("The distance between the bullet, and a bot's head to receive full suppression effect. In Meters.")]
    [Category("Suppression")]
    [MinMax(1f, 30f, 100f)]
    [Advanced]
    public float SUPP_DISTANCE_SCALE_START = 4f;

    [DataMember]
    [Name("Suppression Distance Scale End")]
    [Description(
        "The maximum distance between the bullet, and a bot's head to be considered Suppressing fire. In Meters. Scales linearly between Scale End and Scale Start."
    )]
    [Category("Suppression")]
    [MinMax(1f, 30f, 100f)]
    [Advanced]
    public float SUPP_DISTANCE_SCALE_END = 10f;

    [DataMember]
    [Name("Suppression Distance Amplify Distance")]
    [Description("If a bullet is closer than this distance, in meters, to the bot's head. Amplify the amount of suppression.")]
    [Category("Suppression")]
    [MinMax(0f, 5f, 100f)]
    [Advanced]
    public float SUPP_DISTANCE_AMP_DIST = 0.5f;

    [DataMember]
    [Name("Suppression Distance Amplify Amount")]
    [Description("If a bullet is closer than Amplify Distance to the bot's head. Amplify the amount of suppression by this multiplier.")]
    [Category("Suppression")]
    [MinMax(1f, 3f, 100f)]
    [Advanced]
    public float SUPP_DISTANCE_AMP_AMOUNT = 1.5f;

    [DataMember]
    [Description("The maximum distance between the bullet, and a bot's head to be considered under active enemy fire.")]
    [MinMax(0.1f, 20f, 100f)]
    [Category("Suppression")]
    [Advanced]
    public float MaxUnderFireDistance = 2f;

    [DataMember]
    [Hidden]
    [Name("Suppression States")]
    [Description("Configure each tier of suppression.")]
    [MinMax(0.01f, 10f, 100f)]
    [Category("Suppression")]
    [Advanced]
    public Dictionary<ESuppressionState, SuppressionConfig> SUPPRESSION_STATES = new()
    {
        {
            ESuppressionState.Light,
            new SuppressionConfig
            {
                Threshold = 1f,
                PrecisionSpeedCoef = 0.8f,
                AccuracySpeedCoef = 1.2f,
                GainSightCoef = 0.9f,
                ScatteringCoef = 1.35f,
                VisibleDistCoef = 0.85f,
                HearingDistCoef = 0.8f,
            }
        },
        {
            ESuppressionState.Medium,
            new SuppressionConfig
            {
                Threshold = 6f,
                PrecisionSpeedCoef = 0.75f,
                AccuracySpeedCoef = 1.5f,
                GainSightCoef = 0.75f,
                ScatteringCoef = 1.75f,
                VisibleDistCoef = 0.6f,
                HearingDistCoef = 0.6f,
            }
        },
        {
            ESuppressionState.Heavy,
            new SuppressionConfig
            {
                Threshold = 15f,
                PrecisionSpeedCoef = 0.5f,
                AccuracySpeedCoef = 2f,
                GainSightCoef = 0.65f,
                ScatteringCoef = 2.5f,
                VisibleDistCoef = 0.5f,
                HearingDistCoef = 0.4f,
            }
        },
        {
            ESuppressionState.Extreme,
            new SuppressionConfig
            {
                Threshold = 25f,
                PrecisionSpeedCoef = 0.25f,
                AccuracySpeedCoef = 3f,
                GainSightCoef = 0.5f,
                ScatteringCoef = 3f,
                VisibleDistCoef = 0.33f,
                HearingDistCoef = 0.25f,
            }
        },
    };

    [DataMember]
    [Name("Amount Multiplier")]
    [Description(
        "Linearly increase or decrease the amount of suppression points bots receive from 1 bullet. Higher = Bots get suppressed more easily."
    )]
    [MinMax(0.01f, 5f, 100f)]
    [Category("Suppression")]
    public float SUPP_AMOUNT_MULTI = 1f;

    [DataMember]
    [Name("Strength Multiplier")]
    [Description(
        "Linearly increase or decrease the strength of suppression effects on bots. Higher = Suppression has more effect on bot stats."
    )]
    [MinMax(0.01f, 5f, 100f)]
    [Category("Suppression")]
    public float SUPP_STRENGTH_MULTI = 1f;

    [DataMember]
    [Advanced]
    [Name("Decay Tick Amount")]
    [Description("How much suppression to remove per update tick.")]
    [MinMax(0.01f, 5f, 100f)]
    [Category("Suppression")]
    public float SUP_DECAY_AMOUNT = 0.25f;

    [DataMember]
    [Advanced]
    [Name("Decay Tick Frequency")]
    [Description("How often to tick decay per second. 0.25 = 4 per second")]
    [MinMax(0.01f, 1f, 100f)]
    [Category("Suppression")]
    public float SUP_DECAY_FREQ = 0.25f;

    [DataMember]
    [Advanced]
    [Name("State Update Tick Frequency")]
    [Description("How often to check suppression state per second. 0.5 = 2 per second")]
    [MinMax(0.01f, 1f, 100f)]
    [Category("Suppression")]
    public float SUP_CHECK_FREQ = 0.5f;

    [DataMember]
    [Advanced]
    [Name("Suppression Amounts Per Caliber")]
    [Description(
        "For each bullet that flies by a bot, add this number to their suppression counter, which decays constantly and linearly."
    )]
    [MinMax(0.1f, 20f, 100f)]
    [Category("Suppression")]
    [DefaultDictionary(nameof(SUPP_AMOUNTS_DEFAULT))]
    public Dictionary<string, float> SUPP_AMOUNTS = new()
    {
        { Calibers.Caliber20x1mm, 0.25f },
        { Calibers.Caliber9x18PM, 1f },
        { Calibers.Caliber9x19PARA, 1.1f },
        { Calibers.Caliber46x30, 1.2f },
        { Calibers.Caliber9x21, 1.25f },
        { Calibers.Caliber57x28, 1.3f },
        { Calibers.Caliber762x25TT, 1.4f },
        { Calibers.Caliber1143x23ACP, 1.5f },
        { Calibers.Caliber9x33R, 1.5f },
        { Calibers.Caliber545x39, 2.1f },
        { Calibers.Caliber556x45NATO, 2f },
        { Calibers.Caliber9x39, 2.5f },
        { Calibers.Caliber762x35, 2.4f },
        { Calibers.Caliber762x39, 2.5f },
        { Calibers.Caliber366TKM, 2.5f },
        { Calibers.Caliber68x51, 2.5f },
        { Calibers.Caliber762x51, 2.65f },
        { Calibers.Caliber127x55, 2.7f },
        { Calibers.Caliber762x54R, 2.75f },
        { Calibers.Caliber20g, 3f },
        { Calibers.Caliber12g, 3f },
        { Calibers.Caliber23x75, 3f },
        { Calibers.Caliber26x75, 3f },
        { Calibers.Caliber30x29, 3f },
        { Calibers.Caliber40x46, 3f },
        { Calibers.Caliber40mmRU, 3f },
        { Calibers.Caliber86x70, 5f },
        { Calibers.Caliber127x99, 5f },
        { Calibers.Caliber127x108, 5f },
        { Calibers.Caliber725, 10f },
        { Calibers.Default, 2f },
    };

    [IgnoreDataMember]
    [Hidden]
    public static readonly Dictionary<string, float> SUPP_AMOUNTS_DEFAULT = new()
    {
        { Calibers.Caliber20x1mm, 0.25f },
        { Calibers.Caliber9x18PM, 1f },
        { Calibers.Caliber9x19PARA, 1.1f },
        { Calibers.Caliber46x30, 1.2f },
        { Calibers.Caliber9x21, 1.25f },
        { Calibers.Caliber57x28, 1.3f },
        { Calibers.Caliber762x25TT, 1.4f },
        { Calibers.Caliber1143x23ACP, 1.5f },
        { Calibers.Caliber9x33R, 1.5f },
        { Calibers.Caliber545x39, 2.1f },
        { Calibers.Caliber556x45NATO, 2f },
        { Calibers.Caliber9x39, 2.5f },
        { Calibers.Caliber762x35, 2.4f },
        { Calibers.Caliber762x39, 2.5f },
        { Calibers.Caliber366TKM, 2.5f },
        { Calibers.Caliber68x51, 2.5f },
        { Calibers.Caliber762x51, 2.65f },
        { Calibers.Caliber127x55, 2.7f },
        { Calibers.Caliber762x54R, 2.75f },
        { Calibers.Caliber86x70, 5f },
        { Calibers.Caliber20g, 3f },
        { Calibers.Caliber12g, 3f },
        { Calibers.Caliber23x75, 3f },
        { Calibers.Caliber26x75, 3f },
        { Calibers.Caliber30x29, 3f },
        { Calibers.Caliber40x46, 3f },
        { Calibers.Caliber40mmRU, 3f },
        { Calibers.Caliber127x99, 5f },
        { Calibers.Caliber127x108, 5f },
        { Calibers.Caliber725, 10f },
        { Calibers.Default, 2f },
    };

    [DataMember]
    [Advanced]
    [Name("Max Suppression Number")]
    [Description("Suppression caps at this number.")]
    [MinMax(0.01f, 50f, 100f)]
    [Category("Suppression")]
    public float SUPP_MAX_NUM = 30f;

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }
}
