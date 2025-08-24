using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CrossingLears
{
    [DisallowMultipleComponent]
    public class StartState : MonoBehaviour
    {
        public enum StartStateEnum
        {
            Close,
            Open,
            Destroy,
        }

        public StartStateEnum startState;

#if UNITY_EDITOR
        void Reset()
        {
            StartStateController ssc = FindObjectsByType<StartStateController>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(c => c.gameObject.scene == gameObject.scene); // Only same scene

            if (ssc != null)
            {
                if (!ssc.startStates.Contains(this))
                {
                    ssc.startStates.Add(this);
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("StartStateController Missing",
                    "No StartStateController found in this scene.\n\nDo you want to create one?",
                    "Create", "Don't Create"))
                {
                    StartStateController.SpawnStartStateController(gameObject.scene);
                }
            }
        }
#endif
    }
}
