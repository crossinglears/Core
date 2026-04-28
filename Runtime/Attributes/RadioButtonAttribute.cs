using System;
using UnityEngine;

namespace CrossingLears
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class RadioAttribute : PropertyAttribute
    {
        public readonly string[] BoolLabels;

        public RadioAttribute()
        {
            BoolLabels = new string[] { "True", "False" };
        }

        public RadioAttribute(string trueLabel, string falseLabel)
        {
            BoolLabels = new string[] { trueLabel, falseLabel };
        }
    }
}

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class RadioAttribute : CrossingLears.RadioAttribute
{
    public RadioAttribute()
    {
    }

    public RadioAttribute(string trueLabel, string falseLabel) : base(trueLabel, falseLabel)
    {
    }
}

#if UNITY_EDITOR
namespace CrossingLears
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(CrossingLears.RadioAttribute), true)]
    public class RadioDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CrossingLears.RadioAttribute radioAttribute = (CrossingLears.RadioAttribute)attribute;
            EditorGUI.BeginProperty(position, label, property);

            if (property.propertyType == SerializedPropertyType.Boolean)
            {
                DrawBoolean(position, property, label, radioAttribute);
            }
            else if (property.propertyType == SerializedPropertyType.Enum)
            {
                DrawEnum(position, property, label);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Radio works with bool and enum fields only");
            }

            EditorGUI.EndProperty();
        }

        private void DrawBoolean(Rect position, SerializedProperty property, GUIContent label, CrossingLears.RadioAttribute radioAttribute)
        {
            Rect contentRect = EditorGUI.PrefixLabel(position, label);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            string[] displayedLabels = new string[] { radioAttribute.BoolLabels[1], radioAttribute.BoolLabels[0] };
            int selectedIndex = property.boolValue ? 1 : 0;
            int nextIndex = GUI.Toolbar(contentRect, selectedIndex, displayedLabels);
            property.boolValue = nextIndex == 1;
            EditorGUI.indentLevel = indentLevel;
        }

        private void DrawEnum(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect contentRect = EditorGUI.PrefixLabel(position, label);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            property.enumValueIndex = GUI.Toolbar(contentRect, property.enumValueIndex, property.enumDisplayNames);
            EditorGUI.indentLevel = indentLevel;
        }
    }
}
#endif
