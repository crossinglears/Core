using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public partial class LevelDesignTab : CL_WindowTab_WIP
    {
        public override string TabName => "Level Design";

        public override void OnUnfocus()
        {
            base.OnUnfocus();
            roamingFoldout = false;
            SceneView.duringSceneGui -= FreeCamUpdate;
        }

        public Color ActiveColor = Color.cyan;
        public Color DefaultColor = Color.white;

        public override void DrawContent()
        {
            GUILayout.Space(10);

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

            GUILayout.Space(10);

            FreeCamFoldout();
        }

    }
}
