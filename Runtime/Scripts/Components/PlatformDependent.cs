using UnityEngine;
using System;

#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace CrossingLears
{
    public partial class PlatformDependent : MonoBehaviour
    {
        #if UNITY_EDITOR
        public BuildTarget[] targetPlatform;
        #endif
    }

#if UNITY_EDITOR
    public partial class PlatformDependent : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        [Button("Add All")]
        public void AddAllPlatforms()
        {
            targetPlatform = Enum.GetValues(typeof(BuildTarget))
                .Cast<BuildTarget>()
                .Where(t => t != BuildTarget.NoTarget && !IsObsolete(t))
                .ToArray();
        }

        private static bool IsObsolete(BuildTarget target)
        {
            FieldInfo field = typeof(BuildTarget).GetField(target.ToString());
            return field?.GetCustomAttribute<ObsoleteAttribute>() != null;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log($"Build Platform: {report.summary.platform}");

            string activeScenePath = SceneManager.GetActiveScene().path;

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            foreach (var scene in scenes)
            {
                if (!scene.enabled) continue;

                Scene loadedScene = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                ProcessSceneObjects(report);
                EditorSceneManager.SaveScene(loadedScene);
            }

            if (!string.IsNullOrEmpty(activeScenePath))
            {
                EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);
            }
        }

        private void ProcessSceneObjects(BuildReport report)
        {
            PlatformDependent[] platformObjects = FindObjectsByType<PlatformDependent>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (PlatformDependent item in platformObjects)
            {
                if (!item.targetPlatform.Contains(report.summary.platform))
                {
                    Debug.Log($"[PlatformDependent] Excluding {item.gameObject.name} from build (Target: {string.Join(", ", item.targetPlatform)}, Build: {report.summary.platform})");
                    item.gameObject.hideFlags = HideFlags.DontSaveInBuild;
                }
                else
                {
                    item.gameObject.hideFlags = HideFlags.None;
                }
            }
        }
    }
#endif
}
