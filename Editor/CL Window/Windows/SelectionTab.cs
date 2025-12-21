using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public partial class SelectionTab : CL_WindowTab
    {
        public override string TabName => "Selection";

        private GameObject selectedObject;
        private GameObject object1;
        private GameObject object2;

        public override void DrawContent()
        {
            if (GUILayout.Button("Clear Selection"))
            {
                selectedObject = null;
                object1 = null;
                object2 = null;
                Selection.activeGameObject = null;
                return;
            }

            selectedObject = Selection.activeGameObject;

            EditorGUILayout.ObjectField("Selected Object", selectedObject, typeof(GameObject), true);
            GUILayout.BeginHorizontal();
            object1 = EditorGUILayout.ObjectField("Object 1", object1, typeof(GameObject), true) as GameObject;
            if(GUILayout.Button("Select", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(Selection.activeGameObject = object1);
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            object2 = EditorGUILayout.ObjectField("Object 2", object2, typeof(GameObject), true) as GameObject;
            if(GUILayout.Button("Select", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(Selection.activeGameObject = object2);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

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
if (object1 != null && object2 != null)
{
    Vector3 worldDelta = object1.transform.position - object2.transform.position;
    Vector3 rotatedDelta = object1.transform.InverseTransformDirection(worldDelta);

    rotatedDelta = new Vector3(
        Mathf.Abs(rotatedDelta.x),
        Mathf.Abs(rotatedDelta.y),
        Mathf.Abs(rotatedDelta.z)
    );

    Vector3 newRotatedDelta = EditorGUILayout.Vector3Field("Difference", rotatedDelta);

    if (newRotatedDelta != rotatedDelta)
    {
        Vector3 signedLocalDelta = new Vector3(
            Mathf.Sign(object1.transform.InverseTransformDirection(worldDelta).x) * newRotatedDelta.x,
            Mathf.Sign(object1.transform.InverseTransformDirection(worldDelta).y) * newRotatedDelta.y,
            Mathf.Sign(object1.transform.InverseTransformDirection(worldDelta).z) * newRotatedDelta.z
        );

        Vector3 newWorldDelta = object1.transform.TransformDirection(signedLocalDelta);
        object2.transform.position = object1.transform.position - newWorldDelta;
    }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply difference to Snap"))
                {
                    EditorSnapSettings.move = rotatedDelta;
                }
                if (GUILayout.Button("Copy", GUILayout.Width(50)))
                {
                    EditorGUIUtility.systemCopyBuffer = rotatedDelta.Vector3ToString();
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(new GUIContent("Match Bounds Center", "Moves Object 1 so its Renderer bounds center matches Object 2")))
                {
                    MatchBoundsCenter(object1.GetComponent<Renderer>(), object2.GetComponent<Renderer>());
                }
            }

            ParentSystem();
        }

        private Transform ParentTransform;
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
