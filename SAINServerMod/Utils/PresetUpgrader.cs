using System.Text.Json.Nodes;

namespace SAINServerMod.Utils;

public sealed class PresetSchemaDiff(string name)
{
    public string Name { get; } = name;

    public SortedSet<string> Added { get; } = new(StringComparer.Ordinal);

    public SortedSet<string> Removed { get; } = new(StringComparer.Ordinal);

    public List<string> SkippedFiles { get; } = [];

    public bool HasChanges
    {
        get { return Added.Count > 0 || Removed.Count > 0; }
    }

    public void SkipFile(string file)
    {
        SkippedFiles.Add(Path.GetFileName(file));
    }
}

public static class PresetUpgrader
{
    private const int MaxDepth = 16;
    private const int MinDictionaryEntries = 2;

    public static PresetSchemaDiff Diff(string name, string savedJson, string currentJson)
    {
        var diff = new PresetSchemaDiff(name);
        DiffInto(diff, string.Empty, savedJson, currentJson);
        return diff;
    }

    public static void DiffInto(PresetSchemaDiff diff, string prefix, string savedJson, string currentJson)
    {
        Walk(Parse(savedJson), Parse(currentJson), prefix, diff, 0);
    }

    private static JsonNode? Parse(string json)
    {
        return JsonNode.Parse(json.TrimStart('﻿'));
    }

    private static void Walk(JsonNode? saved, JsonNode? current, string path, PresetSchemaDiff diff, int depth)
    {
        if (depth > MaxDepth)
        {
            return;
        }

        if (saved is JsonArray savedArray && current is JsonArray currentArray)
        {
            int shared = Math.Min(savedArray.Count, currentArray.Count);
            for (int i = 0; i < shared; i++)
            {
                Walk(savedArray[i], currentArray[i], path + "[*]", diff, depth + 1);
            }
            return;
        }

        if (saved is not JsonObject savedObject || current is not JsonObject currentObject)
        {
            return;
        }

        if (TryWalkAsDictionary(savedObject, currentObject, path, diff, depth))
        {
            return;
        }

        foreach (KeyValuePair<string, JsonNode?> member in currentObject)
        {
            string memberPath = Extend(path, member.Key);
            if (!savedObject.ContainsKey(member.Key))
            {
                diff.Added.Add(memberPath);
                continue;
            }
            Walk(savedObject[member.Key], member.Value, memberPath, diff, depth + 1);
        }

        foreach (KeyValuePair<string, JsonNode?> member in savedObject)
        {
            if (!currentObject.ContainsKey(member.Key))
            {
                diff.Removed.Add(Extend(path, member.Key));
            }
        }
    }

    private static bool TryWalkAsDictionary(JsonObject saved, JsonObject current, string path, PresetSchemaDiff diff, int depth)
    {
        if (!IsHomogeneous(saved) || !IsHomogeneous(current))
        {
            return false;
        }

        string? shared = current.Select(entry => entry.Key).FirstOrDefault(saved.ContainsKey);
        if (shared is null)
        {
            return false;
        }

        Walk(saved[shared], current[shared], path + "[*]", diff, depth + 1);
        return true;
    }

    private static bool IsHomogeneous(JsonObject node)
    {
        if (node.Count < MinDictionaryEntries)
        {
            return false;
        }

        HashSet<string>? shape = null;
        foreach (KeyValuePair<string, JsonNode?> entry in node)
        {
            if (entry.Value is not JsonObject entryObject)
            {
                return false;
            }

            if (shape is null)
            {
                shape = [.. entryObject.Select(member => member.Key)];
                continue;
            }

            if (!shape.SetEquals(entryObject.Select(member => member.Key)))
            {
                return false;
            }
        }
        return true;
    }

    private static string Extend(string path, string member)
    {
        return path.Length == 0 ? member : $"{path}.{member}";
    }
}
