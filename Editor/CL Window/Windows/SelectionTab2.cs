using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public partial class SelectionTab : CL_WindowTab
    {
        GameObject lastHierarchySelection;
        GameObject lastProjectSelection;

        void Replacers()
        {
            GUILayout.Space(10);

            if(Selection.activeGameObject)
            {
                if(Selection.activeGameObject.scene.IsValid())
                {
                    lastHierarchySelection = Selection.activeGameObject;
                }
                else
                {
                    lastProjectSelection = Selection.activeGameObject;
                }
            }

            EditorGUILayout.LabelField("Prefab Replacer", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(lastHierarchySelection == null || lastProjectSelection == null);
            EditorGUILayout.ObjectField("Hierarchy Object", lastHierarchySelection, typeof(GameObject), true);
            EditorGUILayout.ObjectField("Project Prefab", lastProjectSelection, typeof(GameObject), false);

            if(GUILayout.Button(new GUIContent("Replace With Prefab", "Replace the Heirarchy Object with the Project Object")))
            {
                int option = EditorUtility.DisplayDialogComplex(
                    "Replace With Prefab",
                    "Choose replace mode",
                    "Full Prefab Replace",
                    "Keep Overrides",
                    "Cancel"
                );

                if(option == 0)
                {
                    ReplacePrefab(false);
                }
                else if(option == 1)
                {
                    ReplacePrefab(true);
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        void ReplacePrefab(bool keepOverrides)
        {
            GameObject target = lastHierarchySelection;
            GameObject prefab = lastProjectSelection;

            if(target == null || prefab == null) return;

            Transform parent = target.transform.parent;
            Vector3 position = target.transform.position;
            Quaternion rotation = target.transform.rotation;
            Vector3 scale = target.transform.localScale;

            Undo.DestroyObjectImmediate(target);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);

            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = scale;

            if(keepOverrides)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance);
            }

            Selection.activeGameObject = instance;
        }
    }
}
