using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public partial class SelectionTab : CL_WindowTab
    {
        Vector2 ScatterSpacing = new Vector2(1, 1);
        int ScatterDimensions = 3;
        bool ScatterCentered = true;
        int ScatterOrientation = 0;

        public void ScatterChildren()
        {
            GUILayout.Space(10);
            Transform parent = Selection.activeTransform;
            if (parent == null)
            {
                GUI.enabled = false;
            }
            bool willScatter = GUILayout.Button("Scatter Children");
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Spacing", GUILayout.Width(EditorGUIUtility.labelWidth));
            ScatterSpacing.x = EditorGUILayout.FloatField(ScatterSpacing.x);
            ScatterSpacing.y = EditorGUILayout.FloatField(ScatterSpacing.y);
            EditorGUILayout.EndHorizontal();
                    
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Columns", GUILayout.Width(EditorGUIUtility.labelWidth));
            ScatterDimensions = EditorGUILayout.IntField(ScatterDimensions);
            if (GUILayout.Button("Automatic"))
            {
                Transform autoParent = Selection.activeTransform;
                if (autoParent != null)
                {
                    int count = autoParent.childCount;
                    ScatterDimensions = Mathf.CeilToInt(Mathf.Sqrt(count));
                }
            }
            EditorGUILayout.EndHorizontal();

            
            EditorGUILayout.BeginHorizontal();
            ScatterOrientation = EditorGUILayout.Popup("Orientation", ScatterOrientation, new[] { "XZ", "XY", "YZ" });
            ScatterCentered = EditorGUILayout.Toggle("Centered", ScatterCentered);
            EditorGUILayout.EndHorizontal();

            if (!willScatter)
            {
                return;
            }

            int childCount = parent.childCount;
            if (childCount == 0)
            {
                return;
            }

            Vector3[] positions = new Vector3[childCount];

            for (int i = 0; i < childCount; i++)
            {
                int xIndex = i % ScatterDimensions;
                int yIndex = i / ScatterDimensions;

                float x = 0f;
                float y = 0f;
                float z = 0f;

                if (ScatterOrientation == 0)
                {
                    x = xIndex * ScatterSpacing.x;
                    z = yIndex * ScatterSpacing.y;
                }
                else if (ScatterOrientation == 1)
                {
                    x = xIndex * ScatterSpacing.x;
                    y = yIndex * ScatterSpacing.y;
                }
                else
                {
                    y = xIndex * ScatterSpacing.x;
                    z = yIndex * ScatterSpacing.y;
                }

                positions[i] = new Vector3(x, y, z);
            }

            Vector3 offset = Vector3.zero;

            if (ScatterCentered)
            {
                Vector3 sum = Vector3.zero;
                for (int i = 0; i < positions.Length; i++)
                {
                    sum += positions[i];
                }
                offset = sum / positions.Length;
            }

            for (int i = 0; i < childCount; i++)
            {
                Transform child = parent.GetChild(i);
                Undo.RecordObject(child, "Scatter Children");
                child.localPosition = positions[i] - offset;
            }
        }
    }
}