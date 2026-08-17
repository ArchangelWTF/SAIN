using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.Personalities.BasePersonality.Categories;

[DataContract]
public class PersonalityTalkSettings : SAINSettingsBase<PersonalityTalkSettings>, ISAINSettings
{
    [DataMember]
    [Name("Can Yell Taunts")]
    [Description("Hey you...yeah YOU! FUCK YOU! You heard?")]
    public bool CanTaunt = false;

    [DataMember]
    [Name("Can Yell Taunts Frequently")]
    [Description("HEY COCKSUCKAAAA")]
    public bool FrequentTaunt = false;

    [DataMember]
    [Name("Can Yell Taunts Constantly")]
    [Description("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    public bool ConstantTaunt = false;

    [DataMember]
    [Description("Will this personality yell back at enemies taunting them")]
    public bool CanRespondToEnemyVoice = true;

    [DataMember]
    [Advanced]
    [MinMax(0.1f, 100f, 100f)]
    public float TauntFrequency = 15f;

    [DataMember]
    [Advanced]
    [MinMax(0f, 100f, 1f)]
    public float TauntChance = 50f;

    [DataMember]
    [Advanced]
    [MinMax(0.1f, 150f, 100f)]
    public float TauntMaxDistance = 50f;

    [DataMember]
    [Advanced]
    public bool CanFakeDeathRare = false;

    [DataMember]
    [Advanced]
    public float FakeDeathChance = 2f;

    [DataMember]
    [Advanced]
    public bool CanBegForLife = false;
}
