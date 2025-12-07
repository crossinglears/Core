using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public class ObjectSnappingTab : CL_WindowTab
    {        
        public override string TabName => "Object Snap";

        private enum SnapSpace { Global, Local }
        private SnapSpace snapSpace = SnapSpace.Global;

        private float gridX = 1f;
        private float gridY = 1f;
        private float gridZ = 1f;

        private float rotX = 15f;
        private float rotY = 15f;
        private float rotZ = 15f;

        private float scaleX = 1f;
        private float scaleY = 1f;
        private float scaleZ = 1f;

        public override void OnEnable()
        {
            base.OnEnable();

            snapSpace = (SnapSpace)EditorPrefs.GetInt("CL_ObjectSnapping_SnapSpace", 0);

            gridX = EditorPrefs.GetFloat("CL_ObjectSnapping_GridX", 1f);
            gridY = EditorPrefs.GetFloat("CL_ObjectSnapping_GridY", 1f);
            gridZ = EditorPrefs.GetFloat("CL_ObjectSnapping_GridZ", 1f);

            rotX = EditorPrefs.GetFloat("CL_ObjectSnapping_RotX", 15f);
            rotY = EditorPrefs.GetFloat("CL_ObjectSnapping_RotY", 15f);
            rotZ = EditorPrefs.GetFloat("CL_ObjectSnapping_RotZ", 15f);

            scaleX = EditorPrefs.GetFloat("CL_ObjectSnapping_ScaleX", 1f);
            scaleY = EditorPrefs.GetFloat("CL_ObjectSnapping_ScaleY", 1f);
            scaleZ = EditorPrefs.GetFloat("CL_ObjectSnapping_ScaleZ", 1f);
        }

        public override void OnDisable()
        {
            base.OnDisable();

            EditorPrefs.SetInt("CL_ObjectSnapping_SnapSpace", (int)snapSpace);

            EditorPrefs.SetFloat("CL_ObjectSnapping_GridX", gridX);
            EditorPrefs.SetFloat("CL_ObjectSnapping_GridY", gridY);
            EditorPrefs.SetFloat("CL_ObjectSnapping_GridZ", gridZ);

            EditorPrefs.SetFloat("CL_ObjectSnapping_RotX", rotX);
            EditorPrefs.SetFloat("CL_ObjectSnapping_RotY", rotY);
            EditorPrefs.SetFloat("CL_ObjectSnapping_RotZ", rotZ);

            EditorPrefs.SetFloat("CL_ObjectSnapping_ScaleX", scaleX);
            EditorPrefs.SetFloat("CL_ObjectSnapping_ScaleY", scaleY);
            EditorPrefs.SetFloat("CL_ObjectSnapping_ScaleZ", scaleZ);
        }


        public override void DrawTitle()
        {
            GUILayout.BeginHorizontal();
            base.DrawTitle();

            snapSpace = (SnapSpace)EditorGUILayout.EnumPopup(snapSpace, GUILayout.Width(90));

            GUILayout.EndHorizontal();
        }

        public override void DrawContent()
        {
            GUILayout.Label("Position", EditorStyles.boldLabel);
            if (GUILayout.Button("Snap Position", GUILayout.Height(25)))
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    Undo.RecordObject(go.transform, "Snap Position");

                    Vector3 pos = snapSpace == SnapSpace.Local ? go.transform.localPosition : go.transform.position;

                    if (gridX != 0) pos.x = Mathf.Round(pos.x / gridX) * gridX;
                    if (gridY != 0) pos.y = Mathf.Round(pos.y / gridY) * gridY;
                    if (gridZ != 0) pos.z = Mathf.Round(pos.z / gridZ) * gridZ;

                    if (snapSpace == SnapSpace.Local) go.transform.localPosition = pos;
                    else go.transform.position = pos;
                }
            }

            GUILayout.Space(5);
            DrawSnapRow("X", ref gridX, SnapPosX, Handles.xAxisColor);
            DrawSnapRow("Y", ref gridY, SnapPosY, Handles.yAxisColor);
            DrawSnapRow("Z", ref gridZ, SnapPosZ, Handles.zAxisColor);

            GUILayout.Space(15);
            GUILayout.Label("Rotation", EditorStyles.boldLabel);
            if (GUILayout.Button("Snap Rotation", GUILayout.Height(25)))
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    Undo.RecordObject(go.transform, "Snap Rotation");

                    Vector3 rot = snapSpace == SnapSpace.Local ? go.transform.localEulerAngles : go.transform.eulerAngles;

                    if (rotX != 0) rot.x = Mathf.Round(rot.x / rotX) * rotX;
                    if (rotY != 0) rot.y = Mathf.Round(rot.y / rotY) * rotY;
                    if (rotZ != 0) rot.z = Mathf.Round(rot.z / rotZ) * rotZ;

                    if (snapSpace == SnapSpace.Local) go.transform.localEulerAngles = rot;
                    else go.transform.eulerAngles = rot;
                }
            }


            GUILayout.Space(5);
            DrawSnapRow("X", ref rotX, SnapRotX);
            DrawSnapRow("Y", ref rotY, SnapRotY);
            DrawSnapRow("Z", ref rotZ, SnapRotZ);

            GUILayout.Space(15);
            GUILayout.Label("Scale", EditorStyles.boldLabel);
            if (GUILayout.Button("Snap Scale", GUILayout.Height(25)))
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    Undo.RecordObject(go.transform, "Snap Scale");
                    Vector3 scale = go.transform.localScale;
                    if (scaleX != 0) scale.x = Mathf.Round(scale.x / scaleX) * scaleX;
                    if (scaleY != 0) scale.y = Mathf.Round(scale.y / scaleY) * scaleY;
                    if (scaleZ != 0) scale.z = Mathf.Round(scale.z / scaleZ) * scaleZ;
                    go.transform.localScale = scale;
                }
            }

            GUILayout.Space(5);
            DrawSnapRow("X", ref scaleX, SnapScaleX);
            DrawSnapRow("Y", ref scaleY, SnapScaleY);
            DrawSnapRow("Z", ref scaleZ, SnapScaleZ);
        }

        private void DrawSnapRow(string label, ref float value, System.Action onSnap)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(20));
            value = EditorGUILayout.FloatField(value);
            if (GUILayout.Button($"Snap {label}", GUILayout.Width(100)))
                onSnap.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawSnapRow(string label, ref float value, System.Action onSnap, Color color)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(20));
            value = EditorGUILayout.FloatField(value);
            
            GUI.color = color;
            if (GUILayout.Button($"Snap {label}", GUILayout.Width(100)))
                onSnap.Invoke();

            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }

private void SnapPosX()
{
    foreach (GameObject go in Selection.gameObjects)
    {
        Undo.RecordObject(go.transform, "Snap Position X");
        Vector3 pos = snapSpace == SnapSpace.Local ? go.transform.localPosition : go.transform.position;
        if (gridX != 0) pos.x = Mathf.Round(pos.x / gridX) * gridX;

        if (snapSpace == SnapSpace.Local) go.transform.localPosition = pos;
        else go.transform.position = pos;
    }
}

private void SnapPosY()
{
    foreach (GameObject go in Selection.gameObjects)
    {
        Undo.RecordObject(go.transform, "Snap Position Y");
        Vector3 pos = snapSpace == SnapSpace.Local ? go.transform.localPosition : go.transform.position;
        if (gridY != 0) pos.y = Mathf.Round(pos.y / gridY) * gridY;

        if (snapSpace == SnapSpace.Local) go.transform.localPosition = pos;
        else go.transform.position = pos;
    }
}

private void SnapPosZ()
{
    foreach (GameObject go in Selection.gameObjects)
    {
        Undo.RecordObject(go.transform, "Snap Position Z");
        Vector3 pos = snapSpace == SnapSpace.Local ? go.transform.localPosition : go.transform.position;
        if (gridZ != 0) pos.z = Mathf.Round(pos.z / gridZ) * gridZ;

        if (snapSpace == SnapSpace.Local) go.transform.localPosition = pos;
        else go.transform.position = pos;
    }
}

private void SnapRotX()
{
    foreach (GameObject go in Selection.gameObjects)
    {
        Undo.RecordObject(go.transform, "Snap Rotation X");
        Vector3 rot = snapSpace == SnapSpace.Local ? go.transform.localEulerAngles : go.transform.eulerAngles;
        if (rotX != 0) rot.x = Mathf.Round(rot.x / rotX) * rotX;

        if (snapSpace == SnapSpace.Local) go.transform.localEulerAngles = rot;
        else go.transform.eulerAngles = rot;
    }
}

private void SnapRotY()
{
    foreach (GameObject go in Selection.gameObjects)
    {
        Undo.RecordObject(go.transform, "Snap Rotation Y");
        Vector3 rot = snapSpace == SnapSpace.Local ? go.transform.localEulerAngles : go.transform.eulerAngles;
        if (rotY != 0) rot.y = Mathf.Round(rot.y / rotY) * rotY;

        if (snapSpace == SnapSpace.Local) go.transform.localEulerAngles = rot;
        else go.transform.eulerAngles = rot;
    }
}

private void SnapRotZ()
{
    foreach (GameObject go in Selection.gameObjects)
    {
        Undo.RecordObject(go.transform, "Snap Rotation Z");
        Vector3 rot = snapSpace == SnapSpace.Local ? go.transform.localEulerAngles : go.transform.eulerAngles;
        if (rotZ != 0) rot.z = Mathf.Round(rot.z / rotZ) * rotZ;

        if (snapSpace == SnapSpace.Local) go.transform.localEulerAngles = rot;
        else go.transform.eulerAngles = rot;
    }
}

private void SnapScaleX()
{
    foreach (GameObject go in Selection.gameObjects)
    {
        Undo.RecordObject(go.transform, "Snap Scale X");
        Vector3 scale = go.transform.localScale;
        if (scaleX != 0) scale.x = Mathf.Round(scale.x / scaleX) * scaleX;
        go.transform.localScale = scale;
    }
}

private void SnapScaleY()
{
    foreach (GameObject go in Selection.gameObjects)
    {
        Undo.RecordObject(go.transform, "Snap Scale Y");
        Vector3 scale = go.transform.localScale;
        if (scaleY != 0) scale.y = Mathf.Round(scale.y / scaleY) * scaleY;
        go.transform.localScale = scale;
    }
}

private void SnapScaleZ()
{
    foreach (GameObject go in Selection.gameObjects)
    {
        Undo.RecordObject(go.transform, "Snap Scale Z");
        Vector3 scale = go.transform.localScale;
        if (scaleZ != 0) scale.z = Mathf.Round(scale.z / scaleZ) * scaleZ;
        go.transform.localScale = scale;
    }
}

    }
}
