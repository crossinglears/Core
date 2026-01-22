using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public partial class SelectionTab : CL_WindowTab
    {
        private GameObject selectedObject;
        private GameObject object1;
        private GameObject object2;

        private Vector3 cachedSign;
        private Transform ParentTransform;
        
        void SelectionBasic()
        {
            if (GUILayout.Button("Clear Selection"))
            {
                selectedObject = null;
                object1 = null;
                object2 = null;
                cachedSign = Vector3.zero;
                Selection.activeGameObject = null;
                return;
            }

            selectedObject = Selection.activeGameObject;

            EditorGUILayout.ObjectField("Selected Object", selectedObject, typeof(GameObject), true);

            GUILayout.BeginHorizontal();
            object1 = EditorGUILayout.ObjectField("Object 1", object1, typeof(GameObject), true) as GameObject;
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(Selection.activeGameObject = object1);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            object2 = EditorGUILayout.ObjectField("Object 2", object2, typeof(GameObject), true) as GameObject;
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(Selection.activeGameObject = object2);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (Selection.transforms.Length == 2)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Assign Selected Objects"))
                {
                    object1 = Selection.transforms[1].gameObject;
                    Selection.activeGameObject = object2 = Selection.transforms[0].gameObject;
                }
                if(GUILayout.Button("Zero", GUILayout.Width(50)))
                {
                    object1 = Selection.transforms[1].gameObject;
                    object2 = Selection.transforms[0].gameObject;
                    
                    Undo.RecordObject(object2.transform, "Zero Relative Position");
                    object2.transform.position = object1.transform.position;
                    Selection.activeGameObject = object2;
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set Object 1"))
                {
                    object1 = selectedObject;
                }

                if (GUILayout.Button("Set Object 2"))
                {
                    object2 = selectedObject;
                }
                EditorGUILayout.EndHorizontal();
            }


            if (object1 != null && object2 != null)
            {
                Vector3 worldDelta = object1.transform.position - object2.transform.position;
                Vector3 localDelta = object1.transform.InverseTransformDirection(worldDelta);

                EditorGUI.BeginChangeCheck();
                Vector3 newLocalDelta = EditorGUILayout.Vector3Field("Difference", localDelta);
                if (EditorGUI.EndChangeCheck())
                {
                    Vector3 newWorldDelta = object1.transform.TransformDirection(newLocalDelta);
                    Undo.RecordObject(object2.transform, "Move Object 2");
                    object2.transform.position = object1.transform.position - newWorldDelta;
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply difference to Snap"))
                {
                    EditorSnapSettings.move = new Vector3(
                        Mathf.Abs(localDelta.x),
                        Mathf.Abs(localDelta.y),
                        Mathf.Abs(localDelta.z)
                    );
                }
                if (GUILayout.Button("Copy", GUILayout.Width(50)))
                {
                    EditorGUIUtility.systemCopyBuffer = localDelta.Vector3ToString();
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(new GUIContent("Match Bounds Center", "Moves Object 1 so its Renderer bounds center matches Object 2")))
                {
                    MatchBoundsCenter(object1.GetComponent<Renderer>(), object2.GetComponent<Renderer>());
                }
            }
        }

        void ParentSystem()
        {
            GUILayout.Space(20);
            ParentTransform = EditorGUILayout.ObjectField("Parent", ParentTransform, typeof(Transform), true) as Transform;

            if (GUILayout.Button("Assign Parent"))
            {
                ParentTransform = selectedObject.transform;
            }
            if (GUILayout.Button("Assign Children"))
            {
                foreach (Transform item in Selection.transforms)
                {
                    Undo.SetTransformParent(item, ParentTransform, "Assign Children");
                }
            }

            if (GUILayout.Button("Unparent"))
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

        public static void MatchBoundsCenter(Renderer source, Renderer target)
        {
            Vector3 sourceCenter = source.bounds.center;
            Vector3 targetCenter = target.bounds.center;

            Vector3 delta = targetCenter - sourceCenter;
            source.transform.position += delta;
        }
    }
}