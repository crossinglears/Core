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

            foreach (StartState obj in Object.FindObjectsByType<StartState>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                switch (obj.startState)
                {
                    case StartState.StartStateEnum.Destroy:
                        obj.destroy();
                        break;
                    case StartState.StartStateEnum.Open:
                        obj.open();
                        break;
                    case StartState.StartStateEnum.Close:
                        obj.close();
                        break;
                }
            }
        }
    }
}
