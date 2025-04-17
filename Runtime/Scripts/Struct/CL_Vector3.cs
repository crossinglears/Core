using UnityEngine;

namespace CrossingLears
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;

    [CustomPropertyDrawer(typeof(CL_Vector3))]
    public class CL_Vector3Drawer : PropertyDrawer
    {
        static readonly Color handleColor = Color.cyan;
        static SerializedProperty currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valProp = property.FindPropertyRelative("value");
            EditorGUI.PropertyField(position, valProp, label);

            currentProperty = property;
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            if (currentProperty == null) return;

            var valProp = currentProperty.FindPropertyRelative("value");
            Vector3 pos = valProp.vector3Value;

            Handles.color = handleColor;
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(pos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(currentProperty.serializedObject.targetObject, "Move CL_Vector3 Handle");
                valProp.vector3Value = newPos;
                currentProperty.serializedObject.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(currentProperty.serializedObject.targetObject as GameObject != null 
                    ? ((GameObject)currentProperty.serializedObject.targetObject).scene 
                    : EditorSceneManager.GetActiveScene());
            }
        }
    }
#endif

    [System.Serializable]
    public struct CL_Vector3
    {
        public Vector3 value;

        public static implicit operator Vector3(CL_Vector3 v) => v.value;
        public static implicit operator CL_Vector3(Vector3 v) => new CL_Vector3 { value = v };
    }
}
