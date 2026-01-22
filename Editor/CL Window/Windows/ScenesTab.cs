using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CrossingLears.Editor
{
    public class ScenesTab : CL_WindowTab
    {
        public override string TabName => "Scenes";

        private string[] allScenePaths;
        private Dictionary<string, List<string>> groupedScenes = new Dictionary<string, List<string>>();
        private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();
        private string sceneFilter = "";

        public override void Awake()
        {
            base.Awake();
            LoadData();

            sceneFilter = EditorPrefs.GetString("CL_SceneTabFilter", "");
        }

        public override void OnFocus()
        {
            base.OnFocus();
            LoadData();
        }

        private void LoadData()
        {
            groupedScenes.Clear();
            foldoutStates.Clear();

            allScenePaths = AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => AssetDatabase.IsOpenForEdit(path))
                .ToArray();

            foreach (string path in allScenePaths)
            {
                string[] segments = path.Split('/');
                string group = segments.Length > 1 ? segments[segments.Length - 2] : "Other";

                if (!groupedScenes.ContainsKey(group))
                {
                    groupedScenes[group] = new List<string>();
                    foldoutStates[group] = true;
                }

                groupedScenes[group].Add(path);
            }
        }

        public override void DrawTitle()
        {
            GUILayout.BeginHorizontal();
            base.DrawTitle();

            Rect searchRect = GUILayoutUtility.GetRect(200, 20, GUILayout.ExpandWidth(false));

            GUIStyle searchStyle = GUI.skin.FindStyle("ToolbarSeachTextField") 
                ?? GUI.skin.FindStyle("ToolbarSearchTextField");

            sceneFilter = GUI.TextField(searchRect, sceneFilter, searchStyle);

            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            EditorPrefs.SetString("CL_SceneTabFilter", sceneFilter);
        }

private bool FuzzyMatch(string text, string filter)
{
    if (string.IsNullOrEmpty(filter)) return true;

    string lowerText = text.ToLower();
    string[] filters = filter.Split('|');

    for (int f = 0; f < filters.Length; f++)
    {
        string part = filters[f];
        if (string.IsNullOrEmpty(part)) continue;

        string lowerFilter = part.ToLower();

        int ti = 0;
        int fi = 0;

        while (ti < lowerText.Length && fi < lowerFilter.Length)
        {
            if (lowerText[ti] == lowerFilter[fi])
                fi++;
            ti++;
        }

        if (fi == lowerFilter.Length)
            return true;
    }

    return false;
}


public override void DrawContent()
{
    EditorGUIUtility.labelWidth = 80;

    if (allScenePaths == null || allScenePaths.Length == 0)
    {
        EditorGUILayout.LabelField("No scenes found.");
        return;
    }

    HashSet<string> openScenes = new HashSet<string>();
    for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++)
        openScenes.Add(UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i).path);

    foreach (KeyValuePair<string, List<string>> group in groupedScenes)
    {
        bool hasMatch = false;
        if (!string.IsNullOrEmpty(sceneFilter))
        {
            for (int i = 0; i < group.Value.Count; i++)
            {
                string p = group.Value[i];
                if (FuzzyMatch(p, sceneFilter))
                {
                    hasMatch = true;
                    break;
                }
            }

            if (!hasMatch)
                continue;
        }

        foldoutStates[group.Key] = EditorGUILayout.Foldout(
            foldoutStates[group.Key],
            group.Key,
            true,
            EditorStyles.foldoutHeader
        );

        if (!foldoutStates[group.Key]) continue;

        for (int i = 0; i < group.Value.Count; i++)
        {
            string path = group.Value[i];

            if (!FuzzyMatch(path, sceneFilter))
                continue;

            string fileName = System.IO.Path.GetFileName(path);
            bool isOpen = openScenes.Contains(path);
            bool isInBuildSettings = EditorBuildSettings.scenes.Any(s => s.path == path);
            Color originalColor = GUI.color;

            EditorGUILayout.BeginHorizontal();

            bool toggle = EditorGUILayout.Toggle(isInBuildSettings, GUILayout.Width(20));
            if (toggle != isInBuildSettings)
            {
                List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();

                if (toggle)
                    buildScenes.Add(new EditorBuildSettingsScene(path, true));
                else
                    buildScenes.RemoveAll(s => s.path == path);

                EditorBuildSettings.scenes = buildScenes.ToArray();
            }

            GUI.color = isOpen ? Color.cyan : originalColor;
            Rect labelRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.MinWidth(60));
            EditorGUI.LabelField(labelRect, new GUIContent(fileName, path));

            if (Event.current.type == EventType.ContextClick && labelRect.Contains(Event.current.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy Path"), false, () => EditorGUIUtility.systemCopyBuffer = path);
                menu.ShowAsContext();
                Event.current.Use();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Project"), GUILayout.Width(20)))
            {
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                if (asset != null)
                {
                    EditorApplication.ExecuteMenuItem("Window/General/Project");
                    Selection.activeObject = asset;
                    EditorGUIUtility.PingObject(asset);
                }
            }

            if (!isInBuildSettings && EditorApplication.isPlaying)
                GUI.enabled = false;

            if (GUILayout.Button("Open", GUILayout.Width(50)))
            {
                if (EditorApplication.isPlaying)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(path, UnityEngine.SceneManagement.LoadSceneMode.Single);
                }
                else
                {
                    if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
                    }
                }
            }

            GUI.enabled = true;

            if (isOpen)
            {
                if (GUILayout.Button("Close", GUILayout.Width(60)))
                {
                    if (EditorApplication.isPlaying)
                    {
                        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(path);
                    }
                    else
                    {
                        if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
                            UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
                        }
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Additive", GUILayout.Width(60)))
                {
                    if (EditorApplication.isPlaying)
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(path, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    }
                    else
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                    }
                }
            }

            GUI.color = originalColor;
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
    }

    EditorGUIUtility.labelWidth = 0;
}
    }
}
