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
        
public override void DrawContent()
{
    if (GUILayout.Button("Clear Selection"))
    {
        selectedObject = null;
        firstSelectedObject = null;
        lastSelectedObject = null;
        Selection.activeGameObject = null;
        return;
    }

    selectedObject = Selection.activeGameObject;
    GameObject[] current = Selection.gameObjects;
    int count = current.Length;

    if (count > 0)
    {
        if (firstSelectedObject == null)
        {
            firstSelectedObject = current[0];
        }

        lastSelectedObject = current[count - 1];
    }
    else
    {
        firstSelectedObject = null;
        lastSelectedObject = null;
    }

    EditorGUILayout.ObjectField("Selected Object", selectedObject, typeof(GameObject), true);
    EditorGUILayout.ObjectField("First Selected Object", firstSelectedObject, typeof(GameObject), true);
    EditorGUILayout.ObjectField("Last Selected Object", lastSelectedObject, typeof(GameObject), true);

    GUILayout.Space(10);

    Vector3 averagePosition = Vector3.zero;
    int validCount = 0;

    for (int i = 0; i < count; i++)
    {
        GameObject obj = current[i];
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
            Vector3 difference = firstSelectedObject.transform.localPosition - lastSelectedObject.transform.localPosition;
            difference = new Vector3(Mathf.Abs(difference.x), Mathf.Abs(difference.y), Mathf.Abs(difference.z));
            EditorGUILayout.Vector3Field("Difference", difference);

            if(GUILayout.Button("Apply difference to Snap"))
            {
                EditorSnapSettings.move = difference;                    
            }
        }
    }

    ParentSystem();
}


        private Transform ParentTransform;
        void ParentSystem()
        {
            GUILayout.Space(20);
            ParentTransform = EditorGUILayout.ObjectField("Parent", ParentTransform, typeof(Transform), true) as Transform;

            if(GUILayout.Button("Assign Parent"))
            {
                ParentTransform = selectedObject.transform;
            }
            if(GUILayout.Button("Assign Children"))
            {
                foreach(Transform item in Selection.transforms)
                {
                    item.SetParent(ParentTransform);
                }
            }
            
            if (GUILayout.Button("Unparent"))
            {
                Transform[] targets = Selection.transforms;

                void DoUnparent(Transform t)
                {
                    Transform parent = t.parent;

                    if (parent == null)
                    {
                        t.SetParent(null);
                        return;
                    }

                    Transform grand = parent.parent;
                    int targetIndex = parent.GetSiblingIndex() + 1;

                    if (grand == null)
                    {
                        t.SetParent(null);
                        t.SetSiblingIndex(targetIndex);
                        return;
                    }

                    t.SetParent(grand);
                    t.SetSiblingIndex(targetIndex);
                }

                for (int i = 0; i < targets.Length; i++)
                {
                    DoUnparent(targets[i]);
                }
                
                EditorGUIUtility.PingObject(selectedObject);
            }

        }
    }

}
