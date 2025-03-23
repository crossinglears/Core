using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace CrossingLears
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

            foreach (var kvp in GroupedScenes)
            {
                GUILayout.Label(kvp.Key, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                
                foreach(string path in kvp.Value)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    var split = path.Split('/');
                    var key = split[split.Length - 1];
                    EditorGUILayout.LabelField(key, GUILayout.ExpandWidth(true), GUILayout.MinWidth(60));
                        
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_Project"), GUILayout.Width(20))) 
                    {
                        var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                        if (asset != null)
                        {
                            EditorGUIUtility.PingObject(asset);
                            Selection.activeObject = asset;
                        }
                    }
                    if (GUILayout.Button("Open Scene", GUILayout.Width(100)))
                    {
                        if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);
                        }
                    }

                    if (GUILayout.Button("Open Additive", GUILayout.Width(100)))
                    {
                        if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Additive);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
                GUILayout.Space(10);
            }
            EditorGUIUtility.labelWidth = 0;
        }
    }
}
