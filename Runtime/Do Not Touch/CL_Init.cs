using UnityEngine;

namespace CrossingLears
{
    public static class GameInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnGameStart()
        {
            Debug.Log("Crossing Lears: Game Started!");
        }
    }
}
