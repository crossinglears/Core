using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace CrossingLears
{
    public static class CSharp_Utilities
    {
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int k = Random.Range(0, i + 1);
                T value = list[k];
                list[k] = list[i];
                list[i] = value;
            }
            return list;
        }

        public static T GetRandomFromList<T>(this IList<T> list) 
            => list[Random.Range(0, list.Count)];

        public static T RandomEnum<T>() where T : struct, IComparable, IFormattable, IConvertible 
            => ((T[])Enum.GetValues(typeof(T))).GetRandomFromList();
    }
}