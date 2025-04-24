using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using CrossingLears;
using System.Collections.Generic;
using System.Collections;

namespace CrossingLearsEditor
{
    public static class EditorExtensions
    {
        public static void DrawButtons(this Editor editor)
        {
            IEnumerable<MethodInfo> methods = editor.target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetParameters().Length == 0);

            foreach (MethodInfo method in methods)
            {
                Attribute attribute = Attribute.GetCustomAttribute(method, typeof(ButtonAttribute));

                if (attribute != null)
                {             
                    var button = (ButtonAttribute)attribute;         
                    string name = string.IsNullOrEmpty(button.Name) ? ObjectNames.NicifyVariableName(method.Name) : button.Name;
                    
                    // if (GUILayout.Button(name))
                    // {
                    //     foreach (var t in editor.targets)
                    //     {
                    //         method.Invoke(t, null);
                    //     }
                    // }

                    if (GUILayout.Button(name))
                    {
                        foreach (var t in editor.targets)
                        {
                            var result = method.Invoke(t, null);
                            if (result is IEnumerator enumerator && t is MonoBehaviour mb)
                            {
                                mb.StartCoroutine(enumerator);
                            }
                        }
                    }
                }
            }
        }
    }
}
