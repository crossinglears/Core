using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace CrossingLears
{
    public class Autograb
    {
        [MenuItem("CONTEXT/Component/AutoGrab (GameObject)")]
        public static void AutoGrabComponentsFromGameObject(MenuCommand command)
        {
            Component component = (Component)command.context;
            Undo.RecordObject(component, "AutoGrab (GameObject)");
            AssignComponents(component, component.GetComponentInChildren);
            EditorUtility.SetDirty(component);
        }

        [MenuItem("CONTEXT/Component/AutoGrab (Scene)")]
        public static void AutoGrabComponentsFromScene(MenuCommand command)
        {
            Component component = (Component)command.context;
            Undo.RecordObject(component, "AutoGrab (Scene)");
            AssignComponents(component, type => (Component)Object.FindAnyObjectByType(type));
            EditorUtility.SetDirty(component);
        }

        private static void AssignComponents(Component targetComponent, System.Func<System.Type, Component> searchFunc)
        {
            FieldInfo[] fields = targetComponent.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (field.GetValue(targetComponent) == null && typeof(Component).IsAssignableFrom(field.FieldType))
                {
                    Component foundComponent = searchFunc(field.FieldType);

                    if (foundComponent != null)
                    {
                        Undo.RecordObject(targetComponent, $"Assign {field.Name}"); // Track field change
                        field.SetValue(targetComponent, foundComponent);
                        Debug.Log($"[AutoGrab] Assigned {field.FieldType.Name} to {field.Name} on {targetComponent.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[AutoGrab] No matching {field.FieldType.Name} found for {field.Name} on {targetComponent.name}");
                    }
                }
            }
        }
    }
}
