using System;
using System.Collections.Generic;

namespace SAIN.Preset.Shared.Helpers;

public static class ListHelpers
{
    public static void FilterByPredicateNonAlloc<T>(this List<T> input, List<T> output, Predicate<T> predicate)
    {
        output.Clear();
        for (int i = 0; i < input.Count; i++)
        {
            T item = input[i];
            if (predicate(item))
            {
                output.Add(item);
            }
        }
    }

    public static void PopulateKeys<T, K>(Dictionary<T, K> dictionary, K defaultVal)
        where T : Enum
    {
        foreach (T item in (T[])Enum.GetValues(typeof(T)))
        {
            if (dictionary.ContainsKey(item))
            {
                continue;
            }
            dictionary.Add(item, defaultVal);
        }
    }

    public static void PopulateKeys<K>(Dictionary<string, K> dictionary, IEnumerable<string> keys, K defaultVal)
    {
        foreach (string key in keys)
        {
            if (dictionary.ContainsKey(key))
            {
                continue;
            }
            dictionary.Add(key, defaultVal);
        }
    }

    public static void CloneEntries<T, K>(Dictionary<T, K> source, Dictionary<T, K> destination)
    {
        foreach (KeyValuePair<T, K> kvp in source)
        {
            if (destination.ContainsKey(kvp.Key))
            {
                continue;
            }
            destination.Add(kvp.Key, kvp.Value);
        }
    }
}
