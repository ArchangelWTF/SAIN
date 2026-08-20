namespace SAINServerMod.Web.Reflection;

public sealed record SainSearchHit(string Breadcrumb, SainMember Member);

public static class SainSearch
{
    private const int MaxDepth = 6;

    public static List<SainSearchHit> Collect(object target, string query, bool showAdvanced, bool showDeveloper)
    {
        var tokens = query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => t.ToLowerInvariant())
            .ToArray();

        var hits = new List<SainSearchHit>();
        Walk(target, tokens, showAdvanced, showDeveloper, string.Empty, 0, hits);
        return hits;
    }

    private static void Walk(object target, string[] tokens, bool adv, bool dev, string prefix, int depth, List<SainSearchHit> hits)
    {
        if (depth > MaxDepth)
        {
            return;
        }

        foreach (var member in SainReflection.Members(target))
        {
            if (!SainReflection.IsVisible(member.Meta, adv, dev))
            {
                continue;
            }

            switch (member.Kind)
            {
                case SainFieldKind.Bool:
                case SainFieldKind.Float:
                case SainFieldKind.Int:
                case SainFieldKind.Enum:
                case SainFieldKind.String:
                    if (Matches(member.Meta, tokens))
                    {
                        hits.Add(new SainSearchHit(prefix, member));
                    }
                    break;

                case SainFieldKind.Nested:
                    var value = member.GetValue();
                    if (value != null)
                    {
                        Walk(value, tokens, adv, dev, Append(prefix, member.Meta.Name), depth + 1, hits);
                    }
                    break;
            }
        }
    }

    private static bool Matches(SainMeta meta, string[] tokens)
    {
        if (tokens.Length == 0)
        {
            return true;
        }
        var haystack = $"{meta.Name} {meta.Description} {meta.Category}".ToLowerInvariant();
        return tokens.All(haystack.Contains);
    }

    private static string Append(string prefix, string name)
    {
        return string.IsNullOrEmpty(prefix) ? name : $"{prefix} › {name}";
    }
}
