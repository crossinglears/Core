using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrossingLears
{
    public static class PlayModeStaticReset
    {
        static readonly List<Action> resets = new List<Action>();

        public static void Register(Action reset)
        {
            resets.Add(reset);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            for (int i = 0; i < resets.Count; i++)
            {
                resets[i]();
            }
        }
    }
}
