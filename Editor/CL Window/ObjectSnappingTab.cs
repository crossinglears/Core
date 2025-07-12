using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public class ObjectSnappingTab : CL_WindowTab
    {
        public override string TabName => "Object Snapping";

        private float gridX = 1f;
        private float gridY = 1f;
        private float gridZ = 1f;

        private float rotX = 15f;
        private float rotY = 15f;
        private float rotZ = 15f;

        // public override void DrawContent()
        // {
        //     if (GUILayout.Button("Snap Position", GUILayout.Height(30)))
        //     {
        //         foreach (GameObject go in Selection.gameObjects)
        //         {
        //             Undo.RecordObject(go.transform, "Snap Position");
        //             Vector3 pos = go.transform.position;
        //             pos.x = Mathf.Round(pos.x / gridX) * gridX;
        //             pos.y = Mathf.Round(pos.y / gridY) * gridY;
        //             pos.z = Mathf.Round(pos.z / gridZ) * gridZ;
        //             go.transform.position = pos;
        //         }
        //     }

        //     GUILayout.Space(10);
        //     DrawSnapRow("X", ref gridX, SnapPosX);
        //     DrawSnapRow("Y", ref gridY, SnapPosY);
        //     DrawSnapRow("Z", ref gridZ, SnapPosZ);

        //     GUILayout.Space(20);
        //     GUILayout.Label("Rotation", EditorStyles.boldLabel);

        //     DrawSnapRow("X", ref rotX, SnapRotX);
        //     DrawSnapRow("Y", ref rotY, SnapRotY);
        //     DrawSnapRow("Z", ref rotZ, SnapRotZ);
        // }
        public override void DrawContent()
{
    GUILayout.Label("Position", EditorStyles.boldLabel);
    if (GUILayout.Button("Snap Position", GUILayout.Height(25)))
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "Snap Position");
            Vector3 pos = go.transform.position;
            pos.x = Mathf.Round(pos.x / gridX) * gridX;
            pos.y = Mathf.Round(pos.y / gridY) * gridY;
            pos.z = Mathf.Round(pos.z / gridZ) * gridZ;
            go.transform.position = pos;
        }
    }

    GUILayout.Space(5);
    DrawSnapRow("X", ref gridX, SnapPosX);
    DrawSnapRow("Y", ref gridY, SnapPosY);
    DrawSnapRow("Z", ref gridZ, SnapPosZ);

    GUILayout.Space(15);
    GUILayout.Label("Rotation", EditorStyles.boldLabel);
    if (GUILayout.Button("Snap Rotation", GUILayout.Height(25)))
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            Undo.RecordObject(go.transform, "Snap Rotation");
            Vector3 rot = go.transform.eulerAngles;
            rot.x = Mathf.Round(rot.x / rotX) * rotX;
            rot.y = Mathf.Round(rot.y / rotY) * rotY;
            rot.z = Mathf.Round(rot.z / rotZ) * rotZ;
            go.transform.eulerAngles = rot;
        }
    }

    GUILayout.Space(5);
    DrawSnapRow("X", ref rotX, SnapRotX);
    DrawSnapRow("Y", ref rotY, SnapRotY);
    DrawSnapRow("Z", ref rotZ, SnapRotZ);
}


        // private void DrawSnapRow(string label, ref float value, System.Action onSnap)
        // {
        //     EditorGUILayout.BeginHorizontal();
        //     GUILayout.Label(label, GUILayout.Width(20));
        //     value = EditorGUILayout.FloatField(value, GUILayout.Width(60));
        //     if (GUILayout.Button($"Snap {label}", GUILayout.Width(100)))
        //         onSnap.Invoke();
        //     EditorGUILayout.EndHorizontal();
        // }

        private void DrawSnapRow(string label, ref float value, System.Action onSnap)
{
    EditorGUILayout.BeginHorizontal();
    GUILayout.Label(label, GUILayout.Width(20));
    value = EditorGUILayout.FloatField(value);
    if (GUILayout.Button($"Snap {label}", GUILayout.Width(100)))
        onSnap.Invoke();
    EditorGUILayout.EndHorizontal();
}


        private void SnapPosX()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                Undo.RecordObject(go.transform, "Snap Position X");
                Vector3 pos = go.transform.position;
                pos.x = Mathf.Round(pos.x / gridX) * gridX;
                go.transform.position = pos;
            }
        }

        private void SnapPosY()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                Undo.RecordObject(go.transform, "Snap Position Y");
                Vector3 pos = go.transform.position;
                pos.y = Mathf.Round(pos.y / gridY) * gridY;
                go.transform.position = pos;
            }
        }

        private void SnapPosZ()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                Undo.RecordObject(go.transform, "Snap Position Z");
                Vector3 pos = go.transform.position;
                pos.z = Mathf.Round(pos.z / gridZ) * gridZ;
                go.transform.position = pos;
            }
        }

        private void SnapRotX()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                Undo.RecordObject(go.transform, "Snap Rotation X");
                Vector3 rot = go.transform.eulerAngles;
                rot.x = Mathf.Round(rot.x / rotX) * rotX;
                go.transform.eulerAngles = rot;
            }
        }

        private void SnapRotY()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                Undo.RecordObject(go.transform, "Snap Rotation Y");
                Vector3 rot = go.transform.eulerAngles;
                rot.y = Mathf.Round(rot.y / rotY) * rotY;
                go.transform.eulerAngles = rot;
            }
        }

        private void SnapRotZ()
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                Undo.RecordObject(go.transform, "Snap Rotation Z");
                Vector3 rot = go.transform.eulerAngles;
                rot.z = Mathf.Round(rot.z / rotZ) * rotZ;
                go.transform.eulerAngles = rot;
            }
        }
    }
}
