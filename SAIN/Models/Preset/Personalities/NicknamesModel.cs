using System.Collections.Generic;
using SAIN.Preset.Shared.Models.Preset.Personalities;

namespace SAIN.Models.Preset.Personalities;

public sealed class NicknamesModel
{
    public Dictionary<string, EPersonality> NicknamePersonalities { get; set; } =
        new()
        {
            { "steve", EPersonality.Wreckless },
            { "solarint", EPersonality.GigaChad },
            { "lvndmark", EPersonality.SnappingTurtle },
            { "chomp", EPersonality.Chad },
            { "senko", EPersonality.Chad },
            { "kaeno", EPersonality.Timmy },
            { "justnu", EPersonality.Timmy },
            { "ratthew", EPersonality.Rat },
            { "choccy", EPersonality.Rat },
        };
}
