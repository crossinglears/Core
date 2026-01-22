using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public partial class LevelDesignTab : CL_WindowTab_WIP
    {
        public static Vector3 itempos1;
        public static Vector3 itempos2;
        public static Vector3 itempos3;
        public static Vector3 rotationEuler;

        void RelaterMenu()
        {
            GUILayout.BeginHorizontal();
            B1();
            B2();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            B3();
            B4();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Rotation");
            rotationEuler = EditorGUILayout.Vector3Field("", rotationEuler);
            GUILayout.EndHorizontal();
        }

        void B1()
        {
            if (GUILayout.Button("B1"))
                itempos1 = Selection.activeGameObject.transform.position;
        }

        void B2()
        {
            if (GUILayout.Button("B2"))
                itempos2 = Selection.activeGameObject.transform.position;
        }

        void B3()
        {
            if (GUILayout.Button("B3"))
                itempos3 = Selection.activeGameObject.transform.position;
        }

        void B4()
        {
            if (GUILayout.Button("B4"))
            {
                GameObject[] selectedObjects = Selection.gameObjects;
                if (selectedObjects.Length == 0) return;

                GameObject mainObject = Selection.activeGameObject;
                if (mainObject == null) return;

                Vector3 offset = itempos3 - itempos1 + itempos2;
                Vector3 pivot = itempos3;

                Vector3 direction = offset - pivot;
                Quaternion rotation = Quaternion.Euler(rotationEuler);
                Vector3 rotatedDirection = rotation * direction;
                Vector3 mainFinalPos = pivot + rotatedDirection;

                // Record Undo for all selected objects
                foreach (GameObject go in selectedObjects)
                {
                    Undo.RecordObject(go.transform, "Move with Rotation");
                }

                Vector3 mainOriginalPos = mainObject.transform.position;
                mainObject.transform.position = mainFinalPos;

                foreach (GameObject go in selectedObjects)
                {
                    if (go == mainObject) continue;

                    Vector3 relativeOffset = go.transform.position - mainOriginalPos;
                    Vector3 rotatedOffset = rotation * relativeOffset;
                    go.transform.position = mainFinalPos + rotatedOffset;
                }
            }
        }
    }
}
