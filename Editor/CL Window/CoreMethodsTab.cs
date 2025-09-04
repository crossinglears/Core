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
            StartStateControllerButton();
            UpgradeScrollRectButton();
            DisableAllNavigation();
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
    }
}
