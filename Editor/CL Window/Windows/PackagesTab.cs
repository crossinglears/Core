using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace CrossingLearsEditor
{
    public class PackagesTab : CL_WindowTab
    {
        static List<PackageEntry> cL_Packages = new(){
            new ("Directory", "Find anything easier", "https://github.com/crossinglears/Directory.git"),
            new ("Toolbar", "Extra tools in the editor", "https://github.com/crossinglears/CustomToolbar.git"),
            new ("Latest Menu", "A simplified menu manager", "https://github.com/crossinglears/Latest-Menu.git"),
            new ("UI", "A collection of UI tools", "https://github.com/crossinglears/UI.git"),
            new ("Audio", "A simple Audio Manager", "https://github.com/crossinglears/Audio.git"),
            new ("Haptic Feedback", "Enable vibration on mobile", "https://github.com/crossinglears/HapticFeedback.git"),
        };
        
        public override string TabName => "Packages";

        private AddRequest addRequest;
        private string newPackageName = "";
        private static readonly string savePath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "CrossingLears",
            "FavoritePackages.json"
        );

        private List<PackageEntry> FavoriteTabs = new();

        public override void OnFocus()
        {
            base.OnFocus();
            LoadFavorites();
        }

        void CrossingLearsPackages()
        {
            // Full container box
            EditorGUILayout.BeginVertical("helpbox");
            {
                // Title bar rect
                Rect rect = GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true));

                // Draw colored bar
                EditorGUI.DrawRect(rect, CL_Design.brown);

                // Draw centered label
                GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
                EditorGUI.LabelField(rect, "Crossing Lears Packages", titleStyle);
            }

            {
                Rect rect = EditorGUILayout.BeginVertical();
                EditorGUI.DrawRect(rect, CL_Design.gold);
                
                GUILayout.Space(3);
            }

            // Content area
            GUIStyle centeredStyle = new GUIStyle(CL_Design.ColoredLabel(CL_Design.brown));
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            
            foreach (var package in cL_Packages)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.LabelField(package.Name, CL_Design.ColoredLabel(CL_Design.brown), GUILayout.Width(80));

                EditorGUILayout.LabelField(package.Desc, centeredStyle, GUILayout.MinWidth(1));


                if (GUILayout.Button("Install", GUILayout.Width(60)))
                {
                    InstallPackage(package.Url);
                }
                GUILayout.Space(3);
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(3);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        void FavouritePackages()
        {
            GUILayout.Label("Favorite Packages", EditorStyles.boldLabel);

            
            EditorGUILayout.BeginVertical("helpbox");
            if (FavoriteTabs.Count == 0)
            {
                EditorGUILayout.LabelField("None");
            }
            else
            {
                for (int i = 0; i < FavoriteTabs.Count; i++)
                {
                    var package = FavoriteTabs[i];
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(package.Name, GUILayout.Width(80), GUILayout.MinWidth(0));
                    package.Url = EditorGUILayout.TextField(package.Url, GUILayout.ExpandWidth(true), GUILayout.MinWidth(10));

                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        FavoriteTabs.RemoveAt(i);
                        i--;
                    }
                    else if (GUILayout.Button("Install", GUILayout.Width(60)))
                    {
                        InstallPackage(package.Url);
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        public override void DrawContent()
        {
            EditorGUILayout.HelpBox("Enlist your frequently used packages to make it easier to install them on your future projects. The list is persistent and is stored in your computer.", MessageType.Info);

            // Save path display
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.TextField("Save Path", savePath, EditorStyles.textField, GUILayout.ExpandWidth(true));
            GUI.enabled = true;
            if (GUILayout.Button("Copy", GUILayout.Width(60)))
            {
                EditorGUIUtility.systemCopyBuffer = savePath;
                Debug.Log("Path copied: " + savePath);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            FavouritePackages();

            GUILayout.Space(3);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(45));
            newPackageName = EditorGUILayout.TextField(newPackageName, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Add Entry", GUILayout.Width(100)) && !string.IsNullOrWhiteSpace(newPackageName))
            {
                FavoriteTabs.Add(new PackageEntry(newPackageName, ""));
                newPackageName = "";
                SaveFavorites();
            }
            EditorGUILayout.EndHorizontal();

            // Reset & Save buttons
            EditorGUILayout.BeginHorizontal();
            float buttonWidth = (EditorGUIUtility.currentViewWidth - 130) / 2;

            if (GUILayout.Button("Reset List", GUILayout.Width(buttonWidth - 5)))
            {
                if (EditorUtility.DisplayDialog("Reset Favorites",
                    "Are you sure you want to reset the package list?", "Yes", "No"))
                {
                    FavoriteTabs = new List<PackageEntry>
                    {
                        new PackageEntry("Core", "https://github.com/crossinglears/Core.git#main")
                    };
                    SaveFavorites();
                }
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Save List", GUILayout.Width(buttonWidth - 5)))
            {
                SaveFavorites();
            }

            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            CrossingLearsPackages();
        }

        public void InstallPackage(string githubUrl)
        {
            addRequest = Client.Add(githubUrl);
            EditorApplication.update += PackageProgress;
        }

        private void PackageProgress()
        {
            if (addRequest.IsCompleted)
            {
                if (addRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Package added successfully!");
                }
                else
                {
                    Debug.LogError("Failed to install package: " + addRequest.Error.message);
                }
                EditorApplication.update -= PackageProgress;
            }
        }

        private void SaveFavorites()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            File.WriteAllText(savePath, JsonUtility.ToJson(new PackageListWrapper(FavoriteTabs)));
        }

        private void LoadFavorites()
        {
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                FavoriteTabs = JsonUtility.FromJson<PackageListWrapper>(json)?.Packages 
                               ?? new List<PackageEntry> { new PackageEntry("Core", "https://github.com/crossinglears/Core.git#main") };
            }
            else
            {
                FavoriteTabs = new List<PackageEntry>
                {
                    new PackageEntry("Core", "https://github.com/crossinglears/Core.git#main")
                };
            }
            SaveFavorites(); // Ensure the loaded favorites are saved
        }

        [System.Serializable]
        private class PackageListWrapper
        {
            public List<PackageEntry> Packages = new();

            public PackageListWrapper(List<PackageEntry> packages)
            {
                Packages = packages;
            }
        }

        [System.Serializable]
        private class PackageEntry
        {
            public string Name;
            public string Url;
            public string Desc;

            public PackageEntry(string name, string url)
            {
                Name = name;
                Url = url;
            }

            public PackageEntry(string name, string desc, string url)
            {
                Desc = desc;
                Name = name;
                Url = url;
            }
        }
    }
}
