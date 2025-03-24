using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace CrossingLears
{
    public class PackagesTab : CL_WindowTab
    {
        public override string TabName => "Packages";

        private AddRequest addRequest;
        private string newPackageName = "";
        private static readonly string savePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "CrossingLears", "FavoritePackages.json");

        private List<(string Name, string Url)> FavoriteTabs = new List<(string, string)>();

        public override void OnFocus()
        {
            base.OnFocus();
            LoadFavorites();
        }
        
        public override void DrawContent()
        {
            EditorGUILayout.HelpBox("Enlist your frequently used packages to make it easier to install it on your future projects. The list is persistent and is stored in your computer", MessageType.Info);
            EditorGUILayout.Space(5);
            EditorGUIUtility.labelWidth = 80;

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

            EditorGUIUtility.labelWidth = 0;

            EditorGUILayout.Space(10);
            GUILayout.Label("Favorite Packages", EditorStyles.boldLabel);
            for (int i = 0; i < FavoriteTabs.Count; i++)
            {
                var package = FavoriteTabs[i];
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(package.Name, GUILayout.Width(80));
                FavoriteTabs[i] = (package.Name, EditorGUILayout.TextField(package.Url, GUILayout.ExpandWidth(true)));
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    FavoriteTabs.RemoveAt(i);
                    i--;
                }
                if (GUILayout.Button("Install", GUILayout.Width(60)))
                {
                    InstallPackage(FavoriteTabs[i].Url);
                }

                EditorGUILayout.EndHorizontal();
            }
            
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            newPackageName = EditorGUILayout.TextField(newPackageName, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Add Entry", GUILayout.Width(100)) && !string.IsNullOrWhiteSpace(newPackageName))
            {
                FavoriteTabs.Add((newPackageName, ""));
                newPackageName = "";
                SaveFavorites();
            }
            EditorGUILayout.EndHorizontal();
            
            // GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            float buttonWidth = (EditorGUIUtility.currentViewWidth - 20 - 100) / 2; // Half width with spacing
            if (GUILayout.Button("Reset List", GUILayout.Width(buttonWidth - 5)))
            {
                if (EditorUtility.DisplayDialog("Reset Favorites", 
                    "Are you sure you want to reset the package list?", "Yes", "No"))
                {
                    FavoriteTabs = new List<(string, string)>
                    {
                        ("Core", "https://github.com/crossinglears/Core.git#main")
                    };
                    SaveFavorites();
                }
            }

            GUILayout.Space(10); // Space between buttons

            if (GUILayout.Button("Save List", GUILayout.Width(buttonWidth - 5)))
            {
                SaveFavorites();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void InstallPackage(string githubUrl)
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
                FavoriteTabs = JsonUtility.FromJson<PackageListWrapper>(json)?.ToList() ?? new List<(string, string)>
                {
                    ("Core", "https://github.com/crossinglears/Core.git#main")
                };
            }
            else
            {
                FavoriteTabs = new List<(string, string)>
                {
                    ("Core", "https://github.com/crossinglears/Core.git#main")
                };
            }
            SaveFavorites(); // Ensure the loaded favorites are saved
        }

        [System.Serializable]
        private class PackageListWrapper
        {
            public List<PackageEntry> Packages = new();

            public PackageListWrapper(List<(string, string)> packages)
            {
                Packages = packages.Select(p => new PackageEntry { Name = p.Item1, Url = p.Item2 }).ToList();
            }

            public List<(string, string)> ToList()
            {
                return Packages.Select(p => (p.Name, p.Url)).ToList();
            }
        }

        [System.Serializable]
        private class PackageEntry
        {
            public string Name;
            public string Url;
        }
    }
}