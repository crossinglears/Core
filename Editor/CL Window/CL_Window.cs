using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossingLearsEditor
{
    public abstract class CL_WindowTab
    {
        public abstract string TabName { get; }
        public abstract void DrawContent();
        // public virtual int Order => 0;
        public virtual void OnFocus() { }
        public virtual void OnUnfocus() { }

        public virtual void OnEnable() { }
        public virtual void Awake() { }
        public virtual void OnDisable() { }

        public virtual void DrawTitle() { GUILayout.Label(TabName, EditorStyles.boldLabel); }
    }

    public abstract class CL_WindowTab_WIP : CL_WindowTab
    {
        public override void DrawTitle()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(TabName, EditorStyles.boldLabel);

            GUI.contentColor = Color.yellow;
            GUILayout.Label("⚠️ Feature in development", EditorStyles.boldLabel);
            GUI.contentColor = Color.white;

            GUILayout.EndHorizontal();
        }
    }

    public class CL_Window : EditorWindow
    {
        internal static CL_Window current;

        private int selectedTab = 0;
        internal List<CL_WindowTab> tabs = new List<CL_WindowTab>();
        private Vector2 leftScrollPos;
        private Vector2 rightScrollPos;
        private static Texture2D cachedTexture;

        internal List<string> IgnoredTabs = new();
        const string IGNOREDTABSKEY = "CL_Window.IgnoredTabs";

        [MenuItem("Crossing Lears/Toolbox")]
        public static void ShowWindow()
        {
            current = GetWindow<CL_Window>("Crossing Toolbox");
        }

        [InitializeOnLoadMethod]
        static void OnScriptReloaded()
        {
            EditorApplication.delayCall += () =>
            {
                current?.Awake();
            };
        }

        private void Awake()
        {
            LoadTabs();
            if (EditorPrefs.HasKey("CL_WindowTabsOrder"))
            {
                string saved = EditorPrefs.GetString("CL_WindowTabsOrder");
                string[] savedNames = saved.Split(';');
                tabs.Sort((a, b) =>
                {
                    int indexA = Array.IndexOf(savedNames, a.TabName);
                    int indexB = Array.IndexOf(savedNames, b.TabName);
                    return indexA.CompareTo(indexB);
                });
            }

            CL_WindowTab generalTab = tabs.Find(tab => tab is GeneralTab);
            if (generalTab != null)
            {
                tabs.Remove(generalTab);
                tabs.Insert(0, generalTab);
            }

            foreach (var item in tabs)
            {
                item.Awake();
            }
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(IGNOREDTABSKEY, string.Join("+", IgnoredTabs));
            if (tabs.Count > 0)
                tabs[selectedTab].OnDisable();
                
            string[] tabNames = tabs.ConvertAll(tab => tab.TabName).ToArray();
            EditorPrefs.SetString("CL_WindowTabsOrder", string.Join(";", tabNames));
        }

        private void OnEnable()
        {
            current = this;
            IgnoredTabs = EditorPrefs.GetString(IGNOREDTABSKEY).Split('+').ToList();
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
            if(tabs == null || tabs.Count == 0)
            tabs = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && typeof(CL_WindowTab).IsAssignableFrom(t))
                .Select(t => (CL_WindowTab)Activator.CreateInstance(t))
                // .OrderBy(tab => tab.Order)
                .ToList();
        }

        private void OnGUI()
        {
            if (tabs.Count == 0)
            {
                EditorGUILayout.LabelField("No tabs available.", EditorStyles.boldLabel);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            DrawLeftPanel();
            DrawSeparator();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();

            Repaint();
        }

        public const float leftPan = 115;
        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(leftPan)); // Reduced width by 2
            GUILayout.Space(10);
            leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos, GUILayout.Width(leftPan), GUILayout.ExpandHeight(true));

            GUIStyle tabStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                padding = new RectOffset(10, 10, 5, 5)
            };

            for (int i = 0; i < tabs.Count; i++)
            {
                CL_WindowTab item = tabs[i];
                if (IgnoredTabs.Contains(item.TabName))
                    continue;

                GUIStyle currentStyle = new GUIStyle(tabStyle);
                if (selectedTab == i)
                    currentStyle.normal.background = GetTabBackgroundColor();

                if (GUILayout.Button(item.TabName, currentStyle, GUILayout.ExpandWidth(true), GUILayout.Height(25)))
                {
                    if (selectedTab != i)
                    {
                        tabs[selectedTab]?.OnUnfocus();
                        item.OnDisable();
                        selectedTab = i;
                        item.OnEnable();
                        item.OnFocus();
                    }
                }
            }

            GUILayout.Space(30);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawSeparator()
        {
            Color lineColor = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.6f, 0.6f, 0.6f);
            Rect lineRect = GUILayoutUtility.GetRect(2, Screen.height, GUILayout.Width(2));
            EditorGUI.DrawRect(lineRect, lineColor);
        }

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            tabs[selectedTab].DrawTitle();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5); // Left margin
            rightScrollPos = EditorGUILayout.BeginScrollView(rightScrollPos, GUILayout.ExpandHeight(true));
            tabs[selectedTab].DrawContent();
            GUILayout.Space(20);
            EditorGUILayout.EndScrollView();
            GUILayout.Space(5); // Right margin
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }


        private static Texture2D GetTabBackgroundColor()
        {
            if (cachedTexture == null)
            {
                cachedTexture = new Texture2D(1, 1);
                Color color = EditorGUIUtility.isProSkin ? new Color(0.35f, 0.35f, 0.35f) : new Color(0.9f, 0.9f, 0.9f);
                cachedTexture.SetPixel(0, 0, color);
                cachedTexture.Apply();
            }
            return cachedTexture;
        }
    }
}