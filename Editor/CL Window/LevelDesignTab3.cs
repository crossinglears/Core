using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public partial class LevelDesignTab : CL_WindowTab_WIP
    {
        void InformationArea()
        {
            GameObject selected = Selection.activeGameObject;

            EditorGUILayout.LabelField("Selected Object Info", EditorStyles.boldLabel);

            if (selected != null)
            {
                Transform t = selected.transform;

                EditorGUILayout.LabelField("Name", selected.name);
                EditorGUILayout.Vector3Field("World Position", t.position);
                EditorGUILayout.Vector3Field("Local Position", t.localPosition);
            }
            else
            {
                EditorGUILayout.LabelField("No object selected.");
            }
        }
    }
}
