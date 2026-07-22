using System.Collections.Generic;

public static class DictionaryExtensions
{

    public static bool ContentEqual<TKey, TValue>(
        this Dictionary<TKey, TValue> a,
        Dictionary<TKey, TValue> b)
        where TKey : notnull
    {
        if (ReferenceEquals(a, b)) return true;
        if (a.Count != b.Count) return false;

        var valueComparer = EqualityComparer<TValue>.Default;
        foreach (var kv in a)
        {
            if (!b.TryGetValue(kv.Key, out var other)) return false;
            if (!valueComparer.Equals(kv.Value, other)) return false;
        }

        return true;
    }

}
