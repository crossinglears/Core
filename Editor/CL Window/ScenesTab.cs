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

        public override void OnFocus()
        {
            base.OnFocus();
            GroupedScenes = new();
            AllScenePaths = AssetDatabase.FindAssets("t:Scene")
                .Select(AssetDatabase.GUIDToAssetPath)
                .ToArray();
            Prevalidate();
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
                    // EditorGUILayout.LabelField(key, GUILayout.ExpandWidth(true), GUILayout.MinWidth(60));
                    EditorGUILayout.LabelField(new GUIContent(key, path), GUILayout.ExpandWidth(true), GUILayout.MinWidth(60));

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
    }
}
