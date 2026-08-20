using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.Personalities.BasePersonality.Categories;

[DataContract]
public class PersonalityRushSettings : SAINSettingsBase<PersonalityRushSettings>, ISAINSettings
{
    [DataMember]
    [Name("Can Rush Healing/Reloading/Grenade-Pulling Enemies")]
    public bool CanRushEnemyReloadHeal = false;

    [DataMember]
    [Name("Can Jump Push")]
    [Description("Can this personality jump when rushing an enemy?")]
    public bool CanJumpCorners = false;

    [DataMember]
    [Name("Jump Push Chance")]
    [Description("If a bot can Jump Push, this is the chance they will actually do it.")]
    [Percentage()]
    public float JumpCornerChance = 60f;

    [DataMember]
    [Name("Can Bunny Hop during Jump Push")]
    [Description("Can this bot hit a clip on you?")]
    public bool CanBunnyHop = false;

    [DataMember]
    [Name("Bunny Hop Chance")]
    [Description("If a bot can bunny hop, this is the chance they will actually do it.")]
    [Percentage()]
    public float BunnyHopChance = 5f;
}
