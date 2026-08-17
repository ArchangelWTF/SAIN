using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.General;

[DataContract]
public class JokeSettings : SAINSettingsBase<JokeSettings>, ISAINSettings
{
    [DataMember]
    [Name("Random Cheater AI")]
    [Description(
        "Emulate the real Live-Like experience! 1% of bots will be a cheater. They will move faster than they should, have 0 recoil, and perfect aim, always shoot full auto at any range if their weapon supports it, and always fire as fast as possible if they have a semi-auto weapon."
    )]
    public bool RandomCheaters = false;

    [DataMember]
    [Name("Random Speed Hacker Chance")]
    [Description("If for some reason you enabled random cheaters, this is the chance they will be assigned as one.")]
    [Percentage]
    public float RandomCheaterChance = 1f;

    [DataMember]
    [Name("Enable Khorovod layers for PMCs")]
    [Description(
        "Requires restart. Allows the pmc's to get assigned a khorovod brain similar to the ones on scavs, will make PMC's rush you at the generator"
    )]
    public bool EnableKhorovodPMCs = true;
}
