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
        public static string Message = "";
        internal static CL_Window current;

        private int selectedTab = 0;
        internal List<CL_WindowTab> tabs = new List<CL_WindowTab>();
        private Vector2 leftScrollPos;
        private Vector2 rightScrollPos;
        private static Texture2D cachedTexture;

        internal List<string> IgnoredTabs = new();
        private bool isPaused;
        private const string IGNOREDTABSKEY = "CL_Window.IgnoredTabs";

        [MenuItem("Crossing Lears/Toolbox")]
        public static void ShowWindow() => current = GetWindow<CL_Window>("Crossing Toolbox");

        [InitializeOnLoadMethod]
        static void OnScriptReloaded() => EditorApplication.delayCall += () => current?.Awake();

        private void Awake()
        {
            LoadTabs();
            if (EditorPrefs.HasKey("CL_WindowTabsOrder"))
            {
                string[] savedNames = EditorPrefs.GetString("CL_WindowTabsOrder").Split(';');
                tabs.Sort((a, b) => Array.IndexOf(savedNames, a.TabName).CompareTo(Array.IndexOf(savedNames, b.TabName)));
            }

            CL_WindowTab generalTab = tabs.Find(tab => tab is GeneralTab);
            if (generalTab != null)
            {
                tabs.Remove(generalTab);
                tabs.Insert(0, generalTab);
            }

            tabs.ForEach(tab => tab.Awake());
        }

        private void OnEnable()
        {
            current = this;
            IgnoredTabs = EditorPrefs.GetString(IGNOREDTABSKEY).Split('+').ToList();
            LoadTabs();
            if (tabs.Count > 0) tabs[selectedTab].OnEnable();
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(IGNOREDTABSKEY, string.Join("+", IgnoredTabs));
            if (tabs.Count > 0) tabs[selectedTab].OnDisable();
            EditorPrefs.SetString("CL_WindowTabsOrder", string.Join(";", tabs.ConvertAll(tab => tab.TabName)));
        }

        private void OnFocus()
        {
            if (tabs.Count > 0) tabs[selectedTab].OnFocus();
        }

        private void LoadTabs()
        {
            if (tabs == null || tabs.Count == 0)
            {
                tabs = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(CL_WindowTab).IsAssignableFrom(t))
                    .Select(t => (CL_WindowTab)Activator.CreateInstance(t))
                    .ToList();
            }
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                GUI.FocusControl(null);

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            DrawLeftPanel();
            DrawVerticalSeparator();
            DrawRightPanel();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // EditorGUILayout.BeginVertical();
            // DrawBottomPanel();
            // EditorGUILayout.EndVertical();
        }

        private void DrawVerticalSeparator()
        {
            Rect rect = GUILayoutUtility.GetRect(2, 0, GUILayout.ExpandHeight(true), GUILayout.Width(2));
            Color lineColor = EditorGUIUtility.isProSkin ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.6f, 0.6f, 0.6f);
            EditorGUI.DrawRect(rect, lineColor);
        }

        private void DrawBottomPanel()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, GUILayout.Height(25));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(isPaused ? "Unpause" : "Pause", GUILayout.Width(80))) isPaused = !isPaused;
            GUILayout.Space(10);
            EditorGUILayout.EndHorizontal();
        }

        public const float leftPan = 115;

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(leftPan));
            GUILayout.Space(10);

            leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(leftPan), GUILayout.ExpandHeight(true));

            GUIStyle tabStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white },
                padding = new RectOffset(6, 3, 3, 3),
                stretchWidth = false
            };

            for (int i = 0; i < tabs.Count; i++)
            {
                CL_WindowTab item = tabs[i];
                if (IgnoredTabs.Contains(item.TabName)) continue;

                GUIStyle currentStyle = new GUIStyle(tabStyle);
                if (selectedTab == i) currentStyle.normal.background = GetTabBackgroundColor();

                if (GUILayout.Button(item.TabName, currentStyle, GUILayout.Width(leftPan - 10), GUILayout.Height(25)))
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

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(10);

            tabs[selectedTab].DrawTitle();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5);
            rightScrollPos = EditorGUILayout.BeginScrollView(rightScrollPos, GUILayout.ExpandHeight(true));
            tabs[selectedTab].DrawContent();
            GUILayout.Space(20);
            EditorGUILayout.EndScrollView();
            GUILayout.Space(5);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private static Texture2D GetTabBackgroundColor()
        {
            if (cachedTexture == null)
            {
                cachedTexture = new Texture2D(1, 1);
                cachedTexture.SetPixel(0, 0, EditorGUIUtility.isProSkin ? new Color(0.35f, 0.35f, 0.35f) : new Color(0.9f, 0.9f, 0.9f));
                cachedTexture.Apply();
            }
            return cachedTexture;
        }
    }
}
