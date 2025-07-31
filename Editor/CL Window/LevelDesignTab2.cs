using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public partial class LevelDesignTab : CL_WindowTab_WIP
    {
        void IsolateButton()
        {
            bool isIsolated = SceneVisibilityManager.instance.IsCurrentStageIsolated();
            string isolateLabel = isIsolated ? "Unisolate" : "Isolate";

            if (GUILayout.Button(isolateLabel))
            {
                if (isIsolated)
                {
                    SceneVisibilityManager.instance.ExitIsolation();
                }
                else
                {
                    foreach (GameObject obj in Selection.gameObjects)
                    {
                        SceneVisibilityManager.instance.Isolate(obj, true);
                    }
                }
            }
        }
    }
}
