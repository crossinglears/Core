using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CrossingLearsEditor
{
    public class ScenesTab : CL_WindowTab
    {
        public override string TabName => "Scenes";

        private string[] AllScenePaths;
        
        public override void Awake()
        {
            base.Awake();
            LoadData();
        }

        public List<string> AlwaysActive = new(); // These paths will always be active

        private const string AlwaysActiveEditorPrefsKey = "CLWindow.ScenesTab.AlwaysActive";

        public override void OnDisable()
        {
            base.OnDisable();
            if(AlwaysActive.Count > 0)
                EditorPrefs.SetString(AlwaysActiveEditorPrefsKey, string.Join(";", AlwaysActive));
            else
                EditorPrefs.DeleteKey(AlwaysActiveEditorPrefsKey);
        }

        private void LoadData()
        {
            GroupedScenes = new();
            // AllScenePaths = AssetDatabase.FindAssets("t:Scene")
            //     .Select(AssetDatabase.GUIDToAssetPath)
            //     .ToArray();
            // Prevalidate();

            AllScenePaths = AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => AssetDatabase.IsOpenForEdit(path))
                .ToArray();
            Prevalidate();

            if (EditorPrefs.HasKey(AlwaysActiveEditorPrefsKey))
                AlwaysActive = EditorPrefs.GetString(AlwaysActiveEditorPrefsKey).Split(';').ToList();
            else
                AlwaysActive = new();
        }

        public override void OnFocus()
        {
            base.OnFocus();
            LoadData();
        }

        private Dictionary<string, List<string>> GroupedScenes = new();

        void Prevalidate()
        {
            foreach(string path in AllScenePaths)
            {
                var split = path.Split('/');
                var key = split[split.Length - 2];

                if(GroupedScenes.TryGetValue(key, out List<string> stringList))
                {
                    stringList.Add(path);
                }
                else
                {
                    GroupedScenes.Add(key, new List<string>() {path});
                }
            }
        }

        public override void DrawContent()
        {
            EditorGUIUtility.labelWidth = 80;
            if (AllScenePaths == null || AllScenePaths.Length == 0)
            {
                EditorGUILayout.LabelField("No scenes found.");
                return;
            }

            var openScenes = new HashSet<string>();
            for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++)
            {
                openScenes.Add(UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i).path);
            }

            foreach (var kvp in GroupedScenes)
            {
                GUILayout.Label(kvp.Key, EditorStyles.boldLabel);
                
                foreach (string path in kvp.Value)
                {
                    bool isOpen = openScenes.Contains(path);
                    Color previousColor = GUI.color;

                    EditorGUILayout.BeginHorizontal();
                    var split = path.Split('/');
                    var key = split[split.Length - 1];

                    
                    // Add a checkbox that adds or remove this scene in the build scene list
                    bool isInBuildSettings = System.Array.Exists(EditorBuildSettings.scenes, s => s.path == path);
                    bool newState = EditorGUILayout.Toggle(isInBuildSettings, GUILayout.Width(20));

                    if (newState != isInBuildSettings)
                    {
                        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();

                        if (newState)
                        {
                            // Add scene to build settings
                            scenes.Add(new EditorBuildSettingsScene(path, true));
                        }
                        else
                        {
                            // Remove scene from build settings
                            scenes.RemoveAll(s => s.path == path);
                        }

                        EditorBuildSettings.scenes = scenes.ToArray();
                    }
                    
                    GUI.color = isOpen ? Color.cyan : previousColor;
                    Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.MinWidth(60));
                    EditorGUI.LabelField(rect, new GUIContent(key, path));

                    if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Copy Path"), false, () => EditorGUIUtility.systemCopyBuffer = path);
                        
                        if(AlwaysActive.Contains(path))
                        {
                            menu.AddItem(new GUIContent("Remove Always Active"), false, () => AlwaysActive.Remove(path));
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Add to Always Active"), false, () => 
                            {
                                AlwaysActive.Add(path);
                                IncludeAllRequiredScenes();
                            });
                        }
                        menu.ShowAsContext();
                        Event.current.Use();
                    }
                    if(AlwaysActive.Contains(path))
                    {
                        GUILayout.Label(new GUIContent("A", "Always Active"), GUILayout.Width(20));
                    }

                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_Project"), GUILayout.Width(20)))
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                        if (asset != null)
                        {
                            EditorApplication.ExecuteMenuItem("Window/General/Project");
                            EditorGUIUtility.PingObject(asset);
                            Selection.activeObject = asset;
                        }
                    }
                    bool NotInBuildSettings = !System.Array.Exists(EditorBuildSettings.scenes, s => s.path == path);
                    
                    if(NotInBuildSettings && EditorApplication.isPlaying) GUI.enabled = false;

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

                    if(isOpen)
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
                                if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                {
                                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                                }
                            }
                        }

                    }
                    GUI.enabled = true;

                    EditorGUILayout.EndHorizontal();
                    GUI.color = previousColor; // Restore original color
                }
                GUILayout.Space(10);
            }
            EditorGUIUtility.labelWidth = 0;
        }

        // public List<string> AlwaysActive = new(); // These paths will always be active
        private void IncludeAllRequiredScenes()
        {
            // Always active
            // loop the path list, if the scene is not loaded, load it additively
            
            foreach (string path in AlwaysActive)
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

        public override void DrawTitle()
        {
            GUILayout.BeginHorizontal();
            // GUILayout.Label(TabName, EditorStyles.boldLabel);
            base.DrawTitle();
            GUILayout.FlexibleSpace();

            if(GUILayout.Button("Log All Always Active"))
            {
                foreach(var item in AlwaysActive)
                {
                    Debug.Log(item);
                }
            }

            if(GUILayout.Button("Reset Always Active"))
            {
                EditorPrefs.DeleteKey(AlwaysActiveEditorPrefsKey);
                AlwaysActive.Clear();
            }
            GUILayout.EndHorizontal();
        }
    }
}
