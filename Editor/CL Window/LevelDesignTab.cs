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
            freeMoveEnabled = false;
            SceneView.duringSceneGui -= FreeMoveUpdate;
        }

        public Color ActiveColor = Color.cyan;
        public Color DefaultColor = Color.white;

        public override void DrawContent()
        {
            GUILayout.Space(10);

            IsolateButton();

            GUILayout.Space(10);

            FreeMoveButton();

            GUILayout.Space(10);

            InformationArea();
        }
    }
}
