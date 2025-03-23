using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrossingLears
{
    public abstract class CL_WindowTab
    {
        public abstract string TabName { get; }
        public abstract void DrawContent();

        public virtual void OnFocus() { }
        public virtual void OnEnable() { }
    }

    public class CL_Window : EditorWindow
    {
        private int selectedTab = 0;
        private List<CL_WindowTab> tabs = new List<CL_WindowTab>();
        private Vector2 leftScrollPos;
        private Vector2 rightScrollPos;

        [MenuItem("Crossing Lears/Open Window")]
        public static void ShowWindow()
        {
            GetWindow<CL_Window>("Crossing Lears");
        }

        private void OnEnable()
        {
            LoadTabs();
            if (tabs.Count > 0)
                tabs[selectedTab].OnEnable();
        }

        private void OnFocus()
        {
            if (tabs.Count > 0)
                tabs[selectedTab].OnFocus();
        }

        private void LoadTabs()
        {
            tabs.Clear();

            IEnumerable<Type> tabTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && typeof(CL_WindowTab).IsAssignableFrom(t));

            foreach (Type type in tabTypes)
            {
                if (Activator.CreateInstance(type) is CL_WindowTab tabInstance)
                {
                    tabs.Add(tabInstance);
                }
            }
        }

        private void OnGUI()
        {
            if (tabs.Count == 0)
            {
                EditorGUILayout.LabelField("No tabs available.", EditorStyles.boldLabel);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            // Left Side
            EditorGUILayout.BeginVertical(GUILayout.Width(120));
            GUILayout.Space(10);
            
            leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(120), GUILayout.ExpandHeight(true));
            
            for (int i = 0; i < tabs.Count; i++)
            {
                GUIStyle tabStyle = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    normal = { textColor = Color.white },
                    padding = new RectOffset(10, 10, 5, 5)
                };

                if (selectedTab == i)
                {
                    tabStyle.normal.background = EditorGUIUtility.isProSkin
                        ? MakeTexture(1, 1, new Color(0.35f, 0.35f, 0.35f))
                        : MakeTexture(1, 1, new Color(0.9f, 0.9f, 0.9f));
                }

                if (GUILayout.Button(tabs[i].TabName, tabStyle, GUILayout.ExpandWidth(true), GUILayout.Height(25)))
                {
                    selectedTab = i;
                    tabs[selectedTab].OnFocus();
                }
            }
            
            GUILayout.Space(30); 
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Vertical Line Separator
            Color lineColor = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.6f, 0.6f, 0.6f);
            Rect lineRect = GUILayoutUtility.GetRect(2, Screen.height, GUILayout.Width(2));
            EditorGUI.DrawRect(lineRect, lineColor);

            // Right Side
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.Label(tabs[selectedTab].TabName, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            rightScrollPos = EditorGUILayout.BeginScrollView(rightScrollPos, GUILayout.ExpandHeight(true));
            tabs[selectedTab].DrawContent();
            GUILayout.Space(20);
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            
            EditorGUILayout.EndHorizontal();
        }

        private static Texture2D MakeTexture(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(width, height);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}