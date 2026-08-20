using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.Enums;

namespace SAIN.Preset.Shared.GlobalSettings.Categories;

[DataContract]
public class SquadTalkSettings : SAINSettingsBase<SquadTalkSettings>, ISAINSettings
{
    [DataMember]
    [Name("Callout Reloading Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _reportReloadingChance = 33f;

    [DataMember]
    [Name("Callout Reloading Frequency")]
    [Description("4 = once every 4 seconds")]
    [MinMax(0.1f, 20f, 100f)]
    public float _reportReloadingFreq = 4f;

    [DataMember]
    [Name("Callout Lost Enemy Visual Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _reportLostVisualChance = 40f;

    [DataMember]
    [Name("Callout Rat Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _reportRatChance = 33f;

    [DataMember]
    [Name("Callout Rat Time Since Seen Enemy")]
    [Description("")]
    [MinMax(1f, 120f, 1f)]
    public float _reportRatTimeSinceSeen = 60f;

    [DataMember]
    [Name("Callout Enemy Conversation Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _reportEnemyConversationChance = 10f;

    [DataMember]
    [Name("Call Enemy Conversation Max Distance")]
    [Description("")]
    [MinMax(1f, 120f, 1f)]
    public float _reportEnemyMaxDist = 70f;

    [DataMember]
    [Name("Callout Enemy Health Status Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _reportEnemyHealthChance = 40f;

    [DataMember]
    [Name("Callout Enemy Health Status Frequency")]
    [Description("8 = once every 8 seconds")]
    [MinMax(0.1f, 20f, 100f)]
    public float _reportEnemyHealthFreq = 8f;

    [DataMember]
    [Name("Callout Enemy Killed Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _reportEnemyKilledChance = 60f;

    [DataMember]
    [Name("Callout Enemy Killed Squadleader Confirm Chance")]
    [Description(
        "When a member calls out that they killed an enemy, this is the chance the squad leader will acknowledge and confirm the kill with a compliment."
    )]
    [Percentage]
    public float _reportEnemyKilledSquadLeadChance = 60f;

    [DataMember]
    [Name("Toxic Squad Leader")]
    [Description("Old bug turned into a feature. Squad leaders will yell Nice Work when a friendly squad member is killed.")]
    public bool _reportEnemyKilledToxicSquadLeader = false;

    [DataMember]
    [Name("Friend Close Distance")]
    [Description("The distance to a friendly that a bot will consider speaking to them.")]
    [MinMax(1f, 120f, 1f)]
    public float _friendCloseDist = 40f;

    [DataMember]
    [Name("Callout Friendly Killed Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _reportFriendKilledChance = 60f;

    [DataMember]
    [Name("Callout Retreat Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _talkRetreatChance = 60f;

    [DataMember]
    [Name("Callout Retreat Frequency")]
    [Description("10 = once every 10 seconds")]
    [MinMax(0.1f, 20f, 100f)]
    public float _talkRetreatFreq = 10f;

    [DataMember]
    [Hidden]
    public ESainPhraseTrigger _talkRetreatTrigger = ESainPhraseTrigger.CoverMe;

    [DataMember]
    [Hidden]
    public ESainTagStatus _talkRetreatMask = ESainTagStatus.Combat;

    [DataMember]
    [Name("Callout Retreat Group Delay")]
    [Description("Group Delay = Squad Members will share cooldown on certain voicelines to prevent spam")]
    public bool _talkRetreatGroupDelay = true;

    [DataMember]
    [Name("Callout Under Fire Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _underFireNeedHelpChance = 45f;

    [DataMember]
    [Hidden]
    public ESainPhraseTrigger _underFireNeedHelpTrigger = ESainPhraseTrigger.NeedHelp;

    [DataMember]
    [Hidden]
    public ESainTagStatus _underFireNeedHelpMask = ESainTagStatus.Combat;

    [DataMember]
    [Name("Callout Need Help Delay")]
    [Description("Group Delay = Squad Members will share cooldown on certain voicelines to prevent spam")]
    public bool _underFireNeedHelpGroupDelay = true;

    [DataMember]
    [Name("Callout Under Fire Frequency")]
    [Description("10 = once every 10 seconds")]
    [MinMax(0.1f, 20f, 100f)]
    public float _underFireNeedHelpFreq = 1f;

    [DataMember]
    [Name("Callout Need Help Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _enemyNeedHelpChance = 40f;

    [DataMember]
    [Name("Callout Noise Heard Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _hearNoiseChance = 50f;

    [DataMember]
    [Name("Call Heard Noise Max Distance")]
    [Description("Max Distance from a non-gunshot sound to report it to squad members with a voiceline.")]
    [MinMax(1f, 120f, 1f)]
    public float _hearNoiseMaxDist = 70f;

    [DataMember]
    [Name("Callout Heard Noise Frequency")]
    [Description("10 = once every 10 seconds")]
    [MinMax(0.1f, 60f, 100f)]
    public float _hearNoiseFreq = 30f;

    [DataMember]
    [Name("Callout Enemy Location Chance")]
    [Description("If conditions are met, this is the chance a bot will actually say a voiceline.")]
    [Percentage]
    public float _enemyLocationTalkChance = 70f;

    [DataMember]
    [Name("Callout Enemy Location Time Since Seen")]
    [Description("Time Since Seen = How long since they have seen or heard an enemy.")]
    [MinMax(0.1f, 10f, 100f)]
    public float _enemyLocationTalkTimeSinceSeen = 3f;

    [DataMember]
    [Name("Callout Enemy Location Frequency")]
    [Description("10 = once every 10 seconds")]
    [MinMax(0.1f, 20f, 100f)]
    public float _enemyLocationTalkFreq = 1f;

    [DataMember]
    [Name("Callout Enemy Location Behind Angle Arc")]
    [MinMax(1f, 90f, 1f)]
    [Advanced]
    public float _enemyLocationBehindAngle = 90f;

    [DataMember]
    [Name("Callout Enemy Location Side Angle Arc")]
    [MinMax(1f, 90f, 1f)]
    [Advanced]
    public float _enemyLocationSideAngle = 45f;

    [DataMember]
    [Name("Callout Enemy Location Front Angle Arc")]
    [MinMax(1f, 90f, 1f)]
    [Advanced]
    public float _enemyLocationFrontAngle = 90f;

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }
}
