using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrossingLears.Editor
{
    public class SelectionGroup : CL_WindowTab
    {
        public override string TabName => "Selection Group";

        private const int SlotCount = 10;
        private const string SaveFilePath = "Assets/Editor/Development Files/SelectionGroups.json";

        private readonly List<Object> slotObjects = new List<Object>();
        private readonly List<SelectionGroupSlot> slots = new List<SelectionGroupSlot>();
        private ReorderableList selectedObjectsList;
        private int selectedSlotIndex;

        public override void OnEnable()
        {
            LoadGroups();
            RefreshSlotObjects();
            BuildSelectedObjectsList();
            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        public override void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            SaveGroups();
        }

        public override void DrawContent()
        {
            DrawSlotButtons();
            GUILayout.Space(8);
            DrawLoadSaveButtons();
            GUILayout.Space(8);
            selectedObjectsList.DoLayoutList();
        }

        private void DrawSlotButtons()
        {
            EditorGUILayout.BeginHorizontal();

            for (int i = 0; i < SlotCount; i++)
            {
                Color previousBackgroundColor = GUI.backgroundColor;
                bool hasObjects = SlotHasObjects(i);

                if (!hasObjects)
                {
                    GUI.backgroundColor = Color.gray;
                }
                else if (selectedSlotIndex == i)
                {
                    GUI.backgroundColor = new Color(0.55f, 0.75f, 1f);
                }

                if (GUILayout.Button((i + 1).ToString(), GUILayout.Height(28f)))
                {
                    selectedSlotIndex = i;
                    RefreshSlotObjects();
                    SaveGroups();
                }

                GUI.backgroundColor = previousBackgroundColor;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawLoadSaveButtons()
        {
            GUI.enabled = Selection.objects.Length > 0;
            if (GUILayout.Button("Add Current Selected Objects"))
            {
                AddCurrentSelectedObjects();
            }

            GUI.enabled = SlotHasObjects(selectedSlotIndex);
            if (GUILayout.Button("Load"))
            {
                LoadSelectedSlot();
            }
            GUI.enabled = true;
        }

        private void BuildSelectedObjectsList()
        {
            selectedObjectsList = new ReorderableList(slotObjects, typeof(Object), true, true, false, false);
            selectedObjectsList.drawHeaderCallback = DrawSelectedObjectsHeader;
            selectedObjectsList.drawElementCallback = DrawSelectedObjectElement;
            selectedObjectsList.onReorderCallback = OnSelectedObjectsReordered;
            selectedObjectsList.elementHeight = EditorGUIUtility.singleLineHeight + 4f;
        }

        private void DrawSelectedObjectsHeader(Rect rect)
        {
            Rect labelRect = new Rect(rect.x, rect.y, rect.width - 55f, rect.height);
            Rect clearRect = new Rect(rect.xMax - 50f, rect.y, 50f, rect.height);

            EditorGUI.LabelField(labelRect, "Group " + (selectedSlotIndex + 1) + " (" + slotObjects.Count + ")");

            if (GUI.Button(clearRect, "Clear"))
            {
                ClearSelectedSlot();
            }

            HandleHeaderDrag(labelRect);
        }

        private void DrawSelectedObjectElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.ObjectField(rect, slotObjects[index], typeof(Object), true);
            EditorGUI.EndDisabledGroup();
        }

        private void OnSelectedObjectsReordered(ReorderableList list)
        {
            SelectionGroupSlot slot = slots[selectedSlotIndex];
            ClearSlot(slot);

            for (int i = 0; i < slotObjects.Count; i++)
            {
                AddObjectToSlot(slotObjects[i], slot);
            }

            SaveGroups();
        }

        private void OnSelectionChanged()
        {
            CL_Window.current.Repaint();
        }

        private void AddCurrentSelectedObjects()
        {
            SelectionGroupSlot slot = slots[selectedSlotIndex];

            Object[] unitySelection = Selection.objects;
            for (int i = 0; i < unitySelection.Length; i++)
            {
                AddObjectToSlot(unitySelection[i], slot);
            }

            RefreshSlotObjects();
            SaveGroups();
        }

        private void LoadSelectedSlot()
        {
            Selection.objects = slotObjects.ToArray();
        }

        private void RefreshSlotObjects()
        {
            slotObjects.Clear();

            List<Object> currentSlotObjects = GetSlotObjects(selectedSlotIndex);
            for (int i = 0; i < currentSlotObjects.Count; i++)
            {
                slotObjects.Add(currentSlotObjects[i]);
            }
        }

        private void HandleHeaderDrag(Rect rect)
        {
            Event currentEvent = Event.current;

            if (!rect.Contains(currentEvent.mousePosition))
            {
                return;
            }

            if (currentEvent.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                currentEvent.Use();
            }

            if (currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                AddDraggedObjects();
                currentEvent.Use();
            }
        }

        private void AddDraggedObjects()
        {
            SelectionGroupSlot slot = slots[selectedSlotIndex];

            for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
            {
                AddObjectToSlot(DragAndDrop.objectReferences[i], slot);
            }

            for (int i = 0; i < DragAndDrop.paths.Length; i++)
            {
                if (AssetDatabase.IsValidFolder(DragAndDrop.paths[i]))
                {
                    AddAssetPathToSlot(DragAndDrop.paths[i], slot);
                }
            }

            RefreshSlotObjects();
            SaveGroups();
        }

        private void ClearSelectedSlot()
        {
            ClearSlot(slots[selectedSlotIndex]);
            RefreshSlotObjects();
            SaveGroups();
        }

        private void ClearSlot(SelectionGroupSlot slot)
        {
            slot.ObjectIds.Clear();
            slot.AssetPaths.Clear();
        }

        private void AddObjectToSlot(Object objectToAdd, SelectionGroupSlot slot)
        {
            string assetPath = AssetDatabase.GetAssetPath(objectToAdd);

            if (AssetDatabase.IsValidFolder(assetPath))
            {
                AddAssetPathToSlot(assetPath, slot);
                return;
            }

            GlobalObjectId globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(objectToAdd);
            string globalObjectIdString = globalObjectId.ToString();

            if (!slot.ObjectIds.Contains(globalObjectIdString))
            {
                slot.ObjectIds.Add(globalObjectIdString);
            }
        }

        private void AddAssetPathToSlot(string assetPath, SelectionGroupSlot slot)
        {
            if (!slot.AssetPaths.Contains(assetPath))
            {
                slot.AssetPaths.Add(assetPath);
            }
        }

        private bool SlotHasObjects(int slotIndex)
        {
            List<Object> slotObjects = GetSlotObjects(slotIndex);
            return slotObjects.Count > 0;
        }

        private List<Object> GetSlotObjects(int slotIndex)
        {
            List<Object> slotObjects = new List<Object>();
            SelectionGroupSlot slot = slots[slotIndex];

            for (int i = 0; i < slot.ObjectIds.Count; i++)
            {
                GlobalObjectId globalObjectId;
                if (GlobalObjectId.TryParse(slot.ObjectIds[i], out globalObjectId))
                {
                    Object slotObject = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
                    if (slotObject != null)
                    {
                        slotObjects.Add(slotObject);
                    }
                }
            }

            for (int i = 0; i < slot.AssetPaths.Count; i++)
            {
                Object slotObject = AssetDatabase.LoadAssetAtPath<Object>(slot.AssetPaths[i]);
                if (slotObject != null)
                {
                    slotObjects.Add(slotObject);
                }
            }

            return slotObjects;
        }

        private void LoadGroups()
        {
            slots.Clear();

            if (System.IO.File.Exists(SaveFilePath))
            {
                string json = System.IO.File.ReadAllText(SaveFilePath);
                SelectionGroupSaveData saveData = JsonUtility.FromJson<SelectionGroupSaveData>(json);

                if (saveData != null)
                {
                    selectedSlotIndex = saveData.SelectedSlotIndex;

                    if (saveData.Slots != null)
                    {
                        for (int i = 0; i < saveData.Slots.Count; i++)
                        {
                            slots.Add(saveData.Slots[i]);
                        }
                    }
                }
            }

            EnsureSlotCount();
        }

        private void SaveGroups()
        {
            EnsureSlotCount();

            SelectionGroupSaveData saveData = new SelectionGroupSaveData();
            saveData.SelectedSlotIndex = selectedSlotIndex;
            saveData.Slots = slots;

            string directory = System.IO.Path.GetDirectoryName(SaveFilePath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            System.IO.File.WriteAllText(SaveFilePath, JsonUtility.ToJson(saveData, true));
            AssetDatabase.Refresh();
        }

        private void EnsureSlotCount()
        {
            while (slots.Count < SlotCount)
            {
                slots.Add(new SelectionGroupSlot());
            }

            if (slots.Count > SlotCount)
            {
                slots.RemoveRange(SlotCount, slots.Count - SlotCount);
            }

            if (selectedSlotIndex < 0 || selectedSlotIndex >= SlotCount)
            {
                selectedSlotIndex = 0;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].ObjectIds == null)
                {
                    slots[i].ObjectIds = new List<string>();
                }

                if (slots[i].AssetPaths == null)
                {
                    slots[i].AssetPaths = new List<string>();
                }
            }
        }

        [System.Serializable]
        private class SelectionGroupSlot
        {
            public List<string> ObjectIds = new List<string>();
            public List<string> AssetPaths = new List<string>();
        }

        [System.Serializable]
        private class SelectionGroupSaveData
        {
            public int SelectedSlotIndex;
            public List<SelectionGroupSlot> Slots = new List<SelectionGroupSlot>();
        }
    }
}
