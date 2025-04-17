using UnityEngine;

namespace CrossingLears
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.SceneManagement;

    [CustomPropertyDrawer(typeof(CL_Vector2))]
    public class CL_Vector2Drawer : PropertyDrawer
    {
        static readonly Color handleColor = Color.green;
        static SerializedProperty currentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valProp = property.FindPropertyRelative("value");
            EditorGUI.PropertyField(position, valProp, label);

            currentProperty = property;
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView s)
        {            
            if (currentProperty == null || currentProperty.serializedObject?.targetObject == null)
            {
                SceneView.duringSceneGui -= OnSceneGUI;
                return;
            }

            SerializedProperty valProp = currentProperty.FindPropertyRelative("value");
            Vector2 vec2 = valProp.vector2Value;
            Vector3 pos = new Vector3(vec2.x, vec2.y, 0f);

            Handles.color = handleColor;
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(pos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(currentProperty.serializedObject.targetObject, "Move CL_Vector2 Handle");
                valProp.vector2Value = new Vector2(newPos.x, newPos.y);
                currentProperty.serializedObject.ApplyModifiedProperties();
                EditorSceneManager.MarkSceneDirty(currentProperty.serializedObject.targetObject as GameObject != null 
                    ? ((GameObject)currentProperty.serializedObject.targetObject).scene 
                    : EditorSceneManager.GetActiveScene());
            }
        }
    }
#endif

    [System.Serializable]
    public struct CL_Vector2
    {
        public Vector2 value;

        public static implicit operator Vector2(CL_Vector2 v) => v.value;
        public static implicit operator CL_Vector2(Vector2 v) => new CL_Vector2 { value = v };
    }
}
