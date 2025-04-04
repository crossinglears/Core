using UnityEngine;

namespace CrossingLears
{
    public static class GameInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnSceneStart()
        {
            Debug.Log("Crossing Lears: Game Started!");

            foreach (var item in MonoBehaviour.FindObjectsByType<ClosedAtStart>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                item.close();
            }
        }
    }
}
