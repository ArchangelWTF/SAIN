using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.GlobalSettings;

namespace SAIN.Preset.Shared.Personalities.BasePersonality.Categories;

[DataContract]
public class PersonalityCoverSettings : SAINSettingsBase<PersonalityCoverSettings>, ISAINSettings
{
    public PersonalityCoverSettings() { }

    public PersonalityCoverSettings(bool createDefaults) { }

    [DataMember]
    [Advanced]
    public bool CanShiftCoverPosition = true;

    [DataMember]
    [Advanced]
    public float ShiftCoverTimeMultiplier = 1f;

    [DataMember]
    [Percentage0to1]
    [Advanced]
    public float MoveToCoverNoEnemySpeed = 1f;

    [DataMember]
    [Percentage0to1]
    [Advanced]
    public float MoveToCoverNoEnemyPose = 1f;

    [DataMember]
    [Percentage0to1]
    [Advanced]
    public float MoveToCoverHasEnemySpeed = 1f;

    [DataMember]
    [Percentage0to1]
    [Advanced]
    public float MoveToCoverHasEnemyPose = 1f;
}
