using UnityEngine;
using CrossingLears;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CrossingLears.Editor
{
    public partial class CoreMethodsTab : CL_WindowTab
    {
        public override string TabName => "Core Methods";

        public override void DrawContent()
        {
            GUILayout.Label("Basic");
            GroupSelectedObjects();
            DisableAllNavigation();
            RenameAllSelected();

            GUILayout.Space(10);
            GUILayout.Label("UI");
            UpgradeScrollRectButton();

            GUILayout.Space(10);
            GUILayout.Label("Utilities");
            StartStateControllerButton();
            MissingScriptsButton();

            GUILayout.Space(10);
            GUILayout.Label("Versioning");
            Versioning();
        }

        void StartStateControllerButton()
        {
            if (!GUILayout.Button("StartState Controller")) return;

            Scene scene = Selection.activeGameObject != null ? Selection.activeGameObject.scene : SceneManager.GetActiveScene();

            if (scene.GetRootGameObjects().Any(go => go.GetComponentInChildren<StartStateController>(true) != null))
            {
                Debug.LogWarning($"StartStateController already exists in scene: {scene.name}");
                return;
            }

            bool hasStartState = Object.FindObjectsByType<StartState>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Any(s => s.gameObject.scene == scene);

            if (hasStartState || UnityEditor.EditorUtility.DisplayDialog("No StartState found",
                "No StartState found in this scene.\n\nDo you want to spawn this controller?",
                "Spawn StartStateController", "Cancel"))
            {
                StartStateController.SpawnStartStateController(scene);
            }
        }

        void UpgradeScrollRectButton()
        {
            GameObject[] selectedGameObjects = Selection.gameObjects;
            GUI.enabled = selectedGameObjects != null && selectedGameObjects.Length > 0;

            if (GUILayout.Button("Upgrade ScrollRect"))
            {
                foreach (GameObject selectedGameObject in selectedGameObjects)
                {
                    SmoothScrollRect.ReplaceWithSmoothScrollRect(selectedGameObject);
                }
            }

            GUI.enabled = true;
        }

        void DisableAllNavigation()
        {
            if (GUILayout.Button(new GUIContent("Disable All Navigation in scene", "Sets all Selectables in the scene to have no navigation")))
            {
                Selectable[] selectables = Object.FindObjectsByType<Selectable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < selectables.Length; i++)
                {
                    Navigation nav = selectables[i].navigation;
                    nav.mode = Navigation.Mode.None;
                    selectables[i].navigation = nav;
                }
            }
        }

        public static void GroupSelectedObjects()
        {
            if (!GUILayout.Button(new GUIContent("Group Selected Objects", "Creates a new parent GameObject at the median position and majority rotation of selected objects, then parents them to it"))) 
                return;


            GameObject[] selected = Selection.gameObjects;
            if (selected.Length == 0) return;

            Vector3 min = selected[0].transform.position;
            Vector3 max = selected[0].transform.position;

            for (int i = 1; i < selected.Length; i++)
            {
                Vector3 pos = selected[i].transform.position;
                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }

            Vector3 median = (min + max) * 0.5f;

            Dictionary<Vector3, int> rotationCounts = new Dictionary<Vector3, int>();
            for (int i = 0; i < selected.Length; i++)
            {
                Vector3 rot = selected[i].transform.rotation.eulerAngles;
                if (rotationCounts.ContainsKey(rot)) rotationCounts[rot]++;
                else rotationCounts.Add(rot, 1);
            }

            Vector3 majorityRotation = Vector3.zero;
            int highest = -1;
            foreach (KeyValuePair<Vector3, int> kvp in rotationCounts)
            {
                if (kvp.Value > highest)
                {
                    highest = kvp.Value;
                    majorityRotation = kvp.Key;
                }
            }

            Transform parent = selected[0].transform.parent;

            GameObject holder = new GameObject("GroupedHolder");
            holder.transform.position = median;
            holder.transform.rotation = Quaternion.Euler(majorityRotation);
            holder.transform.SetParent(parent, true);

            for (int i = 0; i < selected.Length; i++)
            {
                Undo.SetTransformParent(selected[i].transform, holder.transform, "Parent Selected to Holder");
            }

            Selection.activeGameObject = holder;
        }

        void MissingScriptsButton()
        {
            if (GUILayout.Button("Missing Scripts Cleaner"))
            {
                MissingScriptsWindow.Open();
            }
        }

        void Versioning()
        {
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Patch Fix (0.0.1)"))
            {
                VersioningCommands.PatchFix();
            }
            if(GUILayout.Button("Minor Fix (0.1.0)"))
            {
                VersioningCommands.MinorFix();
            }
            if(GUILayout.Button("Major Fix (1.0.0)"))
            {
                VersioningCommands.MajorFix();
            }
            GUILayout.EndHorizontal();
        }
    }

    public class MissingScriptsWindow : EditorWindow
    {
        private List<GameObject> foundObjects = new List<GameObject>();
        private Vector2 scrollPosition;
        private bool hasScanned;

        public static void Open()
        {
            GetWindow<MissingScriptsWindow>("Missing Scripts").Show();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("List all objects with missing scripts"))
            {
                FindObjectsWithMissingScripts();
            }

            GUI.enabled = foundObjects.Count > 0;
            if (GUILayout.Button("Delete all missing script references"))
            {
                DeleteMissingScripts();
            }
            GUI.enabled = true;

            if (hasScanned)
            {
                GUILayout.Label($"Found: {foundObjects.Count}");
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < foundObjects.Count; i++)
            {
                EditorGUILayout.ObjectField(foundObjects[i], typeof(GameObject), true);
            }
            EditorGUILayout.EndScrollView();
        }

        private void FindObjectsWithMissingScripts()
        {
            foundObjects.Clear();
            hasScanned = true;

            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                {
                    continue;
                }

                GameObject[] roots = scene.GetRootGameObjects();
                for (int i = 0; i < roots.Length; i++)
                {
                    CollectMissing(roots[i].transform);
                }
            }
        }

        private void CollectMissing(Transform current)
        {
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(current.gameObject) > 0)
            {
                foundObjects.Add(current.gameObject);
            }

            for (int i = 0; i < current.childCount; i++)
            {
                CollectMissing(current.GetChild(i));
            }
        }

        private void DeleteMissingScripts()
        {
            int removedCount = 0;
            for (int i = 0; i < foundObjects.Count; i++)
            {
                if (foundObjects[i] == null)
                {
                    continue;
                }

                Undo.RegisterCompleteObjectUndo(foundObjects[i], "Remove Missing Scripts");
                removedCount += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(foundObjects[i]);
            }

            Debug.Log($"Removed {removedCount} missing script reference(s)");
            foundObjects.Clear();
        }
    }
}
