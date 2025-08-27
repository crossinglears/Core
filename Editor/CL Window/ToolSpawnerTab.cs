using UnityEngine;
using CrossingLears;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEditor;

namespace CrossingLearsEditor
{
    public class ToolSpawnerTab : CL_WindowTab_WIP
    {
        public override string TabName => "Core Methods";

        public override void DrawContent()
        {
            StartStateControllerButton();
            UpgradeScrollRectButton();
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
    }
}
