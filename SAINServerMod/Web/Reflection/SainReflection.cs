using System.Reflection;

namespace SAINServerMod.Web.Reflection;

public sealed record SainCategory(string Name, IReadOnlyList<SainMember> Members);

public static class SainReflection
{
    private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance;

    public static List<SainMember> Members(object owner)
    {
        var type = owner.GetType();
        var result = new List<SainMember>();

        foreach (var field in type.GetFields(Flags).OrderBy(f => f.MetadataToken))
        {
            if (field.IsInitOnly && field.IsStatic)
            {
                continue;
            }
            var member = new SainMember(owner, field);
            if (member.Meta.Hidden || !IsEditable(member))
            {
                continue;
            }
            result.Add(member);
        }

        foreach (var prop in type.GetProperties(Flags).OrderBy(p => p.MetadataToken))
        {
            if (prop.GetIndexParameters().Length > 0 || prop.GetMethod == null)
            {
                continue;
            }
            var member = new SainMember(owner, prop);
            if (member.Meta.Hidden || !IsEditable(member))
            {
                continue;
            }
            result.Add(member);
        }

        return result;
    }

    private static bool IsEditable(SainMember member)
    {
        return member.Kind is not (SainFieldKind.String or SainFieldKind.Unsupported);
    }

    public static List<SainCategory> Categories(object owner)
    {
        var order = new List<string>();
        var buckets = new Dictionary<string, List<SainMember>>();

        foreach (var member in Members(owner))
        {
            var category = string.IsNullOrWhiteSpace(member.Meta.Category) ? "General" : member.Meta.Category;
            if (!buckets.TryGetValue(category, out var list))
            {
                list = new List<SainMember>();
                buckets[category] = list;
                order.Add(category);
            }
            list.Add(member);
        }

        return order
            .Select(name => new SainCategory(
                name,
                buckets[name]
                    .OrderBy(m =>
                        m.Meta.Developer ? 2
                        : m.Meta.Advanced ? 1
                        : 0
                    )
                    .ToList()
            ))
            .ToList();
    }

    public static bool IsVisible(SainMeta meta, bool showAdvanced, bool showDeveloper)
    {
        if (meta.Hidden)
        {
            return false;
        }

        if (meta.Advanced && !showAdvanced)
        {
            return false;
        }

        if (meta.Developer && !showDeveloper)
        {
            return false;
        }
        return true;
    }
}
