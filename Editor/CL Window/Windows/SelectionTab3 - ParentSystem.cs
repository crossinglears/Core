using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public partial class SelectionTab : CL_WindowTab
    {
        public List<Transform> ChildrensCached = new();
        void ParentSystem()
        {
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            ParentTransform = EditorGUILayout.ObjectField("Parent", ParentTransform, typeof(Transform), true) as Transform;

            if (GUILayout.Button("Assign Parent", GUILayout.Width(150)))
            {
                if(selectedObject)
                {
                    ParentTransform = selectedObject.transform;
                    if(ChildrensCached.Count != 0)
                    {
                        foreach (Transform item in ChildrensCached)
                        {
                            Undo.SetTransformParent(item, ParentTransform, "Assign Children");
                        }
                        ChildrensCached.Clear();
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Children Count: {ChildrensCached.Count} " ,GUILayout.Width(EditorGUIUtility.labelWidth - 5));
            if (GUILayout.Button("Clear", GUILayout.ExpandWidth(true)))
            {
                ChildrensCached.Clear();
            }
            if (GUILayout.Button("Assign Children", GUILayout.Width(150)))
            {
                if(ParentTransform != null)
                {
                    foreach (Transform item in Selection.transforms)
                    {
                        Undo.SetTransformParent(item, ParentTransform, "Assign Children");
                    }
                }
                else
                {
                    foreach(var item in Selection.transforms)
                    {
                        if(!ChildrensCached.Contains(item))
                        {
                            ChildrensCached.Add(item);
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Unparent All Selected Children"))
            {
                Transform[] targets = Selection.transforms;

                void DoUnparent(Transform t)
                {
                    Undo.RecordObject(t, "Unparent");

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