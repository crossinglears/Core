using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public class TransformTab : CL_WindowTab
    {
        public override string TabName => "Transform";

        public override void DrawContent()
        {
            if(GUILayout.Button("No Negative Scale"))
            {
                foreach (Transform item in Selection.transforms)
                {
                    Vector3 localScale = item.localScale;
                    if (localScale.x < 0f || localScale.y < 0f || localScale.z < 0f)
                    {
                        localScale.x = Mathf.Abs(localScale.x);
                        localScale.y = Mathf.Abs(localScale.y);
                        localScale.z = Mathf.Abs(localScale.z);
                        item.localScale = localScale;

                        EditorUtility.SetDirty(item);
                    }
                }
            }
        }
    }
}
