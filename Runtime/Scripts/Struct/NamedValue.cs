using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrossingLears
{
    [Serializable]
    public class NamedValue<T>
    {
        public string name;
        public T value = default;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(NamedValue<>), true)]
    public class NamedValueDrawer : PropertyDrawer
    {
        private const float Spacing = 4f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var nameProp = property.FindPropertyRelative("name");
            var valueProp = property.FindPropertyRelative("value");

            position = EditorGUI.PrefixLabel(position, label);

            float halfWidth = (position.width - Spacing) * 0.5f;

            Rect nameRect = new Rect(position.x, position.y, halfWidth, position.height);
            Rect valueRect = new Rect(position.x + halfWidth + Spacing, position.y, halfWidth, position.height);

            EditorGUI.PropertyField(nameRect, nameProp, GUIContent.none);
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => EditorGUIUtility.singleLineHeight;
    }
#endif
}