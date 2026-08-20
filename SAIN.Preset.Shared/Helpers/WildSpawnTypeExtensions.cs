using System;
using SAIN.Preset.Shared.Enums;

namespace SAIN.Preset.Shared.Helpers;

public static class WildSpawnTypeExtensions
{
    public static bool IsPMC(this ESainWildSpawnType type)
    {
        return type == ESainWildSpawnType.pmcBEAR || type == ESainWildSpawnType.pmcUSEC;
    }

    public static bool IsBoss(this ESainWildSpawnType type)
    {
        return type.ToString().StartsWith("boss", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFollower(this ESainWildSpawnType type)
    {
        return type.ToString().StartsWith("follower", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsBossOrFollower(this ESainWildSpawnType type)
    {
        return type.IsBoss() || type.IsFollower();
    }
}
