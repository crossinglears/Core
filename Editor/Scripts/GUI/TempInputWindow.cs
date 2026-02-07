using System;
using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public sealed class TempInputRequest
    {
        internal string Title;
        internal Type[] Types;
        internal Action<object[]> OnSubmit;
        internal string SubmitMessage;
        internal string[] Labels;
        internal object[] DefaultValues;

        public TempInputRequest Label(params string[] labels)
        {
            Labels = labels;
            return this;
        }

        public TempInputRequest DefaultValue(params object[] values)
        {
            DefaultValues = values;
            return this;
        }

        public void Show()
        {
            TempInputEditorWindow.ShowWindow(
                Title,
                Types,
                OnSubmit,
                SubmitMessage,
                Labels,
                DefaultValues);
        }
    }

    public static class TempInputWindow
    {
        public static TempInputRequest ShowWindow<T1>(
            string title,
            Action<T1> onSubmit,
            string submitMessage = "Submit")
        {
            return Create(
                title,
                new Type[] { typeof(T1) },
                values => onSubmit((T1)values[0]),
                submitMessage);
        }

        public static TempInputRequest ShowWindow<T1, T2>(
            string title,
            Action<T1, T2> onSubmit,
            string submitMessage = "Submit")
        {
            return Create(
                title,
                new Type[] { typeof(T1), typeof(T2) },
                values => onSubmit((T1)values[0], (T2)values[1]),
                submitMessage);
        }

        public static TempInputRequest ShowWindow<T1, T2, T3>(
            string title,
            Action<T1, T2, T3> onSubmit,
            string submitMessage = "Submit")
        {
            return Create(
                title,
                new Type[] { typeof(T1), typeof(T2), typeof(T3) },
                values => onSubmit((T1)values[0], (T2)values[1], (T3)values[2]),
                submitMessage);
        }

        public static TempInputRequest ShowWindow<T1, T2, T3, T4>(
            string title,
            Action<T1, T2, T3, T4> onSubmit,
            string submitMessage = "Submit")
        {
            return Create(
                title,
                new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) },
                values => onSubmit((T1)values[0], (T2)values[1], (T3)values[2], (T4)values[3]),
                submitMessage);
        }

        private static TempInputRequest Create(
            string title,
            Type[] types,
            Action<object[]> onSubmit,
            string submitMessage)
        {
            TempInputRequest request = new TempInputRequest();
            request.Title = title;
            request.Types = types;
            request.OnSubmit = onSubmit;
            request.SubmitMessage = submitMessage;
            return request;
        }
    }

    internal class TempInputEditorWindow : EditorWindow
    {
        private Type[] typesAsked;
        private Action<object[]> onSubmit;
        private string submitMessage;
        private string[] labels;
        private object[] values;

        public static void ShowWindow(
            string title,
            Type[] typesAsked,
            Action<object[]> onSubmit,
            string submitMessage,
            string[] labels,
            object[] defaultValues)
        {
            TempInputEditorWindow window = CreateInstance<TempInputEditorWindow>();
            window.titleContent = new GUIContent(title);
            window.typesAsked = typesAsked;
            window.onSubmit = onSubmit;
            window.submitMessage = submitMessage;
            window.labels = labels;
            window.values = new object[typesAsked.Length];

            if (defaultValues != null)
            {
                int count = Mathf.Min(defaultValues.Length, window.values.Length);
                for (int i = 0; i < count; i++)
                {
                    window.values[i] = defaultValues[i];
                }
            }

            window.minSize = new Vector2(260f, 80f);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            for (int i = 0; i < typesAsked.Length; i++)
            {
                string label = labels != null && i < labels.Length ? labels[i] : string.Empty;
                Type type = typesAsked[i];

                if (type == typeof(int))
                {
                    values[i] = EditorGUILayout.IntField(label, values[i] != null ? (int)values[i] : 0);
                }
                else if (type == typeof(float))
                {
                    values[i] = EditorGUILayout.FloatField(label, values[i] != null ? (float)values[i] : 0f);
                }
                else if (type == typeof(string))
                {
                    values[i] = EditorGUILayout.TextField(label, values[i] != null ? (string)values[i] : string.Empty);
                }
                else if (type == typeof(bool))
                {
                    values[i] = EditorGUILayout.Toggle(label, values[i] != null && (bool)values[i]);
                }
                else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                {
                    values[i] = EditorGUILayout.ObjectField(
                        label,
                        values[i] as UnityEngine.Object,
                        type,
                        false);
                }
            }

            GUILayout.Space(8f);

            if (GUILayout.Button(submitMessage))
            {
                onSubmit(values);
                Close();
            }
        }
    }
}
