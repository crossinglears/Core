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

                Vector3 oldGlobalScale = reference.lossyScale;
                Vector3 newGlobalScale = EditorGUILayout.Vector3Field("Global Scale", oldGlobalScale);

                Vector3 oldLocalScale = reference.localScale;
                Vector3 newLocalScale = EditorGUILayout.Vector3Field("Local Scale", oldLocalScale);

                if (newWorld != oldWorld || newLocal != oldLocal || newGlobalScale != oldGlobalScale || newLocalScale != oldLocalScale)
                {
                    foreach (Transform t in selectedTransforms)
                    {
                        Undo.RecordObject(t, "Change Transform");

                        Vector3 wp = t.position;
                        Vector3 lp = t.localPosition;
                        Vector3 gls = t.lossyScale;
                        Vector3 lcs = t.localScale;

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
                        else if (newLocalScale != oldLocalScale)
                        {
                            if (newLocalScale.x != oldLocalScale.x) lcs.x = newLocalScale.x;
                            if (newLocalScale.y != oldLocalScale.y) lcs.y = newLocalScale.y;
                            if (newLocalScale.z != oldLocalScale.z) lcs.z = newLocalScale.z;
                            t.localScale = lcs;
                        }
                        else if (newGlobalScale != oldGlobalScale)
                        {
                            Vector3 parentScale = t.parent != null ? t.parent.lossyScale : Vector3.one;
                            Vector3 adjustedScale = new Vector3(
                                parentScale.x != 0 ? newGlobalScale.x / parentScale.x : newGlobalScale.x,
                                parentScale.y != 0 ? newGlobalScale.y / parentScale.y : newGlobalScale.y,
                                parentScale.z != 0 ? newGlobalScale.z / parentScale.z : newGlobalScale.z
                            );
                            t.localScale = adjustedScale;
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
