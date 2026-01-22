using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace CrossingLears.Editor
{
    public class ClipboardTab : CL_WindowTab
    {
        public override string TabName => "Clipboard";

        public override void DrawTitle()
        {
            GUILayout.BeginHorizontal();
            base.DrawTitle();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Arrange"))
                ClipboardArrangeWindow.Open(this);

            if (GUILayout.Button("Add New List"))
            {
                clipboardLists.Add(new ClipboardList { ListName = "New List" });
                SaveTasks();
                DrawReorderables();
            }

            GUILayout.EndHorizontal();
        }

        [System.Serializable]
        public class Content
        {
            public string Title;
            public string _Message;
            public string Message
            {
                get { return string.IsNullOrEmpty(_Message) ? Title : _Message; }
                set { _Message = value; }
            }
            public ClipboardPurpose Purpose;
            public string SnapInterval = "";
            public string PrefabReplace = "";

            public void Press()
            {
                Press(Purpose);
            }

            public void Press(ClipboardPurpose x)
            {
                switch (x)
                {
                    case ClipboardPurpose.Clipboard:
                        EditorGUIUtility.systemCopyBuffer = Message;
                        if (ExtensionTools.TryParseVector3(SnapInterval, out Vector3 s))
                            EditorSnapSettings.move = s;
                        break;

                    case ClipboardPurpose.SnapInterval:
                        if (ExtensionTools.TryParseVector3(Message, out Vector3 snap))
                            EditorSnapSettings.move = snap;
                        else if (ExtensionTools.TryParseVector3(SnapInterval, out Vector3 s2))
                            EditorSnapSettings.move = s2;
                        break;

                    case ClipboardPurpose.ResizeObject:
                        if (Selection.activeTransform != null && ExtensionTools.TryParseVector3(Message, out Vector3 scale))
                        {
                            Undo.RecordObject(Selection.activeTransform, "Resize Object");
                            Selection.activeTransform.localScale = scale;
                        }
                        break;

                    case ClipboardPurpose.PrefabReplace:
                        if (PrefabReplace == "") break;

                        string prefabPath = PrefabReplace;
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                        if (prefab == null) break;

                        Transform[] targets = Selection.transforms;
                        if (targets == null || targets.Length == 0) break;

                        List<GameObject> newSelection = new List<GameObject>();

                        foreach (Transform target in targets)
                        {
                            Transform parent = target.parent;

                            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                            if (instance == null) continue;

                            Undo.RegisterCreatedObjectUndo(instance, "Prefab Replace");

                            instance.transform.localPosition = target.localPosition;
                            instance.transform.localRotation = target.localRotation;
                            instance.transform.localScale = target.localScale;
                            instance.transform.SetSiblingIndex(target.GetSiblingIndex());

                            Undo.DestroyObjectImmediate(target.gameObject);
                            newSelection.Add(instance);
                        }

                        Selection.objects = newSelection.ToArray();
                        break;

                    default:
                        Debug.Log($"{Message}");
                        break;
                }
            }
        }

        [System.Serializable]
        public class ClipboardList
        {
            public string ListName = "New List";
            public List<Content> Contents = new List<Content>();
        }

        public enum ClipboardPurpose
        {
            Clipboard,
            ResizeObject,
            SnapInterval,
            PrefabReplace,
            Log,
        }

        internal List<ClipboardList> clipboardLists = new List<ClipboardList>();
        private List<ReorderableList> reorderableLists = new List<ReorderableList>();

        private const string TaskFilePath = "Assets/Editor/Development Files/Clipboard.json";

        public override void OnEnable()
        {
            LoadClipboard();
            DrawReorderables();
        }

        public override void OnFocus()
        {
            DrawReorderables();
        }

        private void DrawReorderables()
        {
            reorderableLists.Clear();

            for (int i = 0; i < clipboardLists.Count; i++)
            {
                int listIndex = i;
                ClipboardList list = clipboardLists[listIndex];

                ReorderableList reorderable = new ReorderableList(list.Contents, typeof(Content), true, true, false, false);

                reorderable.drawHeaderCallback = (Rect rect) =>
{
    float width = rect.width;
    float buttonWidth = 80;
    float menuWidth = 20;

    EditorGUI.LabelField(
        new Rect(rect.x, rect.y, width - buttonWidth - menuWidth - 10, EditorGUIUtility.singleLineHeight),
        list.ListName
    );

    if (GUI.Button(
        new Rect(rect.x + width - buttonWidth - menuWidth - 5, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight),
        "Add Item"))
    {
        ClipboardTab.Content c = new ClipboardTab.Content { Title = "New Content" };
        list.Contents.Add(c);
        ClipboardEditWindow.Open(c, SaveTasks);
    }

    if (GUI.Button(
        new Rect(rect.x + width - menuWidth, rect.y, menuWidth, EditorGUIUtility.singleLineHeight),
        "⋮"))
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Rename"), false, () =>
        {
            StringInputWindow.Open("Rename List", "Enter new name:", list.ListName, newName =>
            {
                if (!string.IsNullOrEmpty(newName))
                {
                    list.ListName = newName;
                    SaveTasks();
                    DrawReorderables();
                }
            });
        });

        menu.AddItem(new GUIContent("Delete"), false, () =>
        {
            clipboardLists.RemoveAt(listIndex);
            SaveTasks();
            DrawReorderables();
        });

        menu.AddItem(new GUIContent("Clear"), false, () =>
        {
            list.Contents.Clear();
            SaveTasks();
        });

        menu.ShowAsContext();
    }
};


                reorderable.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
{
    rect.y += 2;

    Content content = list.Contents[index];
    float lineHeight = EditorGUIUtility.singleLineHeight;

    float x = rect.x + 5f;
    float y = rect.y;
    float w = rect.width;

    float editWidth = 60f;
    float purposeWidth = 90f;
    float addWidth = 25f;
    float removeWidth = 25f;
    float spacing = 5f;

    float removeX = rect.x + w - removeWidth;
    float addX = removeX - spacing - addWidth;
    float purposeX = addX - spacing - purposeWidth;
    float editX = purposeX - spacing - editWidth;

    float labelWidth = editX - x - spacing;

    EditorGUI.LabelField(
        new Rect(x, y, labelWidth, lineHeight),
        content.Title
    );

    if (GUI.Button(new Rect(editX, y, editWidth, lineHeight), "Edit"))
        ClipboardEditWindow.Open(content, SaveTasks);

    if (GUI.Button(new Rect(purposeX, y, purposeWidth, lineHeight), content.Purpose.ToString()))
    {
        content.Press();
        SaveTasks();
    }

    if (GUI.Button(new Rect(addX, y, addWidth, lineHeight), "+"))
    {
        GenericMenu menu = new GenericMenu();

        foreach (ClipboardPurpose value in System.Enum.GetValues(typeof(ClipboardPurpose)))
        {
            ClipboardPurpose captured = value;
            menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(captured.ToString())), false, () =>
            {
                content.Press(captured);
                SaveTasks();
            });
        }

        menu.ShowAsContext();
    }

    if (GUI.Button(new Rect(removeX, y, removeWidth, lineHeight), "X"))
    {
        list.Contents.RemoveAt(index);
        SaveTasks();
        GUIUtility.ExitGUI();
    }
};


                reorderableLists.Add(reorderable);
            }
        }

        public override void DrawContent()
        {
            EditorGUILayout.BeginVertical();
            foreach (ReorderableList r in reorderableLists)
                r.DoLayoutList();
            EditorGUILayout.EndVertical();
        }

        internal void SaveTasks()
        {
            string directory = System.IO.Path.GetDirectoryName(TaskFilePath);
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            string json = JsonUtility.ToJson(new ClipboardWrapper { lists = clipboardLists }, true);
            System.IO.File.WriteAllText(TaskFilePath, json);
            AssetDatabase.Refresh();
        }

        private void LoadClipboard()
        {
            if (System.IO.File.Exists(TaskFilePath))
            {
                string json = System.IO.File.ReadAllText(TaskFilePath);
                ClipboardWrapper wrapper = JsonUtility.FromJson<ClipboardWrapper>(json);
                clipboardLists = wrapper != null ? wrapper.lists : new List<ClipboardList>();
            }
            else clipboardLists = new List<ClipboardList>();
        }

        [System.Serializable]
        private class ClipboardWrapper
        {
            public List<ClipboardList> lists;
        }
    }

    public class ClipboardArrangeWindow : EditorWindow
    {
        private ClipboardTab tab;
        private ReorderableList reorderable;

        public static void Open(ClipboardTab clipboardTab)
        {
            ClipboardArrangeWindow window = GetWindow<ClipboardArrangeWindow>("Arrange Lists");
            window.tab = clipboardTab;
            window.Init();
            window.Show();
        }

        private void Init()
        {
            reorderable = new ReorderableList(tab.clipboardLists, typeof(ClipboardTab.ClipboardList), true, true, false, false);
            reorderable.drawHeaderCallback = rect =>
                EditorGUI.LabelField(rect, "Clipboard Lists");

            reorderable.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2;
                EditorGUI.LabelField(rect, tab.clipboardLists[index].ListName);
            };
        }

        private void OnGUI()
        {
            if (reorderable != null)
                reorderable.DoLayoutList();

            GUILayout.Space(10);

            if (GUILayout.Button("Save"))
            {
                tab.SaveTasks();
                tab.OnFocus();
                Close();
            }
        }
    }

    public class ClipboardEditWindow : EditorWindow
    {
        private static ClipboardTab.Content editingContent;
        private static System.Action saveCallback;
        private bool focusTitle = true;

        public static void Open(ClipboardTab.Content content, System.Action onSave)
        {
            ClipboardEditWindow window = GetWindow<ClipboardEditWindow>("Edit Clipboard");
            editingContent = content;
            saveCallback = onSave;
            window.Show();
        }

        private void OnGUI()
        {
            if (editingContent == null) return;

            GUILayout.Label("Edit Clipboard Content", EditorStyles.boldLabel);

            GUI.SetNextControlName("TitleField");
            editingContent.Title = EditorGUILayout.TextField("Title", editingContent.Title);

            if (focusTitle)
            {
                EditorGUI.FocusTextInControl("TitleField");
                focusTitle = false;
            }

            editingContent.Message = EditorGUILayout.TextField("Message", editingContent._Message);
            editingContent.Purpose = (ClipboardTab.ClipboardPurpose)EditorGUILayout.EnumPopup("Purpose", editingContent.Purpose);
            editingContent.SnapInterval = EditorGUILayout.TextField("Snap Interval", editingContent.SnapInterval);

            GUILayout.BeginHorizontal();
            editingContent.PrefabReplace = EditorGUILayout.TextField("Prefab Replace", editingContent.PrefabReplace);
            if(GUILayout.Button("Grab", GUILayout.Width(50)))
            {
                editingContent.PrefabReplace = ExtensionTools.GetPathOfObjectSelected(Selection.activeObject);
                if(string.IsNullOrEmpty(editingContent.Title) || editingContent.Title == "New Content")
                {
                    editingContent.Title = Selection.activeObject.name;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button("Save"))
            {
                saveCallback?.Invoke();
                Close();
            }
        }
    }
}