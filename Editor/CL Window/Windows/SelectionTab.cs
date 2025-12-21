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

        private Vector3 cachedSign;
        private Transform ParentTransform;

        public override void DrawContent()
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
                cachedSign = Vector3.zero;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            object2 = EditorGUILayout.ObjectField("Object 2", object2, typeof(GameObject), true) as GameObject;
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                EditorGUIUtility.PingObject(Selection.activeGameObject = object2);
                cachedSign = Vector3.zero;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Object 1"))
            {
                object1 = selectedObject;
                cachedSign = Vector3.zero;
            }
            if (GUILayout.Button("Set Object 2"))
            {
                object2 = selectedObject;
                cachedSign = Vector3.zero;
            }
            EditorGUILayout.EndHorizontal();

            if (object1 != null && object2 != null)
            {
                Vector3 worldDelta = object1.transform.position - object2.transform.position;
                Vector3 localDelta = object1.transform.InverseTransformDirection(worldDelta);

                if (cachedSign == Vector3.zero)
                {
                    cachedSign = new Vector3(
                        Mathf.Sign(localDelta.x),
                        Mathf.Sign(localDelta.y),
                        Mathf.Sign(localDelta.z)
                    );
                }

                Vector3 absLocalDelta = new Vector3(
                    Mathf.Abs(localDelta.x),
                    Mathf.Abs(localDelta.y),
                    Mathf.Abs(localDelta.z)
                );

                EditorGUI.BeginChangeCheck();
                Vector3 newAbsLocalDelta = EditorGUILayout.Vector3Field("Difference", absLocalDelta);
                if (EditorGUI.EndChangeCheck())
                {
                    Vector3 signedLocalDelta = new Vector3(
                        cachedSign.x * newAbsLocalDelta.x,
                        cachedSign.y * newAbsLocalDelta.y,
                        cachedSign.z * newAbsLocalDelta.z
                    );

                    Vector3 newWorldDelta = object1.transform.TransformDirection(signedLocalDelta);
                    Undo.RecordObject(object2.transform, "Move Object 2");
                    object2.transform.position = object1.transform.position - newWorldDelta;
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply difference to Snap"))
                {
                    EditorSnapSettings.move = absLocalDelta;
                }
                if (GUILayout.Button("Copy", GUILayout.Width(50)))
                {
                    EditorGUIUtility.systemCopyBuffer = absLocalDelta.Vector3ToString();
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(new GUIContent("Match Bounds Center", "Moves Object 1 so its Renderer bounds center matches Object 2")))
                {
                    MatchBoundsCenter(object1.GetComponent<Renderer>(), object2.GetComponent<Renderer>());
                }
            }

            ParentSystem();
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