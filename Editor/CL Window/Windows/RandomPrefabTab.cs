using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace CrossingLearsEditor
{
    public class RandomPrefabTab : CL_WindowTab
    {
        public override string TabName => "Random Prefab";

        public List<RandomSet> randomSets = new List<RandomSet>();

        private ReorderableList randomSetList;

        public override void Awake()
        {
            base.Awake();
            string path = "Assets/Editor/Development Files/RandomSets.json";
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                randomSets = JsonUtility.FromJson<RandomSetListWrapper>(json).Sets;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            string path = "Assets/Editor/Development Files/RandomSets.json";
            RandomSetListWrapper wrapper = new RandomSetListWrapper { Sets = randomSets };
            string json = JsonUtility.ToJson(wrapper, true);
            System.IO.File.WriteAllText(path, json);
        }

        [System.Serializable]
        private class RandomSetListWrapper
        {
            public List<RandomSet> Sets = new List<RandomSet>();
        }


        public RandomPrefabTab()
        {
            randomSetList = new ReorderableList(randomSets, typeof(RandomSet), true, false, false, false);
            randomSetList.drawElementCallback = DrawRandomSetElement;
            randomSetList.elementHeight = EditorGUIUtility.singleLineHeight + 6f;
        }

        public override void DrawContent()
        {
            randomSetList.list = randomSets;
            randomSetList.DoLayoutList();
        }

        private void DrawRandomSetElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            RandomSet set = randomSets[index];

            Rect menuRect = new Rect(rect.xMax - 25f, rect.y + 2f, 25f, EditorGUIUtility.singleLineHeight);
            Rect randomRect = new Rect(rect.xMax - 110f, rect.y + 2f, 80f, EditorGUIUtility.singleLineHeight);
            Rect editRect = new Rect(rect.xMax - 195f, rect.y + 2f, 80f, EditorGUIUtility.singleLineHeight);
            Rect nameRect = new Rect(rect.x, rect.y + 2f, rect.xMax - 210f, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(nameRect, set.RandomSetName);

            if (GUI.Button(editRect, "Edit"))
            {
                RandomSetEditorWindow.Open(set);
            }

            if (GUI.Button(randomRect, "Randomize"))
            {
                set.Randomize();
            }

            if (GUI.Button(menuRect, "⋮"))
            {
                GenericMenu menu = new GenericMenu();
                int captured = index;
                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    randomSets.RemoveAt(captured);
                });
                menu.ShowAsContext();
            }
        }

        public override void DrawTitle()
        {
            base.DrawTitle();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Create new Random Set"))
            {
                randomSets.Add(new RandomSet());
            }
            GUILayout.EndHorizontal();
        }

        [System.Serializable]
        public class RandomSet
        {
            public string RandomSetName = "";
            public List<string> PrefabPaths = new List<string>();

            public void Randomize()
            {
                if (PrefabPaths.Count == 0) return;

                List<GameObject> prefabs = new List<GameObject>();
                for (int i = 0; i < PrefabPaths.Count; i++)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPaths[i]);
                    if (prefab != null)
                        prefabs.Add(prefab);
                }

                if (prefabs.Count == 0) return;

                Transform[] targets = Selection.transforms;
                if (targets.Length == 0) return;

                Undo.IncrementCurrentGroup();
                int group = Undo.GetCurrentGroup();

                List<GameObject> created = new List<GameObject>();

                for (int i = 0; i < targets.Length; i++)
                {
                    Transform target = targets[i];
                    GameObject chosen = prefabs[Random.Range(0, prefabs.Count)];

                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(chosen, target.parent);
                    Undo.RegisterCreatedObjectUndo(instance, "Randomize Prefab");

                    instance.transform.position = target.position;
                    instance.transform.rotation = target.rotation;
                    instance.transform.localScale = target.localScale;

                    created.Add(instance);

                    Undo.DestroyObjectImmediate(target.gameObject);
                }

                Undo.CollapseUndoOperations(group);

                Selection.objects = created.ToArray();
            }
        }
    }

    
    public class RandomSetEditorWindow : EditorWindow
    {
        private RandomPrefabTab.RandomSet targetSet;
        private ReorderableList pathList;
        private string tempName;

        private const float Width = 400f;
        private const float Padding = 12f;

        public static void Open(RandomPrefabTab.RandomSet set)
        {
            RandomSetEditorWindow window = GetWindow<RandomSetEditorWindow>("Random Set Editor", true);
            window.targetSet = set;
            window.tempName = set.RandomSetName;
            window.Initialize();
            window.ShowUtility();
        }

        private void Initialize()
        {
            pathList = new ReorderableList(targetSet.PrefabPaths, typeof(string), true, true, true, true);
            pathList.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Prefab Paths");
            };
            pathList.drawElementCallback = (rect, index, active, focused) =>
            {
                targetSet.PrefabPaths[index] =
                    EditorGUI.TextField(
                        new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight),
                        targetSet.PrefabPaths[index]);
            };
        }

        private void OnGUI()
        {
            if (targetSet == null) return;

            EditorGUILayout.LabelField("Random Set Name");
            tempName = EditorGUILayout.TextField(tempName);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add All Selected Objects"))
            {
                Object[] selectedObjects = Selection.objects;

                foreach (Object obj in selectedObjects)
                {
                    string path = ExtensionTools.GetPathOfObjectSelected(obj);

                    if (!string.IsNullOrEmpty(path) && !targetSet.PrefabPaths.Contains(path))
                    {
                        targetSet.PrefabPaths.Add(path);
                    }
                }
            }
            if(GUILayout.Button("Remove Empty", GUILayout.Width(100)))
            {
                for (int i = targetSet.PrefabPaths.Count - 1; i >= 0; i--)
                {
                    if (string.IsNullOrEmpty(targetSet.PrefabPaths[i]) ||
                        AssetDatabase.LoadAssetAtPath<GameObject>(targetSet.PrefabPaths[i]) == null)
                    {
                        targetSet.PrefabPaths.RemoveAt(i);
                    }
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(6f);

            pathList.DoLayoutList();

            GUILayout.Space(6f);

            if (GUILayout.Button("Save"))
            {
                targetSet.RandomSetName = tempName;
                Close();
            }

            if (Event.current.type == EventType.Repaint)
            {
                float height = GUILayoutUtility.GetLastRect().yMax + Padding;
                Vector2 size = new Vector2(Width, height);
                minSize = size;
                maxSize = size;
            }
        }
    }
}
