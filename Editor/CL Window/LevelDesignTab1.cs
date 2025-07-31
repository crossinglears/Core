using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CrossingLearsEditor
{
    public partial class LevelDesignTab : CL_WindowTab_WIP
    {
        private bool roamingFoldout = false;
        private float roamingSpeed = 0.5f;

        private Vector3 roamingTargetPivot;
        private bool isRoamingMoving = false;

        private readonly HashSet<KeyCode> keysDown = new HashSet<KeyCode>();

        private void FreeCamUpdate(SceneView sceneView)
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
                        // Shift handled below as multiplier
                        break;
                }
            }

            if (move != Vector3.zero)
            {
                float speedMultiplier = (keysDown.Contains(KeyCode.LeftShift) || keysDown.Contains(KeyCode.RightShift)) ? 3f : 1f;
                roamingTargetPivot += move.normalized * roamingSpeed * speedMultiplier;
                isRoamingMoving = true;
            }
        }

        private void UpdateRoamingMovement(SceneView sceneView)
        {
            if (!isRoamingMoving) return;

            Vector3 current = sceneView.pivot;
            Vector3 next = Vector3.Lerp(current, roamingTargetPivot, 0.1f);
            sceneView.pivot = next;
            sceneView.Repaint();

            if ((next - roamingTargetPivot).sqrMagnitude < 0.0001f)
            {
                sceneView.pivot = roamingTargetPivot;
                isRoamingMoving = false;
            }
        }

        //     void FreeCamFoldout()
        //     {
        //         Rect foldoutRect = GUILayoutUtility.GetRect(16f, 22f, GUILayout.ExpandWidth(true));

        //         if (Event.current.type == EventType.MouseDown && foldoutRect.Contains(Event.current.mousePosition))
        //         {
        //             roamingFoldout = !roamingFoldout;
        //             Event.current.Use();

        //             if (roamingFoldout)
        //             {
        //                 SceneView.duringSceneGui -= FreeCamUpdate;
        //                 SceneView.duringSceneGui += FreeCamUpdate;

        //                 SceneView.duringSceneGui -= UpdateRoamingMovement;
        //                 SceneView.duringSceneGui += UpdateRoamingMovement;

        //                 Tools.current = Tool.None;
        //                 SceneView.FocusWindowIfItsOpen<SceneView>();

        //                 roamingTargetPivot = SceneView.lastActiveSceneView.pivot;
        //             }
        //             else
        //             {
        //                 SceneView.duringSceneGui -= FreeCamUpdate;
        //                 SceneView.duringSceneGui -= UpdateRoamingMovement;
        //                 Tools.current = Tool.Move;
        //                 keysDown.Clear(); // Clear any stuck keys
        //             }
        //         }

        //         EditorGUI.Foldout(foldoutRect, roamingFoldout, "Roaming Mode", true);

        //         if (roamingFoldout)
        //         {
        //             roamingSpeed = EditorGUILayout.FloatField("Move Speed", roamingSpeed);
        //         }
        //     }
        // }

        void FreeCamFoldout()
        {
            bool isActive = roamingFoldout;

            GUI.backgroundColor = isActive ? ActiveColor : DefaultColor;

            string buttonLabel = isActive ? "Roaming Mode (ACTIVE)" : "Roaming Mode";
            if (GUILayout.Button(buttonLabel))
            {
                roamingFoldout = !roamingFoldout;

                if (roamingFoldout)
                {
                    SceneView.duringSceneGui -= FreeCamUpdate;
                    SceneView.duringSceneGui += FreeCamUpdate;

                    SceneView.duringSceneGui -= UpdateRoamingMovement;
                    SceneView.duringSceneGui += UpdateRoamingMovement;

                    Tools.current = Tool.None;
                    SceneView.FocusWindowIfItsOpen<SceneView>();

                    roamingTargetPivot = SceneView.lastActiveSceneView.pivot;
                }
                else
                {
                    SceneView.duringSceneGui -= FreeCamUpdate;
                    SceneView.duringSceneGui -= UpdateRoamingMovement;
                    Tools.current = Tool.Move;
                    keysDown.Clear();
                }
            }

            GUI.backgroundColor = DefaultColor;

            if (roamingFoldout)
            {
                roamingSpeed = EditorGUILayout.FloatField("Move Speed", roamingSpeed);
            }
        }
    }
}
