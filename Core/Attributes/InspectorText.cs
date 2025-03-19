using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CrossingLears
{
    public class InspectorTextAttribute : PropertyAttribute
    {
        public string Text;

        public InspectorTextAttribute(string text)
        {
            Text = text;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InspectorTextAttribute))]
    public class InspectorTextDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            InspectorTextAttribute textAttribute = (InspectorTextAttribute)attribute;

            // Draw the background box
            Rect boxRect = new Rect(position.x, position.y, position.width, position.height - 2);
            EditorGUI.DrawRect(boxRect, new Color(0.9f, 0.9f, 0.9f, 1f));

            // Draw the border
            Handles.color = Color.gray;
            Handles.DrawLine(new Vector3(boxRect.x, boxRect.y), new Vector3(boxRect.x + boxRect.width, boxRect.y)); // Top
            Handles.DrawLine(new Vector3(boxRect.x, boxRect.y + boxRect.height), new Vector3(boxRect.x + boxRect.width, boxRect.y + boxRect.height)); // Bottom
            Handles.DrawLine(new Vector3(boxRect.x, boxRect.y), new Vector3(boxRect.x, boxRect.y + boxRect.height)); // Left
            Handles.DrawLine(new Vector3(boxRect.x + boxRect.width, boxRect.y), new Vector3(boxRect.x + boxRect.width, boxRect.y + boxRect.height)); // Right

            // Draw the text
            Rect labelRect = new Rect(position.x + 5, position.y + 5, position.width - 10, position.height - 10);
            GUIStyle labelStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                normal = { textColor = Color.black }
            };
            EditorGUI.LabelField(labelRect, textAttribute.Text, labelStyle);
        }

        public override float GetHeight()
        {
            InspectorTextAttribute textAttribute = (InspectorTextAttribute)attribute;
            GUIStyle labelStyle = new GUIStyle(EditorStyles.wordWrappedLabel);
            float textHeight = labelStyle.CalcHeight(new GUIContent(textAttribute.Text), EditorGUIUtility.currentViewWidth - 40);
            return textHeight + 14; // Add padding
        }
    }
#endif
}
