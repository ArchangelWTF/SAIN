using System;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Enums;

namespace SAIN.Preset.Shared.Preset;

[DataContract]
public sealed class SAINPresetDefinition
{
    [DataMember]
    public string Name;

    [DataMember]
    public string Description;

    [DataMember]
    public string Creator;

    [DataMember]
    public string SAINVersion;

    [DataMember]
    public string SAINPresetVersion;

    [DataMember]
    public string DateCreated;

    [DataMember]
    public SAINDifficulty BaseSAINDifficulty = SAINDifficulty.hard;

    [DataMember]
    public bool IsCustom = true;

    [DataMember]
    public bool CanEditName = true;

    public SAINPresetDefinition Clone()
    {
        return new SAINPresetDefinition()
        {
            Name = Name,
            Description = Description,
            Creator = "None",
            SAINVersion = SAINVersionInfo.SAINVersion,
            SAINPresetVersion = SAINVersionInfo.SAINPresetVersion,
            DateCreated = DateTime.Now.ToString(),
            IsCustom = true,
            BaseSAINDifficulty = BaseSAINDifficulty,
        };
    }

    public static SAINPresetDefinition CreateDefaultDefinition(string difficulty, SAINDifficulty baseDifficulty, string description = null)
    {
        return new SAINPresetDefinition
        {
            Name = difficulty,
            Description = description ?? $"The Default {difficulty} SAIN Preset.",
            Creator = "Solarint",
            SAINVersion = SAINVersionInfo.SAINVersion,
            SAINPresetVersion = SAINVersionInfo.SAINPresetVersion,
            DateCreated = DateTime.Now.ToString(),
            IsCustom = false,
            CanEditName = false,
            BaseSAINDifficulty = baseDifficulty,
        };
    }
}
