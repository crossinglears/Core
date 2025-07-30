using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public class BumpSnapping : CL_WindowTab_WIP
    {
        public override string TabName => "Smart Bump Snap";

        private static bool isEnabled = false;
        private static float snapDistance = 1f;

        public override void DrawContent()
        {
            string label = isEnabled ? "Disable Smart Bump Snap" : "Enable Smart Bump Snap";
            if (GUILayout.Button(label, GUILayout.Height(30)))
            {
                Tools.hidden = isEnabled = !isEnabled;

                if (isEnabled)
                {
                    // Enabled
                    TurnOn();
                }
                else
                    SceneView.duringSceneGui -= OnSceneGUI;

                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Disable Tools", GUILayout.Height(30)))
            {
                Tools.hidden = true;
            }

            if (GUILayout.Button("Enable Tools", GUILayout.Height(30)))
            {
                Tools.hidden = false;
            }

            if (GUILayout.Button("Enable Tools", GUILayout.Height(30)))
            {
                Tools.hidden = false;
            }

            snapDistance = EditorGUILayout.FloatField("Snap Distance", snapDistance);

            if (isEnabled)
            {
                GUILayout.Label("Smart Bump Snapping is ACTIVE", EditorStyles.helpBox);
                EditorGUILayout.Vector3Field("Last Position", lastPosition);
                EditorGUILayout.Vector3Field("Meto Position", XXXXXXXXXPosition);
                PositionWithConfirmedHit = EditorGUILayout.Vector3Field(
                    "Position With Confirmed Hit",
                    PositionWithConfirmedHit.HasValue ? PositionWithConfirmedHit.Value : new Vector3(float.NaN, float.NaN, float.NaN)
                );
            }
        }

        private void TurnOn()
        {
            PositionWithConfirmedHit = null;
            if (Selection.activeTransform)
            {
                lastPosition = Selection.activeTransform.position;
            }
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public override void OnUnfocus()
        {
            isEnabled = false;
            Tools.hidden = false;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private static Vector3 lastPosition;
        private static Vector3? PositionWithConfirmedHit;
        private static Transform lastTransform;

        public static Vector3 newPosition;

        private static Vector3 XXXXXXXXXPosition;

        private static void OnSceneGUI(SceneView sceneView)
        {
            Transform t = Selection.activeTransform;
            if (t == null) return;

            if (t != lastTransform)
            {
                // new Transform selected
                lastPosition = t.transform.position;
                lastTransform = t;
                PositionWithConfirmedHit = null;
            }

            Renderer renderer = t.GetComponent<Renderer>();
            if (renderer == null) return;

            EditorGUI.BeginChangeCheck();
            newPosition = Handles.PositionHandle(lastPosition, t.rotation);
            XXXXXXXXXPosition = newPosition;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(t, "Move with Smart Bump Snap");

                Vector3 delta = newPosition - lastPosition;

                Vector3 dir = Vector3.zero;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y) && Mathf.Abs(delta.x) > Mathf.Abs(delta.z))
                    dir = delta.x > 0 ? Vector3.right : Vector3.left;
                else if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x) && Mathf.Abs(delta.y) > Mathf.Abs(delta.z))
                    dir = delta.y > 0 ? Vector3.up : Vector3.down;
                else if (Mathf.Abs(delta.z) > Mathf.Abs(delta.x) && Mathf.Abs(delta.z) > Mathf.Abs(delta.y))
                    dir = delta.z > 0 ? Vector3.forward : Vector3.back;

                Vector3 size = renderer.bounds.size;
                Vector3 center = renderer.bounds.center;
                Vector3 halfExtents = size * 0.5f;

                Handles.color = Color.red;
                Handles.DrawWireCube(center, size);

                var coll = t.GetComponent<Collider>();

                if(coll != null) coll.enabled = false;
                RaycastHit[] hits = Physics.BoxCastAll(center, halfExtents * 0.5f, dir, Quaternion.identity, snapDistance);
                if(coll != null) coll.enabled = true;

                float closestDistance = Mathf.Infinity;
                Vector3 adjustedPosition = newPosition;


                foreach (RaycastHit h in hits)
                {
                    if (h.transform == t) continue;

                    float dist = h.distance;
                    if (dist < closestDistance)
                    {
                        closestDistance = dist;

                        Bounds bounds = renderer.bounds;

                        if (dir == Vector3.right)
                            adjustedPosition.x = h.point.x - bounds.extents.x;
                        else if (dir == Vector3.left)
                            adjustedPosition.x = h.point.x + bounds.extents.x;
                        else if (dir == Vector3.up)
                            adjustedPosition.y = h.point.y - bounds.extents.y;
                        else if (dir == Vector3.down)
                            adjustedPosition.y = h.point.y + bounds.extents.y;
                        else if (dir == Vector3.forward)
                            adjustedPosition.z = h.point.z - bounds.extents.z;
                        else if (dir == Vector3.back)
                            adjustedPosition.z = h.point.z + bounds.extents.z;
                    }
                }

                if (closestDistance < Mathf.Infinity)
                {
                    newPosition = adjustedPosition;
                    PositionWithConfirmedHit = newPosition;
                }

                if (newPosition != lastPosition)
                {
                    if (PositionWithConfirmedHit != null && Vector3.Distance(PositionWithConfirmedHit.Value, newPosition) < snapDistance)
                    {
                        t.position = PositionWithConfirmedHit.Value;
                        lastPosition = newPosition;
                    }
                    else
                    {
                        lastPosition = t.position = newPosition;
                    }
                }
            }
            SceneView.RepaintAll();
        }
    }
}
