using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class HelperFunc
{
    private static System.Random rng = new System.Random();
    public static T RandomOrDefault<T>(this IList<T> source)
    {
        if (source.Count == 0)
            return default;

        int randomInt = Random.Range(0, source.Count);
        return source[randomInt];
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        for (int n = list.Count - 1; n > 1; n--)
        {
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
