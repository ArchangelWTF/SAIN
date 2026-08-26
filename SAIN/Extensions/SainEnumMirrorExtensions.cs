using System;
using System.Collections.Generic;
using EFT;
using SAIN.Preset.Shared.Enums;

namespace SAIN.Extensions;

public static class SainEnumMirrorExtensions
{
    private static readonly HashSet<int> _esainBotDifficultyValues = ValueSet(typeof(ESainBotDifficulty));
    
    public static ESainWildSpawnType ToESain(this WildSpawnType type)
    {
        return (ESainWildSpawnType)(int)type;
    }

    public static ESainBotDifficulty ToESain(this BotDifficulty difficulty)
    {
        int value = (int)difficulty;
        if (!_esainBotDifficultyValues.Contains(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(difficulty),
                difficulty,
                $"No {nameof(ESainBotDifficulty)} mapping exists for BotDifficulty '{difficulty}' ({value})."
            );
        }
        return (ESainBotDifficulty)value;
    }

    public static WildSpawnType ToEft(this ESainWildSpawnType type)
    {
        return (WildSpawnType)(int)type;
    }

    public static EPhraseTrigger ToEft(this ESainPhraseTrigger trigger)
    {
        return (EPhraseTrigger)trigger;
    }

    public static ETagStatus ToEft(this ESainTagStatus status)
    {
        return (ETagStatus)status;
    }

    private static HashSet<int> ValueSet(Type enumType)
    {
        var set = new HashSet<int>();
        foreach (object value in Enum.GetValues(enumType))
        {
            set.Add(Convert.ToInt32(value));
        }
        return set;
    }
}
