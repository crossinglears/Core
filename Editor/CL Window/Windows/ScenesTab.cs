using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CrossingLearsEditor
{
    public class ScenesTab : CL_WindowTab
    {
        public override string TabName => "Scenes";

        private string[] allScenePaths;
        private Dictionary<string, List<string>> groupedScenes = new();
        private Dictionary<string, bool> foldoutStates = new();

        private List<string> alwaysActive = new();
        private const string AlwaysActiveEditorPrefsKey = "CLWindow.ScenesTab.AlwaysActive";

        public override void Awake()
        {
            base.Awake();
            LoadData();
        }

        public override void OnFocus()
        {
            base.OnFocus();
            LoadData();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (alwaysActive.Count > 0)
                EditorPrefs.SetString(AlwaysActiveEditorPrefsKey, string.Join(";", alwaysActive));
            else
                EditorPrefs.DeleteKey(AlwaysActiveEditorPrefsKey);
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
                var segments = path.Split('/');
                var group = segments.Length > 1 ? segments[^2] : "Other";

                if (!groupedScenes.ContainsKey(group))
                {
                    groupedScenes[group] = new List<string>();
                    foldoutStates[group] = true; // default to expanded
                }

                groupedScenes[group].Add(path);
            }

            if (EditorPrefs.HasKey(AlwaysActiveEditorPrefsKey))
                alwaysActive = EditorPrefs.GetString(AlwaysActiveEditorPrefsKey).Split(';').ToList();
            else
                alwaysActive.Clear();
        }

        public override void DrawTitle()
        {
            GUILayout.BeginHorizontal();
            base.DrawTitle();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Log All Always Active"))
            {
                foreach (var scene in alwaysActive)
                    Debug.Log(scene);
            }

            if (GUILayout.Button("Reset Always Active"))
            {
                EditorPrefs.DeleteKey(AlwaysActiveEditorPrefsKey);
                alwaysActive.Clear();
            }

            GUILayout.EndHorizontal();
        }

        public override void DrawContent()
        {
            EditorGUIUtility.labelWidth = 80;

            if (allScenePaths == null || allScenePaths.Length == 0)
            {
                EditorGUILayout.LabelField("No scenes found.");
                return;
            }

            var openScenes = new HashSet<string>();
            for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++)
                openScenes.Add(UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i).path);

            foreach (var group in groupedScenes)
            {
                foldoutStates.TryAdd(group.Key, true);
                foldoutStates[group.Key] = EditorGUILayout.Foldout(
                    foldoutStates[group.Key],
                    group.Key,
                    true,
                    EditorStyles.foldoutHeader
                );

                if (!foldoutStates[group.Key]) continue;

                foreach (string path in group.Value)
                {
                    bool isOpen = openScenes.Contains(path);
                    bool isInBuildSettings = EditorBuildSettings.scenes.Any(s => s.path == path);
                    bool isAlwaysActive = alwaysActive.Contains(path);
                    Color originalColor = GUI.color;

                    EditorGUILayout.BeginHorizontal();

                    // Toggle for Build Settings
                    bool toggle = EditorGUILayout.Toggle(isInBuildSettings, GUILayout.Width(20));
                    if (toggle != isInBuildSettings)
                    {
                        var buildScenes = EditorBuildSettings.scenes.ToList();

                        if (toggle)
                            buildScenes.Add(new EditorBuildSettingsScene(path, true));
                        else
                            buildScenes.RemoveAll(s => s.path == path);

                        EditorBuildSettings.scenes = buildScenes.ToArray();
                    }

                    // Scene Label with path tooltip
                    GUI.color = isOpen ? Color.cyan : originalColor;
                    Rect labelRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.MinWidth(60));
                    string fileName = System.IO.Path.GetFileName(path);
                    EditorGUI.LabelField(labelRect, new GUIContent(fileName, path));

                    // Context Menu
                    if (Event.current.type == EventType.ContextClick && labelRect.Contains(Event.current.mousePosition))
                    {
                        GenericMenu menu = new();
                        menu.AddItem(new GUIContent("Copy Path"), false, () => EditorGUIUtility.systemCopyBuffer = path);

                        if (isAlwaysActive)
                        {
                            menu.AddItem(new GUIContent("Remove Always Active"), false, () => alwaysActive.Remove(path));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Add to Always Active"), false, () =>
                            {
                                alwaysActive.Add(path);
                                IncludeAllRequiredScenes();
                            });
                        }

                        menu.ShowAsContext();
                        Event.current.Use();
                    }

                    if (isAlwaysActive)
                        GUILayout.Label(new GUIContent("A", "Always Active"), GUILayout.Width(20));

                    // Project View Button
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_Project"), GUILayout.Width(20)))
                    {
                        Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                        if (asset)
                        {
                            EditorApplication.ExecuteMenuItem("Window/General/Project");
                            Selection.activeObject = asset;
                            EditorGUIUtility.PingObject(asset);
                        }
                    }

                    // Open Button
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
                        IncludeAllRequiredScenes();
                    }

                    GUI.enabled = true;

                    // Close or Additive
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
                                    var scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
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

        private void IncludeAllRequiredScenes()
        {
            foreach (string path in alwaysActive)
            {
                if (!UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path).isLoaded)
                {
                    if (EditorApplication.isPlaying)
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(path, UnityEngine.SceneManagement.LoadSceneMode.Additive);
                    }
                    else
                    {
                        if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                        }
                    }
                }
            }
        }
    }
}
