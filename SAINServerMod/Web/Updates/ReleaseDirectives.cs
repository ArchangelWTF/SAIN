using System.Text.RegularExpressions;

namespace SAINServerMod.Web.Updates;

public sealed record CompatibilityTag(string Text, string Platform, string? Version);

/// <summary>
/// Parses the compatibility directives GitHub release bodies carry as HTML comments, e.g.
/// <c>&lt;!-- compat: SPT 4.1 --&gt;</c>, and strips them out of the rendered notes.
/// </summary>
public static class ReleaseDirectives
{
    private static readonly Regex _htmlComment = new(@"<!--(?<content>.*?)-->", RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex _compatDirective = new(
        @"^\s*compat(?:ibility)?\s*:\s*(?<entries>.+)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline
    );

    private static readonly Regex _whitespaceRun = new(@"\s+", RegexOptions.Compiled);

    public static List<CompatibilityTag> ParseCompatibility(string? body)
    {
        List<CompatibilityTag> tags = [];

        if (string.IsNullOrWhiteSpace(body))
        {
            return tags;
        }

        foreach (Match comment in _htmlComment.Matches(body))
        {
            Match directive = _compatDirective.Match(comment.Groups["content"].Value);

            if (!directive.Success)
            {
                continue;
            }

            string[] entries = directive
                .Groups["entries"]
                .Value.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (string entry in entries)
            {
                string cleaned = _whitespaceRun.Replace(entry, " ").Trim();

                if (cleaned.Length == 0 || tags.Any(tag => string.Equals(tag.Text, cleaned, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                tags.Add(ToTag(cleaned));
            }
        }

        return tags;
    }

    private static CompatibilityTag ToTag(string entry)
    {
        int split = entry.LastIndexOf(' ');

        if (split <= 0)
        {
            return new CompatibilityTag(entry, entry, null);
        }

        return new CompatibilityTag(entry, entry[..split].Trim(), entry[(split + 1)..].Trim());
    }

    public static string? Strip(string? body)
    {
        return body is null ? null : _htmlComment.Replace(body, string.Empty);
    }
}
