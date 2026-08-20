using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.BotSettings.SAINSettings.Categories;

[DataContract]
public class SAINGrenadeSettings : SAINSettingsBase<SAINGrenadeSettings>, ISAINSettings
{
    [DataMember]
    public bool CanThrowWhileSprinting = false;

    [DataMember]
    [NameAndDescription("Can Throw at Visible Enemies", "Toggles bots throwing grenades directly at enemies they can see.")]
    public bool CAN_THROW_STRAIGHT_CONTACT = false;

    [DataMember]
    [NameAndDescription(
        "Since Since Enemy Seen before Throw",
        "How long it has been since a bot's enemy has been visible before a bot can consider throwing a grenade."
    )]
    [MinMax(0.0f, 30f, 100f)]
    public float TimeSinceSeenBeforeThrow = 4f;

    [DataMember]
    [NameAndDescription("Time Before Next Throw - Min", "How much time to wait before a bot is allowed to throw another grenade.")]
    [MinMax(3f, 30f, 100f)]
    public float ThrowGrenadeFrequency = 5f;

    [DataMember]
    [NameAndDescription("Time Before Next Throw - Max", "How much time to wait before a bot is allowed to throw another grenade.")]
    [MinMax(3f, 60f, 100f)]
    public float ThrowGrenadeFrequency_MAX = 10f;

    [DataMember]
    [NameAndDescription(
        "Minimum Friendly Distance to Throw Target",
        "How close a friendly bot can be in Meters to a bot's grenade target before it stops them from throwing it."
    )]
    [MinMax(0.01f, 30f, 100f)]
    public float MinFriendlyDistance = 8f;

    [DataMember]
    [NameAndDescription("Minimum Enemy Distance to Bot", "How close a enemy can be in Meters before a bot doesn't try throwing grenades.")]
    [MinMax(0.01f, 30f, 100f)]
    public float MinEnemyDistance = 8f;

    [DataMember]
    [NameAndDescription("Grenade Spread", "How much distance, in meters, to randomize a bot's throw target position.")]
    [MinMax(0f, 5f, 100f)]
    [CopyValue]
    public float GrenadePrecision = 0.25f;

    [Percentage0to1]
    [Advanced]
    [Hidden]
    [IgnoreDataMember]
    public float MIN_THROW_DIST_PERCENT_0_1 = 0.5f;

    [Hidden]
    [IgnoreDataMember]
    public float CHANCE_TO_NOTIFY_ENEMY_GR_100 = 100f;

    [Hidden]
    [IgnoreDataMember]
    public float DELTA_GRENADE_START_TIME = 0.0f;

    [Hidden]
    [IgnoreDataMember]
    public int BEWARE_TYPE = 2;

    [Hidden]
    [IgnoreDataMember]
    public float DELTA_NEXT_ATTEMPT = 4f;
}
