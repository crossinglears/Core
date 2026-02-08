using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public class ImportantPathsTab : CL_WindowTab
    {
        public override string TabName => "Paths";

        public List<Path> FilePaths = new()
        {
            new("Asset Folder", Application.dataPath),
            new("Persistent Data", Application.persistentDataPath),
            new("Streaming Assets", Application.streamingAssetsPath),
            new("Cache Path", Application.temporaryCachePath),
            new("Log Path", Application.consoleLogPath)
        };

        public List<Path> EditorPaths = new()
        {
            new("Editor", EditorApplication.applicationContentsPath),
        };

        public List<Path> TemplatesPath = new()
        {
            new("Script Templates", EditorApplication.applicationContentsPath + "/Resources/ScriptTemplates/"),
            new("Project Templates", EditorApplication.applicationPath.Replace("Unity.exe", "") + @"Data\Resources\PackageManager\ProjectTemplates\"),
        };

        public List<Path> BuildLogsPath = new()
        {
            new("Build Log (Windows)", System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData) + "/Unity/Editor/Editor.log"),
            new("Build Log (macOS)", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/Library/Logs/Unity/Editor.log"),
            new("Build Log (Linux)", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/.config/unity3d/Editor.log"),
        };


        public override void DrawContent()
        {
            DisplaySet(FilePaths);
            CL_GUILayout.HorizontalSeparator();

            DisplaySet(TemplatesPath);
            CL_GUILayout.HorizontalSeparator();

            DisplaySet(EditorPaths);
            CL_GUILayout.HorizontalSeparator();

            DisplaySet(BuildLogsPath);
        }

        private void DisplaySet(List<Path> paths)
        {
            foreach(Path item in paths)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label(item.Name, EditorStyles.boldLabel, GUILayout.Width(130));

                GUILayout.Label(item.DirectoryPath, GUILayout.MinWidth(20));
                if(GUILayout.Button("Copy", GUILayout.Width(50)))
                {
                    EditorGUIUtility.systemCopyBuffer = item.DirectoryPath;
                }
                if(GUILayout.Button("Open", GUILayout.Width(50)))
                {
                    item.OpenFileViewer();
                }
                GUILayout.EndHorizontal();
            }
        }
        
        public class Path
        {
            public string Name;
            public string DirectoryPath;

            public void OpenFileViewer()
            {
                string path = DirectoryPath;
                while (!System.IO.Directory.Exists(path) && !System.IO.File.Exists(path))
                {
                    path = System.IO.Path.GetDirectoryName(path);
                    if (string.IsNullOrEmpty(path))
                    {
                        Debug.LogError($"Cannot reveal path: {DirectoryPath}");
                        return;
                    }
                }

                EditorUtility.RevealInFinder(path);
            }

            public Path(string n, string d)
            {
                Name = n;
                DirectoryPath = d;
            }
        }
    }
}
