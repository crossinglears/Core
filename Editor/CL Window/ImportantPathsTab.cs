using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public class ImportantPathsTab : CL_WindowTab
    {
        public override string TabName => "Paths";
        public override int Order => 100;

        public List<Path> FilePaths = new()
        {
            new("Persistent Data", Application.persistentDataPath),
            new("Data Path", Application.dataPath),
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
            new("Script Template", EditorApplication.applicationContentsPath + "/Resources/ScriptTemplates/"),
        };


        public override void DrawContent()
        {
            DisplaySet(FilePaths);

            GUILayout.Space(10);

            DisplaySet(TemplatesPath);
            GUILayout.Space(10);

            DisplaySet(EditorPaths);
        }

        private void DisplaySet(List<Path> paths)
        {
            foreach(var item in paths)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(20);
                GUILayout.Label(item.Name, EditorStyles.boldLabel, GUILayout.Width(100));

                GUILayout.Label(item.DirectoryPath, GUILayout.MinWidth(20));
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
