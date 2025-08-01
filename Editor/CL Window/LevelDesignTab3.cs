using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public partial class LevelDesignTab : CL_WindowTab_WIP
    {
        void InformationArea()
        {
            Transform[] selectedTransforms = Selection.transforms;

            EditorGUILayout.LabelField("Selected Object Info", EditorStyles.boldLabel);

            if (selectedTransforms.Length > 0)
            {
                Transform reference = selectedTransforms[0];

                EditorGUILayout.LabelField("Selected Objects: " + selectedTransforms.Length);
                // EditorGUILayout.LabelField("Reference Name:", reference.name);

                Object newReference = EditorGUILayout.ObjectField("Reference Object", reference.gameObject, typeof(GameObject), true);
                if (newReference != null && newReference != reference.gameObject)
                {
                    Selection.activeGameObject = (GameObject)newReference;
                    selectedTransforms = Selection.transforms;
                    reference = selectedTransforms[0];
                }

                Vector3 oldWorld = reference.position;
                Vector3 newWorld = EditorGUILayout.Vector3Field("World Position", oldWorld);

                Vector3 oldLocal = reference.localPosition;
                Vector3 newLocal = EditorGUILayout.Vector3Field("Local Position", oldLocal);

                if (newWorld != oldWorld || newLocal != oldLocal)
                {
                    foreach (Transform t in selectedTransforms)
                    {
                        Undo.RecordObject(t, "Change Transform Position");

                        Vector3 wp = t.position;
                        Vector3 lp = t.localPosition;

                        if (newWorld != oldWorld)
                        {
                            if (newWorld.x != oldWorld.x) wp.x = newWorld.x;
                            if (newWorld.y != oldWorld.y) wp.y = newWorld.y;
                            if (newWorld.z != oldWorld.z) wp.z = newWorld.z;
                            t.position = wp;
                        }
                        else if (newLocal != oldLocal)
                        {
                            if (newLocal.x != oldLocal.x) lp.x = newLocal.x;
                            if (newLocal.y != oldLocal.y) lp.y = newLocal.y;
                            if (newLocal.z != oldLocal.z) lp.z = newLocal.z;
                            t.localPosition = lp;
                        }

                        EditorUtility.SetDirty(t);
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No object selected.");
            }
        }
    }
}
