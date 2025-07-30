using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public partial class LevelDesignTab : CL_WindowTab_WIP
    {
        private bool roamingFoldout = false;
        private float roamingSpeed = 0.5f;
        
        private void FreeCamUpdate(SceneView sceneView)
        {
            Debug.Log("FreeCamUpdate");
            Event e = Event.current;
            if (e.type != EventType.KeyDown && e.type != EventType.KeyUp) return;

            Vector3 move = Vector3.zero;

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.W) move += sceneView.camera.transform.forward;
                if (e.keyCode == KeyCode.A) move -= sceneView.camera.transform.right;
                if (e.keyCode == KeyCode.S) move -= sceneView.camera.transform.forward;
                if (e.keyCode == KeyCode.D) move += sceneView.camera.transform.right;
                if (e.keyCode == KeyCode.E) move -= sceneView.camera.transform.up;
                if (e.keyCode == KeyCode.Q) move += sceneView.camera.transform.up;

                if (move != Vector3.zero)
                {
                    Undo.RecordObject(sceneView, "Free Cam Move");
                    sceneView.pivot += move * roamingSpeed * (e.shift ? 3 : 1);
                    sceneView.Repaint();
                }
            }
        }

        void FreeCamFoldout()
        {
            Rect foldoutRect = GUILayoutUtility.GetRect(16f, 22f, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.MouseDown && foldoutRect.Contains(Event.current.mousePosition))
            {
                roamingFoldout = !roamingFoldout;
                Event.current.Use();

                if (roamingFoldout)
                {
                    SceneView.duringSceneGui -= FreeCamUpdate;
                    SceneView.duringSceneGui += FreeCamUpdate;
                    Tools.current = Tool.None;
                    SceneView.FocusWindowIfItsOpen<SceneView>();
                }
                else
                {
                    SceneView.duringSceneGui -= FreeCamUpdate;
                    Tools.current = Tool.Move;
                }
            }
            EditorGUI.Foldout(foldoutRect, roamingFoldout, "Roaming Mode", true);


            if (roamingFoldout)
            {
                roamingSpeed = EditorGUILayout.FloatField("Move Speed", roamingSpeed);
            }
        }
    }
}
