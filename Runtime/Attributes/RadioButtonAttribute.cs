using System;
using UnityEngine;

namespace CrossingLears
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class RadioAttribute : PropertyAttribute
    {
        public readonly string[] BoolLabels;
        public readonly int[] IntValues;

        public RadioAttribute()
        {
            BoolLabels = new string[] { "True", "False" };
            IntValues = new int[0];
        }

        public RadioAttribute(string trueLabel, string falseLabel)
        {
            BoolLabels = new string[] { trueLabel, falseLabel };
            IntValues = new int[0];
        }

        public RadioAttribute(params int[] intValues)
        {
            BoolLabels = new string[] { "True", "False" };
            IntValues = intValues;
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

    public RadioAttribute(params int[] intValues) : base(intValues)
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
            else if (property.propertyType == SerializedPropertyType.Integer)
            {
                DrawInt(position, property, label, radioAttribute);
            }
            else if (property.propertyType == SerializedPropertyType.Enum)
            {
                DrawEnum(position, property, label);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Radio works with bool, int, and enum fields only");
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

        private void DrawInt(Rect position, SerializedProperty property, GUIContent label, CrossingLears.RadioAttribute radioAttribute)
        {
            if (radioAttribute.IntValues.Length == 0)
            {
                EditorGUI.LabelField(position, label.text, "Radio int fields need int parameters");
                return;
            }

            Rect contentRect = EditorGUI.PrefixLabel(position, label);
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            string[] displayedLabels = new string[radioAttribute.IntValues.Length];
            int selectedIndex = -1;

            for (int i = 0; i < radioAttribute.IntValues.Length; i++)
            {
                displayedLabels[i] = radioAttribute.IntValues[i].ToString();

                if (property.intValue == radioAttribute.IntValues[i])
                {
                    selectedIndex = i;
                }
            }

            int nextIndex = GUI.Toolbar(contentRect, selectedIndex, displayedLabels);

            if (nextIndex >= 0)
            {
                property.intValue = radioAttribute.IntValues[nextIndex];
            }

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
