using System.Collections.Generic;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories;

[DataContract]
public class TalkSettings : SAINSettingsBase<TalkSettings>, ISAINSettings
{
    [DataMember]
    [Category("Peaceful Talk")]
    [Name("Talkative Scavs")]
    [Description("When at peace, scavs will talk to each other and be noisy. Revealing their location.")]
    public bool TalkativeScavs = true;

    [DataMember]
    [Category("Peaceful Talk")]
    [Name("Talkative PMCs")]
    [Description("When at peace, pmcs will talk to each other and be noisy. Revealing their location.")]
    public bool TalkativePMCs = false;

    [DataMember]
    [Category("Peaceful Talk")]
    [Name("Talkative Raiders and Rogues")]
    [Description("When at peace, raiders and rogues will talk to each other and be noisy. Revealing their location.")]
    public bool TalkativeRaidersRogues = true;

    [DataMember]
    [Category("Peaceful Talk")]
    [Name("Talkative Bosses")]
    [Description("When at peace, Bosses and boss guards will talk to each other and be noisy. Revealing their location.")]
    public bool TalkativeBosses = true;

    [DataMember]
    [Category("Peaceful Talk")]
    [Name("Talkative Goons")]
    [Description("When at peace, The Goons will talk to each other and be noisy. Revealing their location.")]
    public bool TalkativeGoons = false;

    [DataMember]
    [Name("Human Response Chance")]
    [Description("Percentage chance to respond to a voiceline from a friendly human player.")]
    [Category("Friendly Response")]
    [Percentage]
    public float FriendlyReponseChance = 85f;

    [DataMember]
    [Name("AI Response Chance")]
    [Description("Percentage chance to respond to a voiceline from a friendly AI player.")]
    [Category("Friendly Response")]
    [Percentage]
    public float FriendlyReponseChanceAI = 80f;

    [DataMember]
    [Name("Human Response Max Distance")]
    [Category("Friendly Response")]
    [Percentage]
    public float FriendlyReponseDistance = 65f;

    [DataMember]
    [Name("AI Response Max Distance")]
    [Category("Friendly Response")]
    [Percentage]
    public float FriendlyReponseDistanceAI = 35f;

    [DataMember]
    [Name("Response Frequency")]
    [Description("2 = 1 check every 2 second")]
    [Category("Friendly Response")]
    [MinMax(0.5f, 10f)]
    public float FriendlyResponseFrequencyLimit = 1f;

    [DataMember]
    [Name("Response Delay Randomization Min")]
    [Category("Friendly Response")]
    [MinMax(0.25f, 3f)]
    public float FriendlyResponseMinRandomDelay = 0.33f;

    [DataMember]
    [Name("Response Delay Randomization Max")]
    [Category("Friendly Response")]
    [MinMax(0.25f, 3f)]
    public float FriendlyResponseMaxRandomDelay = 0.75f;

    [DataMember]
    [Name("Vanilla Bot Talking")]
    [Description(
        "Disable all SAIN based handling of bot talking. No more squad chatter, no more quiet bots, completely disables SAIN's handling of bot voices"
    )]
    public bool DisableBotTalkPatching = false;

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }
}
