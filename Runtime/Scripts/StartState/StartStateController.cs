using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrossingLears
{
    [DefaultExecutionOrder(-100)]
    public class StartStateController : MonoBehaviour
    {
        public List<StartState> startStates;

#if UNITY_EDITOR
        public static void SpawnStartStateController(Scene scene)
        {
            GameObject obj = new GameObject("StartState Controller");
            var ssc = obj.AddComponent<StartStateController>();

            // Move to the correct scene
            SceneManager.MoveGameObjectToScene(obj, scene);
            ssc.GetAllStartStates();

            UnityEditor.Undo.RegisterCreatedObjectUndo(obj, "Create StartStateController");
        }

        [Button]
        void GetAllStartStates()
        {
            startStates = FindObjectsByType<StartState>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(s => s.gameObject.scene == gameObject.scene)
                .ToList();
        }
        #endif

        [Button("Trigger All StartState")]
        void Start()
        {
            foreach (StartState obj in startStates)
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
