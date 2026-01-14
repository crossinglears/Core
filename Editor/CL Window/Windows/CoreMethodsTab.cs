using UnityEngine;
using CrossingLears;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEditor;
using UnityEngine.UI;

namespace CrossingLearsEditor
{
    public class CoreMethodsTab : CL_WindowTab
    {
        public override string TabName => "Core Methods";

        public override void DrawContent()
        {
            GUILayout.Label("Basic");
            StartStateControllerButton();
            DisableAllNavigation();
            RenameAllSelected();

            GUILayout.Space(10);
            GUILayout.Label("UI");
            UpgradeScrollRectButton();

            GUILayout.Space(10);
            GUILayout.Label("Versioning");
            Versioning();
        }

        void StartStateControllerButton()
        {
            if (!GUILayout.Button("StartState Controller", GUILayout.Height(25))) return;

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

            if (GUILayout.Button("Upgrade ScrollRect", GUILayout.Height(25)))
            {
                SmoothScrollRect.ReplaceWithSmoothScrollRect(Selection.activeGameObject);
            }

            GUI.enabled = true;
        }

        void DisableAllNavigation()
        {
            if (GUILayout.Button("Disable All Navigation in scene", GUILayout.Height(25)))
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

        void RenameAllSelected()
        {
            if (!GUILayout.Button("Rename All Selected", GUILayout.Height(25))) return;
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
            if(GUILayout.Button("Patch Fix (0.0.1)", GUILayout.Height(25)))
            {
                VersioningCommands.PatchFix();
            }
            if(GUILayout.Button("Minor Fix (0.1.0)", GUILayout.Height(25)))
            {
                VersioningCommands.MinorFix();
            }
            GUILayout.EndHorizontal();
        }
    }
}
