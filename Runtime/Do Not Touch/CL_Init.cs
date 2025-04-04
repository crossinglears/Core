using UnityEngine;

namespace CrossingLears
{
    public static class GameInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnSceneStart()
        {
            Debug.Log("Crossing Lears: Game Started!");

            foreach (StartState obj in Object.FindObjectsByType<StartState>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                obj.gameObject.SetActive(obj.startState);
            }
        }
    }
}
