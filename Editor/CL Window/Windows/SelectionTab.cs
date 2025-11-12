using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CrossingLearsEditor
{
    public partial class SelectionTab : CL_WindowTab
    {
        public override string TabName => "Selection";

        private GameObject selectedObject;
        private GameObject firstSelectedObject;
        private GameObject lastSelectedObject;
        private List<GameObject> selectedObjects = new List<GameObject>();

        public override void DrawContent()
        {
            selectedObject = Selection.activeGameObject;

            selectedObjects.Clear();
            selectedObjects.AddRange(Selection.gameObjects);

            // Update firstSelectedObject only when selection starts or changes
            if (selectedObjects.Count > 0)
            {
                firstSelectedObject ??= selectedObjects[0]; // keep first selected until cleared
                lastSelectedObject = selectedObjects[selectedObjects.Count - 1];
            }
            else
            {
                firstSelectedObject = null;
                lastSelectedObject = null;
            }

            // Display selected objects
            EditorGUILayout.ObjectField("Selected Object", selectedObject, typeof(GameObject), true);
            EditorGUILayout.ObjectField("First Selected Object", firstSelectedObject, typeof(GameObject), true);
            EditorGUILayout.ObjectField("Last Selected Object", lastSelectedObject, typeof(GameObject), true);

            GUILayout.Space(10);

            if (selectedObjects.Count == 0) return;

            // Calculate average position
            Vector3 averagePosition = Vector3.zero;
            int validCount = 0;

            foreach (GameObject obj in selectedObjects)
            {
                if (obj != null)
                {
                    averagePosition += obj.transform.position;
                    validCount++;
                }
            }

            if (validCount > 0)
            {
                averagePosition /= validCount;
                EditorGUILayout.Vector3Field("Average Position", averagePosition);

                if (firstSelectedObject != null && lastSelectedObject != null)
                {
                    Vector3 difference = firstSelectedObject.transform.position - lastSelectedObject.transform.position;
                    EditorGUILayout.Vector3Field("Difference", difference);
                }
            }
        }
    }
}
