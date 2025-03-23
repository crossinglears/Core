using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CrossingLears
{
    public class EventsTab : CL_Tab
    {
        public override string TabName => "Events";

        private List<UnityEvent> events = new List<UnityEvent>
        {
            new UnityEvent(),
            new UnityEvent()
        };

        private string[] eventNames = { "On Start", "On End" };

        public override void DrawContent()
        {
            EditorGUILayout.LabelField("Event Triggers", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            for (int i = 0; i < events.Count; i++)
            {
                DrawUnityEvent(events[i], eventNames[i]);
            }
        }

        private void DrawUnityEvent(UnityEvent unityEvent, string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

            SerializedObject serializedEvent = new SerializedObject(new UnityEventWrapper(unityEvent));
            SerializedProperty eventProp = serializedEvent.FindProperty("eventField");

            EditorGUILayout.PropertyField(eventProp);
            serializedEvent.ApplyModifiedProperties();
        }

        private class UnityEventWrapper : ScriptableObject
        {
            public UnityEvent eventField;

            public UnityEventWrapper(UnityEvent unityEvent)
            {
                eventField = unityEvent;
            }
        }
    }
}
