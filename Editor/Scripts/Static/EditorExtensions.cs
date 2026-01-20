using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using CrossingLears;
using System.Collections;
using System.Collections.Generic;

namespace CrossingLearsEditor
{
    public static class EditorExtensions
    {
        private static Dictionary<string, object[]> cachedArgs = new Dictionary<string, object[]>();

        public static void DrawButtons(this Editor editor)
        {
            IEnumerable<MethodInfo> methods = editor.target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                Attribute attribute = Attribute.GetCustomAttribute(method, typeof(ButtonAttribute));
                if (attribute == null)
                    continue;

                ParameterInfo[] parameters = method.GetParameters();
                ButtonAttribute button = (ButtonAttribute)attribute;

                string name = string.IsNullOrEmpty(button.Name)
                    ? ObjectNames.NicifyVariableName(method.Name)
                    : button.Name;

                string key = editor.target.GetInstanceID() + "_" + method.MetadataToken;

                if (!cachedArgs.TryGetValue(key, out object[] args))
                {
                    args = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        Type type = parameters[i].ParameterType;
                        args[i] = type.IsValueType ? Activator.CreateInstance(type) : null;
                    }
                    cachedArgs[key] = args;
                }

                EditorGUILayout.BeginHorizontal();

                if (parameters.Length == 0)
                {
                    if (GUILayout.Button(name, GUILayout.ExpandWidth(true)))
                    {
                        foreach (UnityEngine.Object t in editor.targets)
                        {
                            object result = method.Invoke(t, null);

                            if (result is IEnumerator enumerator && t is MonoBehaviour mb)
                                mb.StartCoroutine(enumerator);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                    continue;
                }

                if (GUILayout.Button(name, GUILayout.Width(118f)))
                {
                    foreach (UnityEngine.Object t in editor.targets)
                    {
                        object result = method.Invoke(t, args);

                        if (result is IEnumerator enumerator && t is MonoBehaviour mb)
                            mb.StartCoroutine(enumerator);
                    }
                }

                for (int i = 0; i < parameters.Length; i++)
                {
                    GUILayout.Space(2f);

                    Type type = parameters[i].ParameterType;

                    if (type == typeof(int))
                        args[i] = EditorGUILayout.IntField((int)args[i]);
                    else if (type == typeof(float))
                        args[i] = EditorGUILayout.FloatField((float)args[i]);
                    else if (type == typeof(bool))
                        args[i] = EditorGUILayout.Toggle((bool)args[i]);
                    else if (type == typeof(string))
                        args[i] = EditorGUILayout.TextField((string)args[i]);
                    else if (type == typeof(Vector2))
                        args[i] = EditorGUILayout.Vector2Field(GUIContent.none, (Vector2)args[i]);
                    else if (type == typeof(Vector3))
                        args[i] = EditorGUILayout.Vector3Field(GUIContent.none, (Vector3)args[i]);
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                        args[i] = EditorGUILayout.ObjectField((UnityEngine.Object)args[i], type, true);
                }

                GUILayout.Space(2f);
                EditorGUILayout.EndHorizontal();
            }
        }

    }
}