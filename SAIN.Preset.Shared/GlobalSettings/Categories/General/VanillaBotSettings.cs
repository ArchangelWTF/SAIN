using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.General;

[DataContract]
public class VanillaBotSettings : SAINSettingsBase<VanillaBotSettings>, ISAINSettings
{
    [DataMember]
    [Name("Vanilla Scavs")]
    [Description(
        "REQUIRES RESTART OF GAME. Non Player-Scavs will have vanilla ai behavior. Disabling sain for player scavs is not currently possible."
    )]
    public bool VanillaScavs = false;

    [DataMember]
    [Name("Vanilla Bosses")]
    [Description("REQUIRES RESTART OF GAME. Bosses (other than those with separate config options) will have vanilla ai behavior.")]
    public bool VanillaBosses = false;

    [DataMember]
    [Name("Vanilla Boss Followers")]
    [Description("REQUIRES RESTART OF GAME. Boss Followers (other than those with separate config options) will have vanilla ai behavior.")]
    public bool VanillaFollowers = false;

    [DataMember]
    [Name("Vanilla Goons")]
    [Description(
        "REQUIRES RESTART OF GAME. Goons will have vanilla behavior. This disables custom personality edits specially made for the goons and I will be very sad."
    )]
    public bool VanillaGoons = false;

    [DataMember]
    [Name("Vanilla Bloodhounds")]
    [Description("REQUIRES RESTART OF GAME")]
    public bool VanillaBloodHounds = false;

    [DataMember]
    [Name("Vanilla Rogues")]
    [Description("REQUIRES RESTART OF GAME")]
    public bool VanillaRogues = false;

    [DataMember]
    [Name("Vanilla Raiders")]
    [Description("REQUIRES RESTART OF GAME")]
    public bool VanillaRaiders = false;

    [DataMember]
    [Name("Vanilla Cultists")]
    [Description("REQUIRES RESTART OF GAME")]
    public bool VanillaCultists = false;

    [DataMember]
    [Name("Vanilla Labyrinth Bots")]
    [Description(
        "REQUIRES RESTART OF GAME. Shadow of Tagilla, Vengeful Killa, and their followers on Labyrinth will have vanilla behavior."
    )]
    public bool VanillaLabyrinthBots = false;

    [DataMember]
    [Name("Vanilla Special Bots")]
    [Description("REQUIRES RESTART OF GAME. Obdolbs (crazy) Scavs and Santa will have vanilla behavior.")]
    public bool VanillaSpecialBots = false;
}
