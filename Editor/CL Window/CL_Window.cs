using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CrossingLears
{
    public abstract class CL_Tab
    {
        public abstract string TabName { get; }
        public abstract void DrawContent();
    }

    public class CL_Window : EditorWindow
    {
        private int selectedTab = 0;
        private List<CL_Tab> tabs = new List<CL_Tab> 
        { 
            new GeneralTab(),
            new EventsTab(),
            new PackagesTab()
        };

        [MenuItem("Crossing Lears/Open Window")]
        public static void ShowWindow()
        {
            GetWindow<CL_Window>("Crossing Lears");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();

            // Left Tab Section (Fixed Width)
            EditorGUILayout.BeginVertical(GUILayout.Width(100));
            GUILayout.Space(10);

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
                }
            }

            EditorGUILayout.EndVertical();

            // Vertical Line Separator
            Color lineColor = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.6f, 0.6f, 0.6f);
            Rect lineRect = GUILayoutUtility.GetRect(2, Screen.height, GUILayout.Width(2));
            EditorGUI.DrawRect(lineRect, lineColor);

            // Right Content Section
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.Label($"{tabs[selectedTab].TabName}", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            tabs[selectedTab].DrawContent();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        // Helper method to create a texture for selected tab background
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