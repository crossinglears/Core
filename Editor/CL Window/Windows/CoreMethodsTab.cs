using UnityEngine;
using CrossingLears;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CrossingLearsEditor
{
    public class CoreMethodsTab : CL_WindowTab
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

            if (hasStartState || EditorUtility.DisplayDialog("No StartState found",
                "No StartState found in this scene.\n\nDo you want to spawn this controller?",
                "Spawn StartStateController", "Cancel"))
            {
                StartStateController.SpawnStartStateController(scene);
            }
        }

        void UpgradeScrollRectButton()
        {
            if (Selection.activeGameObject == null)
                GUI.enabled = false;

            if (GUILayout.Button("Upgrade ScrollRect"))
            {
                SmoothScrollRect.ReplaceWithSmoothScrollRect(Selection.activeGameObject);
            }

            GUI.enabled = true;
        }

        void DisableAllNavigation()
        {
            if (GUILayout.Button("Disable All Navigation in scene"))
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
            if(!GUILayout.Button("Group Selected Objects")) return;

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

        void RenameAllSelected()
        {
            if (!GUILayout.Button("Rename All Selected")) return;
            foreach(Transform item in Selection.transforms)
            {

                GameObject gameObject = item.gameObject;
                Undo.RecordObject(gameObject, "Rename All Selected");
                string text = gameObject.name;

                Text textComponent = gameObject.GetComponentInChildren<Text>();
                if (textComponent != null)
                    text = textComponent.text;
                
                TMPro.TextMeshProUGUI textMeshProUGUI = gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (textMeshProUGUI != null)
                    text = textMeshProUGUI.text;
                
                TMPro.TextMeshPro textMeshPro = gameObject.GetComponentInChildren<TMPro.TextMeshPro>();
                if (textMeshPro != null)
                    text = textMeshPro.text;

                gameObject.name = System.Text.RegularExpressions.Regex
                    .Replace(text, "<.*?>", string.Empty)
                    .Replace("\n", " ");

                EditorUtility.SetDirty(gameObject);
            }

            if (!Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
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
            GUILayout.EndHorizontal();
        }
    }
}
