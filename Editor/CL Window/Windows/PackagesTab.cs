using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditorInternal;
using System.Security.Policy;

namespace CrossingLearsEditor
{
    public class PackagesTab : CL_WindowTab
    {
        static List<PackageEntry> cL_Packages = new()
        {
            new PackageEntry("Directory", "Find anything easier", "https://github.com/crossinglears/Directory.git"),
            new PackageEntry("Toolbar", "Extra tools in the editor", "https://github.com/crossinglears/CustomToolbar.git"),
            new PackageEntry("Latest Menu", "A simplified menu manager", "https://github.com/crossinglears/Latest-Menu.git"),
            new PackageEntry("UI", "A collection of UI tools", "https://github.com/crossinglears/UI.git"),
            new PackageEntry("Audio", "A simple Audio Manager", "https://github.com/crossinglears/Audio.git"),
            new PackageEntry("Haptic Feedback", "Enable vibration on mobile", "https://github.com/crossinglears/HapticFeedback.git"),
        };

        static List<PackageEntry> cL_PaidPackages = new()
        {
            new PackageEntry("Editor Calendar", "Serializable DateTime", "https://crossinglears.itch.io/serializeable-datetime"),
        };

        public override string TabName => "Packages";

        private AddRequest addRequest;

        private static readonly string savePath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "CrossingLears",
            "FavoritePackages.json"
        );

        private List<PackageEntry> FavoriteTabs = new();
        private ReorderableList favoriteList;

        public override void Awake()
        {
            base.Awake();
            OnFocus();
        }

        public override void OnFocus()
        {
            base.OnFocus();
            LoadFavorites();
            BuildReorderableList();
        }

        private void BuildReorderableList()
        {
            favoriteList = new ReorderableList(FavoriteTabs, typeof(PackageEntry), true, false, false, false);

            favoriteList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                PackageEntry entry = FavoriteTabs[index];

                float x = rect.x;
                float y = rect.y + 2;
                float h = EditorGUIUtility.singleLineHeight;

                Rect titleRect = new Rect(x, y, 120, h);
                Rect descRect = new Rect(x + 125, y, rect.width - 125 - 140, h);
                Rect editRect = new Rect(rect.xMax - 135, y, 65, h);
                Rect installRect = new Rect(rect.xMax - 65, y, 60, h);

                EditorGUI.LabelField(titleRect, entry.Name);
                EditorGUI.LabelField(descRect, entry.Desc);

                // if (GUI.Button(editRect, "Edit"))
                // {
                //     PackageEditWindow.Open(entry, SaveFavorites);
                // }

                if (GUI.Button(editRect, "Edit"))
                {
                    PackageEditWindow.Open(entry, SaveFavorites, () =>
                    {
                        FavoriteTabs.Remove(entry);
                        SaveFavorites();
                        BuildReorderableList();
                    });
                }


                if (GUI.Button(installRect, "Install"))
                {
                    InstallPackage(entry.Url);
                }
            };
        }

        public override void DrawContent()
        {
            EditorGUILayout.HelpBox(
                "Enlist your frequently used packages to make it easier to install them on your future projects. The list is persistent and is stored in your computer.",
                MessageType.Info
            );

            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.TextField("Save Path", savePath, EditorStyles.textField, GUILayout.ExpandWidth(true));
            GUI.enabled = true;
            if (GUILayout.Button("Copy", GUILayout.Width(60)))
            {
                EditorGUIUtility.systemCopyBuffer = savePath;
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Favorite Packages", EditorStyles.boldLabel);
            if (GUILayout.Button("Reset", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog(
                    "Reset Favorites",
                    "Are you sure you want to reset the package list?",
                    "Yes",
                    "No"))
                {
                    FavoriteTabs.Clear();
                    FavoriteTabs.Add(new PackageEntry(
                        "Toolbox",
                        "A set of tools to aid game development",
                        "https://github.com/crossinglears/Core.git#main"
                    ));
                    SaveFavorites();
                    BuildReorderableList();
                }
            }

            if (GUILayout.Button("Add", GUILayout.Width(60)))
            {
                FavoriteTabs.Add(new PackageEntry("", "", ""));
                SaveFavorites();
                BuildReorderableList();
            }

            GUILayout.EndHorizontal();

            if (favoriteList != null)
            {
                favoriteList.DoLayoutList();
            }

            CrossingLearsPackages();
        }

        void CrossingLearsPackages()
        {
            EditorGUILayout.BeginVertical("helpbox");
            {
                Rect rect = GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(rect, CL_Design.brown);

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

            GUIStyle centeredStyle = new GUIStyle(CL_Design.ColoredLabel(CL_Design.brown));
            centeredStyle.alignment = TextAnchor.MiddleCenter;

            foreach (PackageEntry package in cL_Packages)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.LabelField(package.Name, CL_Design.ColoredLabel(CL_Design.brown), GUILayout.Width(130));
                EditorGUILayout.LabelField(package.Desc, centeredStyle, GUILayout.MinWidth(1));

                if (GUILayout.Button("Install", GUILayout.Width(60)))
                {
                    InstallPackage(package.Url);
                }
                GUILayout.Space(3);
                EditorGUILayout.EndHorizontal();
            }
            foreach (PackageEntry package in cL_PaidPackages)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.LabelField(package.Name, CL_Design.ColoredLabel(CL_Design.brown), GUILayout.Width(130));
                EditorGUILayout.LabelField(package.Desc, centeredStyle, GUILayout.MinWidth(1));

                if (GUILayout.Button("Buy", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Open URL", $"Open \"{package.Url}\" with your browser?", "Yes", "No"))
                        Application.OpenURL(package.Url);
                }
                GUILayout.Space(3);
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(3);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        public void InstallPackage(string githubUrl)
        {
            addRequest = Client.Add(githubUrl);
            EditorApplication.update += PackageProgress;
        }

        private void PackageProgress()
        {
            if (!addRequest.IsCompleted) return;

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
                               ?? new List<PackageEntry>();
            }
            else
            {
                FavoriteTabs = new List<PackageEntry>();
            }

            SaveFavorites();
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
            public string Desc;
            public string Url;

            public PackageEntry(string name, string desc, string url)
            {
                Name = name;
                Desc = desc;
                Url = url;
            }
        }

        private class PackageEditWindow : EditorWindow
        {
            private static PackageEditWindow activeWindow;

            private PackageEntry entry;
            private System.Action onSave;
            private System.Action onDelete;
            private bool sizeLocked;

            public static void Open(PackageEntry entry, System.Action onSave, System.Action onDelete)
            {
                if (activeWindow != null)
                {
                    activeWindow.Close();
                }

                PackageEditWindow window = CreateInstance<PackageEditWindow>();
                activeWindow = window;

                window.entry = entry;
                window.onSave = onSave;
                window.onDelete = onDelete;
                window.titleContent = new GUIContent("Edit Package");

                window.ShowUtility();
            }

            private void OnGUI()
            {
                EditorGUILayout.LabelField("Title");
                entry.Name = EditorGUILayout.TextField(entry.Name);

                EditorGUILayout.LabelField("Description");
                entry.Desc = EditorGUILayout.TextField(entry.Desc);

                EditorGUILayout.LabelField("URL");
                entry.Url = EditorGUILayout.TextField(entry.Url);

                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Delete"))
                {
                    onDelete?.Invoke();
                    Close();
                }

                if (GUILayout.Button("Save"))
                {
                    onSave?.Invoke();
                    Close();
                }
                EditorGUILayout.EndHorizontal();

                if (!sizeLocked)
                {
                    float width = 300;
                    float height = 120 + 3 * EditorGUIUtility.singleLineHeight;
                    position = new Rect(position.position, new Vector2(width, height));
                    minSize = new Vector2(width, height);
                    maxSize = new Vector2(width, height);
                    sizeLocked = true;
                }
            }
        }

    }
}
