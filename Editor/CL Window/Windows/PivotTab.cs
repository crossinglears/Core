// using UnityEngine;

// namespace CrossingLearsEditor
// {
//     public class PivotTab : CL_WindowTab
//     {
//         public override string TabName => "Pivot";

//         int x, y ,z;

//         public override void DrawContent()
//         {
//             string[] par = new string[] { "Left", "Center", "Right" };
//             string[] par2 = new string[] { "Top", "Center", "Bottom" };
            
//             GUILayout.Toolbar(x, par, GUILayout.Width(120), GUILayout.Height(20));
//             GUILayout.Toolbar(y, par2, GUILayout.Width(120), GUILayout.Height(20));
//             GUILayout.Toolbar(z, par, GUILayout.Width(120), GUILayout.Height(20));
            
//             int pivotSetting = 0;
//             GUILayout.Toolbar(pivotSetting, new string[] {"Average Position of All Children, Average Bounds Center of All Children"}, GUILayout.Width(120), GUILayout.Height(20));

//             if(GUILayout.Button("Apply Pivot"))
//             {
                
//             }
//         }
//     }
// }


using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public class PivotTab : CL_WindowTab
    {
        public override string TabName => "Pivot";

        int x = 1;
        int y = 1;
        int z = 1;
        int pivotSetting = 0;

        public override void DrawContent()
        {
            string[] par = new string[] { "Left", "Center", "Right" };
            string[] par2 = new string[] { "Top", "Center", "Bottom" };

            GUILayout.BeginHorizontal();
            GUILayout.Label("X", GUILayout.Width(20));
            x = GUILayout.Toolbar(x, par, GUILayout.Height(20), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Y", GUILayout.Width(20));
            y = GUILayout.Toolbar(y, par2, GUILayout.Height(20), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Z", GUILayout.Width(20));
            z = GUILayout.Toolbar(z, par, GUILayout.Height(20), GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
                        
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pivot Mode", GUILayout.Width(120));
            pivotSetting = EditorGUILayout.Popup(pivotSetting, new string[] { "Average Bounds Center of All Children", "Average Position of All Children",  });
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            if (GUILayout.Button("Apply Pivot", GUILayout.Height(25)))
            {
                ApplyPivot();
            }
        }

        void ApplyPivot()
        {
            Transform target = Selection.activeTransform;
            if (target == null) return;

            List<Transform> children = new List<Transform>();
            for (int i = 0; i < target.childCount; i++)
            {
                children.Add(target.GetChild(i));
            }
            if (children.Count == 0) return;

            Vector3 desired;

            if (pivotSetting == 0) // Average Position
            {
                Vector3 sum = Vector3.zero;
                foreach (Transform child in children)
                {
                    sum += child.position;
                }
                desired = sum / children.Count;
            }
            else // Average Bounds Center
            {
                Bounds bounds = new Bounds(children[0].position, Vector3.zero);
                foreach (Transform child in children)
                {
                    Renderer r = child.GetComponent<Renderer>();
                    if (r != null)
                    {
                        bounds.Encapsulate(r.bounds);
                    }
                    else
                    {
                        bounds.Encapsulate(child.position);
                    }
                }
                desired = bounds.center;
            }

            // Adjust pivot according to x, y, z selection
            Bounds finalBounds = new Bounds(desired, Vector3.zero);
            foreach (Transform child in children)
            {
                Renderer r = child.GetComponent<Renderer>();
                if (r != null)
                    finalBounds.Encapsulate(r.bounds);
                else
                    finalBounds.Encapsulate(child.position);
            }

            float selectX = x == 0 ? finalBounds.min.x : (x == 1 ? finalBounds.center.x : finalBounds.max.x);
            float selectY = y == 0 ? finalBounds.max.y : (y == 1 ? finalBounds.center.y : finalBounds.min.y);
            float selectZ = z == 0 ? finalBounds.min.z : (z == 1 ? finalBounds.center.z : finalBounds.max.z);

            desired = new Vector3(selectX, selectY, selectZ);

            Vector3 offset = target.position - desired;

            foreach (Transform child in children)
            {
                child.position += offset;
            }

            target.position = desired;
        }
    }
}
