using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace CrossingLears
{
    public class TodoTab : CL_WindowTab
    {
        public override string TabName => "Todo";
        
        private List<TodoTask> todoTasks = new();
        private List<TodoTask> finishedTasks = new();
        private bool isEditing = false;
        private int editingIndex = -1;
        private ReorderableList reorderableList;

        [System.Serializable]
        public class TodoTask
        {
            public bool isDone;
            public string TaskName;
        }
        
        public override void OnEnable()
        {
            LoadTasks();
            reorderableList = new ReorderableList(todoTasks, typeof(TodoTask), true, true, false, false);
            
            reorderableList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Todo List");
            };
            
            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2;
                var task = todoTasks[index];
                float width = rect.width;
                
                // Checkbox
                bool wasDone = task.isDone;
                task.isDone = EditorGUI.Toggle(new Rect(rect.x, rect.y, 20, EditorGUIUtility.singleLineHeight), task.isDone);
                
                // Move to finished tasks if checked
                if (!wasDone && task.isDone)
                {
                    finishedTasks.Add(task);
                    todoTasks.RemoveAt(index);
                    SaveTasks();
                    GUIUtility.ExitGUI();
                    return;
                }
                
                // Task Name (Editable if in edit mode)
                if (isEditing && editingIndex == index)
                {
                    task.TaskName = EditorGUI.TextField(new Rect(rect.x + 25, rect.y, width - 150, EditorGUIUtility.singleLineHeight), task.TaskName);
                }
                else
                {
                    EditorGUI.LabelField(new Rect(rect.x + 25, rect.y, width - 80, EditorGUIUtility.singleLineHeight), task.TaskName);
                }
                
                // Delete Button
                if (GUI.Button(new Rect(rect.x + width - 60, rect.y, 50, EditorGUIUtility.singleLineHeight), "Delete"))
                {
                    todoTasks.RemoveAt(index);
                    SaveTasks();
                    GUIUtility.ExitGUI();
                    return;
                }

                // Edit Button
                if (GUI.Button(new Rect(rect.x + width - 120, rect.y, 50, EditorGUIUtility.singleLineHeight), isEditing && editingIndex == index ? "Save" : "Edit"))
                {
                    if (isEditing && editingIndex == index)
                    {
                        isEditing = false;
                        editingIndex = -1;
                        SaveTasks();
                    }
                    else
                    {
                        isEditing = true;
                        editingIndex = index;
                    }
                }
            };
        }

        public override void DrawContent()
        {
            EditorGUILayout.BeginVertical();
            
            // Add new task
            if (GUILayout.Button("Add Task"))
            {
                todoTasks.Add(new TodoTask { TaskName = "New Task" });
                
                isEditing = true;
                editingIndex = todoTasks.Count - 1;
                SaveTasks();
            }
            
            reorderableList.DoLayoutList(); // Display the reorderable list

            GUILayout.Space(10);
            
            DrawDoneTasks();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDoneTasks()
        {
            EditorGUILayout.LabelField($"Finished Tasks ({finishedTasks.Count})", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            if(finishedTasks.Count > 0)
            {
                foreach (TodoTask item in finishedTasks)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField( item.TaskName );
                    if (GUILayout.Button("Reassign"))
                    {
                        item.isDone = false;
                        todoTasks.Add(item);
                        finishedTasks.Remove(item);
                        SaveTasks();
                        GUIUtility.ExitGUI();
                        return;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Nothing here yet");
            }
            EditorGUI.indentLevel--;
        }

        private void SaveTasks()
        {
            string json = JsonUtility.ToJson(new TodoTaskListWrapper { tasks = todoTasks, finished = finishedTasks });
            EditorPrefs.SetString("TodoTab_Tasks", json);
        }
        
        private void LoadTasks()
        {
            if (EditorPrefs.HasKey("TodoTab_Tasks"))
            {
                string json = EditorPrefs.GetString("TodoTab_Tasks");
                TodoTaskListWrapper wrapper = JsonUtility.FromJson<TodoTaskListWrapper>(json);
                if (wrapper != null)
                {
                    todoTasks = wrapper.tasks ?? new List<TodoTask>();
                    finishedTasks = wrapper.finished ?? new List<TodoTask>();
                }
            }
        }
        
        [System.Serializable]
        private class TodoTaskListWrapper
        {
            public List<TodoTask> tasks;
            public List<TodoTask> finished;
        }
    }
}