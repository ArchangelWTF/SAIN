using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;
using SAIN.Preset.Shared.Enums;

namespace SAIN.Preset.Shared.GlobalSettings.Categories;

/// <summary>
/// Relative odds of a bot choosing each body part to aim at. Weights are only compared against the parts a bot can actually shoot at that moment
/// </summary>
[DataContract]
public class AimTargetWeights : SAINSettingsBase<AimTargetWeights>, ISAINSettings
{
    [DataMember]
    [Name("Head")]
    [Description("Only used if Can Aim for Headshots is enabled for the bot.")]
    [MinMax(0f, 10f, 100f)]
    public float Head = 0.3f;

    [DataMember]
    [Name("Chest")]
    [MinMax(0f, 10f, 100f)]
    public float Chest = 4f;

    [DataMember]
    [Name("Stomach")]
    [MinMax(0f, 10f, 100f)]
    public float Stomach = 2.5f;

    [DataMember]
    [Name("Arms")]
    [MinMax(0f, 10f, 100f)]
    public float Arms = 1f;

    [DataMember]
    [Name("Legs")]
    [MinMax(0f, 10f, 100f)]
    public float Legs = 1.5f;

    public float For(EAimTargetPart part)
    {
        return part switch
        {
            EAimTargetPart.Head => Head,
            EAimTargetPart.Chest => Chest,
            EAimTargetPart.Stomach => Stomach,
            EAimTargetPart.LeftArm or EAimTargetPart.RightArm => Arms,
            _ => Legs,
        };
    }
}
