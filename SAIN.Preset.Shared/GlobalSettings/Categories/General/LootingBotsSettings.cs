using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAIN.Preset.Shared.GlobalSettings.Categories.General;

[DataContract]
public class LootingBotsSettings : SAINSettingsBase<LootingBotsSettings>, ISAINSettings
{
    [DataMember]
    [Name("Bot Extraction From Loot")]
    public bool ExtractFromLoot = true;

    [DataMember]
    [Name("Min Loot Val PMC")]
    [MinMax(1f, 5000000, 1f)]
    public float MinLootValPMC = 500000;

    [DataMember]
    [Name("Min Loot Val SCAV")]
    [MinMax(1f, 5000000, 1f)]
    public float MinLootValSCAV = 200000;

    [DataMember]
    [Name("Min Loot Val Other")]
    [MinMax(1f, 5000000, 1f)]
    public float MinLootValOther = 350000;

    [DataMember]
    [Name("Min Loot Val Exception")]
    [Description("If a bot's loot value is greater than or equal to this, they will extract even with space available in their inventory.")]
    [MinMax(1f, 5000000, 1f)]
    public float MinLootValException = 1500000;
}
