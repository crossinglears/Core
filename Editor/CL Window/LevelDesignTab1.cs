using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CrossingLearsEditor
{
    public partial class LevelDesignTab : CL_WindowTab_WIP
    {
        private bool freeMoveEnabled = false;
        private float freeMoveSpeed = 0.5f;

        private Vector3 freeMoveTargetPivot;
        private bool isFreeMoveActive = false;

        private readonly HashSet<KeyCode> keysDown = new HashSet<KeyCode>();

        private void FreeMoveUpdate(SceneView sceneView)
        {
            Event e = Event.current;

            // Register key press
            if (e.type == EventType.KeyDown)
            {
                if (!keysDown.Contains(e.keyCode))
                    keysDown.Add(e.keyCode);
                e.Use();
            }

            // Register key release
            if (e.type == EventType.KeyUp)
            {
                if (keysDown.Contains(e.keyCode))
                    keysDown.Remove(e.keyCode);
                e.Use();
            }

            if (keysDown.Count == 0) return;

            Vector3 move = Vector3.zero;

            foreach (var key in keysDown)
            {
                switch (key)
                {
                    case KeyCode.W: move += sceneView.camera.transform.forward; break;
                    case KeyCode.S: move -= sceneView.camera.transform.forward; break;
                    case KeyCode.A: move -= sceneView.camera.transform.right; break;
                    case KeyCode.D: move += sceneView.camera.transform.right; break;
                    case KeyCode.Q: move -= sceneView.camera.transform.up; break;
                    case KeyCode.E: move += sceneView.camera.transform.up; break;
                    case KeyCode.LeftShift:
                    case KeyCode.RightShift:
                        break;
                }
            }

            if (move != Vector3.zero)
            {
                float speedMultiplier = (keysDown.Contains(KeyCode.LeftShift) || keysDown.Contains(KeyCode.RightShift)) ? 3f : 1f;
                freeMoveTargetPivot = sceneView.pivot + move.normalized * freeMoveSpeed * speedMultiplier;
                isFreeMoveActive = true;
            }
        }

        private void UpdateFreeMove(SceneView sceneView)
        {
            if (!isFreeMoveActive) return;

            Vector3 current = sceneView.pivot;
            Vector3 next = Vector3.Lerp(current, freeMoveTargetPivot, 0.1f);
            sceneView.pivot = next;
            sceneView.Repaint();

            if ((next - freeMoveTargetPivot).sqrMagnitude < 0.0001f)
            {
                sceneView.pivot = freeMoveTargetPivot;
                isFreeMoveActive = false;
            }
        }

        void FreeMoveButton()
        {
            bool isActive = freeMoveEnabled;

            GUI.backgroundColor = isActive ? ActiveColor : DefaultColor;

            string buttonLabel = isActive ? "Free Move Mode (ACTIVE)" : "Free Move Mode";
            if (GUILayout.Button(buttonLabel))
            {
                freeMoveEnabled = !freeMoveEnabled;

                if (freeMoveEnabled)
                {
                    SceneView.duringSceneGui -= FreeMoveUpdate;
                    SceneView.duringSceneGui += FreeMoveUpdate;

                    SceneView.duringSceneGui -= UpdateFreeMove;
                    SceneView.duringSceneGui += UpdateFreeMove;

                    Tools.current = Tool.None;
                    SceneView.FocusWindowIfItsOpen<SceneView>();

                    freeMoveTargetPivot = SceneView.lastActiveSceneView.pivot;
                }
                else
                {
                    SceneView.duringSceneGui -= FreeMoveUpdate;
                    SceneView.duringSceneGui -= UpdateFreeMove;
                    Tools.current = Tool.Move;
                    keysDown.Clear();
                }
            }

            GUI.backgroundColor = DefaultColor;

            if (freeMoveEnabled)
            {
                freeMoveSpeed = EditorGUILayout.FloatField("Move Speed", freeMoveSpeed);
                EditorGUILayout.Vector3Field("Target Pivot", freeMoveTargetPivot);
            }
        }
    }
}
