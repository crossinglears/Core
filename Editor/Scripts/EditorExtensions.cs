using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using CrossingLears;

namespace CrossingLearsEditor
{
    public static class EditorExtensions
    {
        public static void DrawButtons(this Editor editor)
        {
            var methods = editor.target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetParameters().Length == 0);

            foreach (var method in methods)
            {
                var attribute = Attribute.GetCustomAttribute(method, typeof(ButtonAttribute));

                if (attribute != null)
                {             
                    var button = (ButtonAttribute)attribute;         
                    string name = string.IsNullOrEmpty(button.Name) ? ObjectNames.NicifyVariableName(method.Name) : button.Name;
                    if (GUILayout.Button(name))
                    {
                        foreach (var t in editor.targets)
                        {
                            method.Invoke(t, null);
                        }
                    }
                }
            }
        }
    }
}
