using System;
using System.Collections.Generic;
using EFT;
using SAIN.Preset.Shared.Enums;

namespace SAIN.Extensions;

public static class SainEnumMirrorExtensions
{
    private static readonly HashSet<int> _esainWildSpawnValues = ValueSet(typeof(ESainWildSpawnType));
    private static readonly HashSet<int> _esainBotDifficultyValues = ValueSet(typeof(ESainBotDifficulty));
    private static readonly HashSet<int> _eftWildSpawnValues = ValueSet(typeof(WildSpawnType));

    public static ESainWildSpawnType ToESain(this WildSpawnType type)
    {
        int value = (int)type;
        if (!_esainWildSpawnValues.Contains(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                $"No {nameof(ESainWildSpawnType)} mapping exists for WildSpawnType '{type}' ({value})."
            );
        }
        return (ESainWildSpawnType)value;
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
        int value = (int)type;
        if (!_eftWildSpawnValues.Contains(value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                $"No {nameof(WildSpawnType)} mapping exists for ESainWildSpawnType '{type}' ({value})."
            );
        }
        return (WildSpawnType)value;
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
