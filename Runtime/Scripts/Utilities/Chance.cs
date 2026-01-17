using System;
using Random = UnityEngine.Random;

namespace CrossingLears
{
    public static class Chance
    {
        public static bool chance(float chanceTrue) => Random.Range(0, 100f) < chanceTrue;

        public static bool chance(int chanceTrue) => Random.Range(0, 101) < chanceTrue;

        public static bool chance() => Random.value > 0.5f;

        public static int round(float input)
        {
            int whole = (int)input;
            return chance((input - whole) * 100) ? whole + 1 : whole;
        }

        public static T RandomEnum<T>() where T : struct, IComparable, IFormattable, IConvertible 
            => ((T[])Enum.GetValues(typeof(T))).GetRandomFromList();
    }
}
