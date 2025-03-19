using UnityEngine;

namespace CrossingLears
{
    #if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
    #endif


    public class ReadOnlyAttribute : PropertyAttribute { }
}