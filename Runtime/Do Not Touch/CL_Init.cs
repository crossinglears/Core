using UnityEngine;

namespace CrossingLears
{
    [DefaultExecutionOrder(-10)]
    public static class GameInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnSceneStart()
        {
            Debug.Log("Crossing Lears: Game Started!");
        }
    }
}
