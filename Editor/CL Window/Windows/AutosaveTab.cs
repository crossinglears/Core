using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System;
using System.IO;

namespace CrossingLearsEditor
{
    public class AutosaveTab : CL_WindowTab
    {
        private const string KEY_ENABLED = "CL_AUTOSAVE_ENABLED";
        private const string KEY_INTERVAL = "CL_AUTOSAVE_INTERVAL";
        private const string KEY_PATH = "CL_AUTOSAVE_PATH";
        private const string KEY_SAVEMODE = "CL_AUTOSAVE_SAVEMODE";
        private const string KEY_MAXSCENES = "CL_AUTOSAVE_MAXSCENES";

        private static double nextSaveTime;
        private static SaveMode saveMode;
        private static int maxScenes;

        private bool autosaveEnabled;
        private string saveDirectory;
        private float saveInterval;

        public override string TabName => "Auto Save";

        public override void Awake()
        {
            base.Awake();

            Debug.Log("AutosaveSystem init");

            autosaveEnabled = EditorPrefs.GetBool(KEY_ENABLED, false);
            saveInterval = EditorPrefs.GetFloat(KEY_INTERVAL, 3);
            saveDirectory = EditorPrefs.GetString(KEY_PATH, "Autosaves");
            maxScenes = EditorPrefs.GetInt(KEY_MAXSCENES, 5);
            saveMode = (SaveMode)EditorPrefs.GetInt(KEY_SAVEMODE, 0);

            if (autosaveEnabled)
                EditorApplication.update += Update;
            else
                EditorApplication.update -= Update;

            ScheduleNext();
        }

        private static void ScheduleNext()
        {
            float minutes = EditorPrefs.GetFloat(KEY_INTERVAL, 5);
            nextSaveTime = EditorApplication.timeSinceStartup + (minutes * 60);
        }

        public static void Update()
        {
            if (EditorApplication.timeSinceStartup >= nextSaveTime)
            {
                SaveNow();
                ScheduleNext();
            }
        }

        public static void SaveNow()
        {
            string folder = EditorPrefs.GetString(KEY_PATH, "Autosaves");
            if (!folder.StartsWith("Assets")) folder = "Assets/" + folder;
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", folder.Replace("Assets/", ""));

            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string stamp = DateTime.Now.ToString("yy-MM-dd-HH-mm-ss");
            string path;

            switch (saveMode)
            {
                case SaveMode.KeepEverything:
                    path = $"{folder}/{sceneName} {stamp}.unity";
                    EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), path, true);
                    break;

                case SaveMode.KeepFew:
                    string[] files = Directory.GetFiles(folder, "*.unity");
                    if (files.Length >= maxScenes)
                    {
                        Array.Sort(files, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));
                        File.Delete(files[0]);
                    }
                    path = $"{folder}/{sceneName} {stamp}.unity";
                    EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), path, true);
                    break;

                case SaveMode.KeepOnlyTheLatest:
                    path = $"{folder}/{sceneName}(autosaved).unity";
                    EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), path, true);
                    break;
            }

            AssetDatabase.Refresh();
            Debug.Log("Scene autosaved");
        }

        public override void DrawContent()
        {
            bool newEnabled = EditorGUILayout.Toggle("Enable Autosave", autosaveEnabled);

            // if (saveMode == SaveMode.KeepFew)
            // {
            //     GUILayout.BeginHorizontal();
            //     int newMaxScenes = EditorGUILayout.IntField(maxScenes, GUILayout.Width(60));
            //     if (newMaxScenes != maxScenes)
            //     {
            //         maxScenes = newMaxScenes;
            //         EditorPrefs.SetInt(KEY_MAXSCENES, maxScenes);
            //     }
            //     GUILayout.EndHorizontal();
            // }
            
            SaveMode newsaveMode;
            switch (saveMode)
            {
                case SaveMode.KeepFew:
                {
                    GUILayout.BeginHorizontal();

                    newsaveMode = (SaveMode)EditorGUILayout.EnumPopup("Save Mode", saveMode);
                    if (saveMode != newsaveMode)
                    {
                        saveMode = newsaveMode;
                        EditorPrefs.SetInt("CL_AUTOSAVE_SAVEMODE", (int)newsaveMode);
                    }

                    int newMaxScenes = EditorGUILayout.IntField(maxScenes, GUILayout.Width(60));
                    if (maxScenes != newMaxScenes)
                    {
                        maxScenes = newMaxScenes;
                        EditorPrefs.SetInt(KEY_MAXSCENES, maxScenes);
                    }

                    GUILayout.EndHorizontal();
                    break;
                }
                default:
                {
                    newsaveMode = (SaveMode)EditorGUILayout.EnumPopup("Save Mode", saveMode);
                    break;
                }
            }

            if (saveMode != newsaveMode)
            {
                saveMode = newsaveMode;
                EditorPrefs.SetInt("CL_AUTOSAVE_SAVEMODE", (int)newsaveMode);
            }


            if (newEnabled != autosaveEnabled)
            {
                autosaveEnabled = newEnabled;
                if (autosaveEnabled) EditorApplication.update += Update;
                else EditorApplication.update -= Update;
                EditorPrefs.SetBool(KEY_ENABLED, autosaveEnabled);
            }

            GUILayout.BeginHorizontal();
            string newDir = EditorGUILayout.TextField("Save Directory", saveDirectory);
            if (GUILayout.Button("Locate"))
            {
                string folderPath = saveDirectory.StartsWith("Assets") ? saveDirectory : "Assets/" + saveDirectory;
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
                if (obj != null) EditorGUIUtility.PingObject(obj);
            }
            GUILayout.EndHorizontal();

            if (newDir != saveDirectory)
            {
                saveDirectory = newDir;
                EditorPrefs.SetString(KEY_PATH, saveDirectory);
            }

            float newInterval = EditorGUILayout.FloatField("Save Interval (minutes)", saveInterval);
            if (newInterval != saveInterval)
            {
                saveInterval = Mathf.Max(newInterval, 0.1f);
                EditorPrefs.SetFloat(KEY_INTERVAL, saveInterval);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Save Now")) SaveNow();
        }

        private enum SaveMode
        {
            KeepFew,
            KeepEverything,
            KeepOnlyTheLatest
        }
    }
}
