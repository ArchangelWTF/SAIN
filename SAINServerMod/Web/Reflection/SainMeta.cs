using System.Reflection;
using System.Runtime.Serialization;
using SAIN.Preset.Shared.Attributes;

namespace SAINServerMod.Web.Reflection;

public sealed class SainMeta
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string Category { get; init; } = "General";
    public string? Section { get; init; }

    public bool HasRange { get; init; }
    public float Min { get; init; }
    public float Max { get; init; }
    public float Rounding { get; init; } = 100f;

    public bool HasDefaultFloat { get; init; }
    public float DefaultFloat { get; init; }

    public bool Advanced { get; init; }
    public bool Developer { get; init; }
    public bool Experimental { get; init; }
    public bool Hidden { get; init; }
    public bool SimpleValue { get; init; }

    public float Round(float value)
    {
        return Rounding > 0 ? (float)(System.Math.Round(value * Rounding) / Rounding) : value;
    }

    public float Clamp(float value)
    {
        if (!HasRange)
        {
            return value;
        }
        return System.Math.Max(Min, System.Math.Min(Max, value));
    }

    public static SainMeta Read(MemberInfo member)
    {
        var nameDesc = member.GetCustomAttribute<NameAndDescriptionAttribute>();
        var name = nameDesc?.Name ?? member.GetCustomAttribute<NameAttribute>()?.Value ?? Prettify(member.Name);
        var description = nameDesc?.Description ?? member.GetCustomAttribute<DescriptionAttribute>()?.Value;

        var range = member.GetCustomAttribute<GUIValuesAttribute>();
        var defaultFloat = member.GetCustomAttribute<DefaultFloatAttribute>();

        return new SainMeta
        {
            Name = name,
            Description = description,
            Category = member.GetCustomAttribute<CategoryAttribute>()?.Value ?? "General",
            Section = member.GetCustomAttribute<SectionAttribute>()?.Value,
            HasRange = range != null,
            Min = range?.Min ?? 0f,
            Max = range?.Max ?? 100f,
            Rounding = range?.Rounding ?? 100f,
            HasDefaultFloat = defaultFloat != null,
            DefaultFloat = defaultFloat?.Value ?? 0f,
            Advanced = member.GetCustomAttribute<AdvancedAttribute>() != null,
            Developer = member.GetCustomAttribute<DeveloperOptionAttribute>() != null,
            Experimental = member.GetCustomAttribute<ExperimentalAttribute>() != null,
            Hidden = member.GetCustomAttribute<HiddenAttribute>() != null || member.GetCustomAttribute<IgnoreDataMemberAttribute>() != null,
            SimpleValue = member.GetCustomAttribute<SimpleValueAttribute>() != null,
        };
    }

    private static string Prettify(string raw)
    {
        // Turn UPPER_SNAKE / camelCase into a readable label as a fallback for un-named fields.
        var spaced = raw.Replace('_', ' ').Trim();
        if (spaced.Length == 0)
        {
            return raw;
        }
        return char.ToUpperInvariant(spaced[0]) + spaced[1..];
    }
}
