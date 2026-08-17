using System;

namespace SAIN.Preset.Shared.Helpers;

public static class MathExt
{
    public static float Round(float value, float round)
    {
        return (float)(Math.Round(value * round) / round);
    }

    public static float Round100(float value)
    {
        return Round(value, 100f);
    }

    public static float Clamp(float value, float min, float max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}
