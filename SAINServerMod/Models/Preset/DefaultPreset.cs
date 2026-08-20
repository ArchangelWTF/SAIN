using SAIN.Preset.Shared.Enums;

namespace SAINServerMod.Models.Preset;

public sealed record DefaultPreset(SAINDifficulty Difficulty, string Name, string Description);
