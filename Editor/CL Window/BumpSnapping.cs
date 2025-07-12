using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public class BumpSnapping : CL_WindowTab
    {
        public override string TabName => "Smart Bump Snap";

        private bool isEnabled = false;

        public override void DrawContent()
        {
            string label = isEnabled ? "Disable Smart Bump Snap" : "Enable Smart Bump Snap";
            if (GUILayout.Button(label, GUILayout.Height(30)))
            {
                isEnabled = !isEnabled;
                SceneView.RepaintAll();
            }

            if (isEnabled)
            {
                GUILayout.Label("Smart Bump Snapping is ACTIVE", EditorStyles.helpBox);
            }
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static bool isGloballyEnabled => EditorWindowTabs.SmartBumpSnapInstance?.isEnabled == true;

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!isGloballyEnabled) return;

            if (Selection.activeTransform == null) return;

            Transform t = Selection.activeTransform;

            Handles.color = Color.red;
            Handles.DrawWireCube(t.position, t.GetComponent<Renderer>()?.bounds.size ?? Vector3.one);

            // Simulated bump detection (non-physics based)
            Collider[] hits = Physics.OverlapBox(t.position, Vector3.one * 0.5f);
            foreach (Collider hit in hits)
            {
                if (hit.transform != t)
                {
                    Vector3 dir = (t.position - hit.ClosestPoint(t.position)).normalized;
                    t.position += dir * 0.01f; // Nudge away slightly
                    break;
                }
            }

            SceneView.RepaintAll();
        }

        // Used to access isEnabled from static method
        public static SmartBumpSnapTab Instance => EditorWindowTabs.SmartBumpSnapInstance as SmartBumpSnapTab;
    }

    // Optional: if you use a central tab manager
    public static class EditorWindowTabs
    {
        public static CL_WindowTab SmartBumpSnapInstance;
    }
}
