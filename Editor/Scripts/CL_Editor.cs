using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class CL_Editor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            this.DrawButtons();
            DrawDefaultInspector();
        }
    }

    public static class CL_Design
    {
        public static Color brown = new Color32(62, 52, 27, 255);
        public static Color gold = new Color32(135, 126, 84, 255);

        public static void DrawColoredBox(Color bgColor, System.Action content)
        {
            Rect rect = EditorGUILayout.BeginVertical();
            {
                EditorGUI.DrawRect(rect, bgColor);

                content?.Invoke();
            }
            EditorGUILayout.EndVertical();
        }

        public static GUIStyle BrownTextLabel;
        static CL_Design()
        {
            BrownTextLabel = new GUIStyle(EditorStyles.label);
            BrownTextLabel.normal.textColor = CL_Design.brown;
        }
    }
}
