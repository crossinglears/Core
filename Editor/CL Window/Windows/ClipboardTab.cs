using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace CrossingLearsEditor
{
    public class ClipboardTab : CL_WindowTab
    {
        public override string TabName => "Clipboard";

        public override void DrawTitle()
        {
            GUILayout.BeginHorizontal();
            base.DrawTitle();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add New List"))
            {
                clipboardLists.Add(new ClipboardList { ListName = "New List" });
                SaveTasks();
                DrawReorderables();
            }
            if (GUILayout.Button("Check Dictionary"))
            {
                LogAllClipboardItems();
                void LogAllClipboardItems()
                {
                    foreach (KeyValuePair<string, List<Content>> pair in keyValuePairs)
                    {
                        Debug.Log("InvokeID: " + pair.Key);

                        for (int i = 0; i < pair.Value.Count; i++)
                        {
                            ClipboardTab.Content content = pair.Value[i];
                            Debug.Log("    Title: " + content.Title + " | Message: " + content.Message);
                        }
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        [System.Serializable]
        public class Content
        {
            public string Title;
            public string _Message;
            public string InvokeID = "";

            public string Message
            {
                get { return string.IsNullOrEmpty(_Message) ? Title : _Message; }
                set { _Message = value; }
            }

            public ClipboardPurpose Purpose;

            public void Press(ClipboardTab clipboardTab, bool recursion)
            {
                if(recursion && !string.IsNullOrEmpty(InvokeID))
                {
                    foreach(Content item in clipboardTab.keyValuePairs[InvokeID])
                    {
                        item.Press(clipboardTab, false);
                    }
                    return;
                }

                switch (Purpose)
                {
                    case ClipboardPurpose.Clipboard:
                        EditorGUIUtility.systemCopyBuffer = Message;
                        break;

                    case ClipboardPurpose.SnapInterval:
                        if (ExtensionTools.TryParseVector3(Message, out Vector3 snap))
                            EditorSnapSettings.move = snap;
                        break;

                    case ClipboardPurpose.ResizeObject:
                        if (Selection.activeTransform != null && ExtensionTools.TryParseVector3(Message, out Vector3 scale))
                        {
                            Undo.RecordObject(Selection.activeTransform, "Resize Object");
                            Selection.activeTransform.localScale = scale;
                        }
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
            Log,
        }

        private List<ClipboardList> clipboardLists = new List<ClipboardList>();
        private List<ReorderableList> reorderableLists = new List<ReorderableList>();
        public Dictionary<string, List<Content>> keyValuePairs = new();

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

                var reorderable = new ReorderableList(list.Contents, typeof(Content), true, true, false, false);

                reorderable.drawHeaderCallback = (Rect rect) =>
                {
                    float width = rect.width;
                    float buttonWidth = 80;
                    float menuWidth = 20;

                    // Text field for list name
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, width - buttonWidth - menuWidth - 10, EditorGUIUtility.singleLineHeight), list.ListName);

                    // Add item button
                    if (GUI.Button(new Rect(rect.x + width - buttonWidth - menuWidth - 5, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight), "Add Item"))
                    {
                        var c = new Content { Title = "New Content", Message = "", Purpose = ClipboardPurpose.Clipboard };
                        list.Contents.Add(c);
                        ClipboardEditWindow.Open(c, SaveTasks, this);
                    }

                    // 3-dot menu button
                    if (GUI.Button(new Rect(rect.x + width - menuWidth, rect.y, menuWidth, EditorGUIUtility.singleLineHeight), "⋮"))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Rename"), false, () =>
                        {                            
                            StringInputWindow.Open("Rename List", "Enter a new name for this list:", list.ListName, newName =>
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
                            clipboardLists.Clear();
                            SaveTasks();
                            DrawReorderables();
                        });
                        menu.ShowAsContext();
                    }
                };

                reorderable.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    rect.y += 2;
                    Content content = list.Contents[index];
                    float width = rect.width;

                    EditorGUI.LabelField(new Rect(rect.x + 5, rect.y, width - 200, EditorGUIUtility.singleLineHeight), content.Title);

                    if (GUI.Button(new Rect(rect.x + width - 180, rect.y, 60, EditorGUIUtility.singleLineHeight), "Edit"))
                        ClipboardEditWindow.Open(content, SaveTasks, this);

                    if (GUI.Button(new Rect(rect.x + width - 115, rect.y, 90, EditorGUIUtility.singleLineHeight), content.Purpose.ToString()))
                    {
                        content.Press(this, true);
                        SaveTasks();
                    }

                    if (GUI.Button(new Rect(rect.x + width - 25, rect.y, 25, EditorGUIUtility.singleLineHeight), "X"))
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

            foreach (var reorderable in reorderableLists)
                reorderable.DoLayoutList();

            EditorGUILayout.EndVertical();
        }

        private void SaveTasks()
        {
            string directory = System.IO.Path.GetDirectoryName(TaskFilePath);
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            string json = JsonUtility.ToJson(new ClipboardWrapper { lists = clipboardLists }, true);
            System.IO.File.WriteAllText(TaskFilePath, json);
            AssetDatabase.Refresh();
        }

        // private void LoadClipboard()
        // {
        //     if (System.IO.File.Exists(TaskFilePath))
        //     {
        //         string json = System.IO.File.ReadAllText(TaskFilePath);
        //         ClipboardWrapper wrapper = JsonUtility.FromJson<ClipboardWrapper>(json);
        //         clipboardLists = wrapper?.lists ?? new List<ClipboardList>();
        //     }
        //     else
        //     {
        //         clipboardLists = new List<ClipboardList>();
        //     }

        //     foreach(var item in clipboardLists)
        //     {
        //         if(item.in)
        //     }
        // }

        private void LoadClipboard()
        {
            if (System.IO.File.Exists(TaskFilePath))
            {
                string json = System.IO.File.ReadAllText(TaskFilePath);
                ClipboardWrapper wrapper = JsonUtility.FromJson<ClipboardWrapper>(json);
                clipboardLists = wrapper != null ? wrapper.lists : new List<ClipboardList>();
            }
            else
            {
                clipboardLists = new List<ClipboardList>();
            }

            keyValuePairs.Clear();

            for (int i = 0; i < clipboardLists.Count; i++)
            {
                ClipboardList list = clipboardLists[i];

                for (int j = 0; j < list.Contents.Count; j++)
                {
                    Content content = list.Contents[j];

                    if (string.IsNullOrEmpty(content.InvokeID))
                    {
                        continue;
                    }

                    if (!keyValuePairs.ContainsKey(content.InvokeID))
                    {
                        keyValuePairs.Add(content.InvokeID, new List<Content>());
                    }

                    keyValuePairs[content.InvokeID].Add(content);
                }
            }
        }


        [System.Serializable]
        private class ClipboardWrapper
        {
            public List<ClipboardList> lists;
        }
    }

    public class ClipboardEditWindow : EditorWindow
    {
        private static ClipboardTab.Content editingContent;
        private static System.Action saveCallback;
        private static  ClipboardTab clipboardTab;

        static string originalInvokeID = "";

        public static void Open(ClipboardTab.Content content, System.Action onSave, ClipboardTab clipboardTab)
        {
            ClipboardEditWindow.clipboardTab = clipboardTab;
            ClipboardEditWindow window = GetWindow<ClipboardEditWindow>("Edit Clipboard");
            editingContent = content;
            saveCallback = onSave;
            originalInvokeID = content.InvokeID;
            window.Show();
        }

        private bool focusTitle = true;

        // private void OnGUI()
        // {
        //     if (editingContent == null) return;

        //     GUILayout.Label("Edit Clipboard Content", EditorStyles.boldLabel);

        //     // Focus the Title field on first draw
        //     GUI.SetNextControlName("TitleField");
        //     editingContent.Title = EditorGUILayout.TextField("Title", editingContent.Title);

        //     if (focusTitle)
        //     {
        //         EditorGUI.FocusTextInControl("TitleField");
        //         focusTitle = false;
        //     }

        //     editingContent.Message = EditorGUILayout.TextField("Message", editingContent._Message);
        //     editingContent.Purpose = (ClipboardTab.ClipboardPurpose)EditorGUILayout.EnumPopup("Purpose", editingContent.Purpose);
        //     editingContent.InvokeID = EditorGUILayout.TextField("Invoke ID", editingContent.InvokeID);

        //     GUILayout.Space(10);

        //     if (GUILayout.Button("Save"))
        //     {

        //         if(!string.IsNullOrEmpty(originalInvokeID) && originalInvokeID != editingContent.InvokeID)
        //         {
        //             List<ClipboardTab.Content> l = clipboardTab.keyValuePairs[originalInvokeID];
        //             l.Remove(editingContent);
        //             if(l.Count == 0)
        //             {
        //                 clipboardTab.keyValuePairs.Remove(originalInvokeID);
        //             }
        //         }

        //         if(!string.IsNullOrEmpty(editingContent.InvokeID) && originalInvokeID != editingContent.InvokeID)
        //         {
                    
        //         }

        //         saveCallback?.Invoke();
        //         Close();
        //     }
        // }

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
            editingContent.InvokeID = EditorGUILayout.TextField("Invoke ID", editingContent.InvokeID);

            GUILayout.Space(10);

            if (GUILayout.Button("Save"))
            {
                bool hadOriginal = !string.IsNullOrEmpty(originalInvokeID);
                bool changed = editingContent.InvokeID != originalInvokeID;
                bool hasNew = !string.IsNullOrEmpty(editingContent.InvokeID);

                if (hadOriginal && changed)
                {
                    List<ClipboardTab.Content> list = clipboardTab.keyValuePairs[originalInvokeID];
                    list.Remove(editingContent);
                    if (list.Count == 0)
                    {
                        clipboardTab.keyValuePairs.Remove(originalInvokeID);
                    }
                }

                if (changed && hasNew)
                {
                    if (!clipboardTab.keyValuePairs.ContainsKey(editingContent.InvokeID))
                    {
                        clipboardTab.keyValuePairs.Add(editingContent.InvokeID, new List<ClipboardTab.Content>());
                    }

                    clipboardTab.keyValuePairs[editingContent.InvokeID].Add(editingContent);
                }

                saveCallback?.Invoke();
                Close();
            }
        }

    }
}
