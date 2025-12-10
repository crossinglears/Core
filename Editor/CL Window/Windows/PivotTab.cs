using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public class PivotTab : CL_WindowTab
    {
        public enum PivotSetting
        {
            AverageBoundsCenter,
            AveragePosition,
        }

        public override string TabName => "Pivot";

        int x = 1;
        int y = 1;
        int z = 1;
        PivotSetting pivotSetting = PivotSetting.AverageBoundsCenter;

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
            pivotSetting = (PivotSetting)EditorGUILayout.EnumPopup(pivotSetting);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            if (GUILayout.Button("Apply Pivot", GUILayout.Height(25)))
            {
                ApplyPivot(false);
            }

            if (GUILayout.Button("Apply Pivot (Include All Descendants)", GUILayout.Height(25)))
            {
                ApplyPivot(true);
            }
        }

        void ApplyPivot(bool includeDescendants)
        {
            Transform target = Selection.activeTransform;
            if (target == null) return;

            List<Transform> moveChildren = new List<Transform>();
            List<Transform> calcChildren = new List<Transform>();

            if (includeDescendants)
            {
                Transform[] all = target.GetComponentsInChildren<Transform>();
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] != target) calcChildren.Add(all[i]);
                }

                for (int i = 0; i < target.childCount; i++)
                {
                    moveChildren.Add(target.GetChild(i));
                }
            }
            else
            {
                for (int i = 0; i < target.childCount; i++)
                {
                    Transform c = target.GetChild(i);
                    moveChildren.Add(c);
                    calcChildren.Add(c);
                }
            }

            if (calcChildren.Count == 0) return;

            Vector3 desired = Vector3.zero;

            if (pivotSetting == PivotSetting.AveragePosition)
            {
                Vector3 sum = Vector3.zero;
                for (int i = 0; i < calcChildren.Count; i++)
                {
                    sum += calcChildren[i].position;
                }
                desired = sum / calcChildren.Count;
            }
            else
            {
                Bounds bounds = new Bounds();
                bool set = false;

                for (int i = 0; i < calcChildren.Count; i++)
                {
                    Transform child = calcChildren[i];
                    Renderer r = child.GetComponent<Renderer>();
                    Collider c = child.GetComponent<Collider>();

                    if (r != null)
                    {
                        if (!set) { bounds = new Bounds(r.bounds.center, r.bounds.size); set = true; }
                        else { bounds.Encapsulate(r.bounds); }
                    }
                    else if (c != null)
                    {
                        if (!set) { bounds = new Bounds(c.bounds.center, c.bounds.size); set = true; }
                        else { bounds.Encapsulate(c.bounds); }
                    }
                    else
                    {
                        if (!set) { bounds = new Bounds(child.position, Vector3.zero); set = true; }
                        else { bounds.Encapsulate(child.position); }
                    }
                }

                desired = bounds.center;
            }

            Bounds finalBounds = new Bounds(desired, Vector3.zero);
            for (int i = 0; i < calcChildren.Count; i++)
            {
                Transform child = calcChildren[i];
                Renderer r = child.GetComponent<Renderer>();
                Collider c = child.GetComponent<Collider>();

                if (r != null)
                {
                    finalBounds.Encapsulate(r.bounds);
                }
                else if (c != null)
                {
                    finalBounds.Encapsulate(c.bounds);
                }
                else
                {
                    finalBounds.Encapsulate(child.position);
                }
            }

            float selectX = x == 0 ? finalBounds.min.x : (x == 1 ? finalBounds.center.x : finalBounds.max.x);
            float selectY = y == 0 ? finalBounds.max.y : (y == 1 ? finalBounds.center.y : finalBounds.min.y);
            float selectZ = z == 0 ? finalBounds.min.z : (z == 1 ? finalBounds.center.z : finalBounds.max.z);

            desired = new Vector3(selectX, selectY, selectZ);

            Vector3 offset = target.position - desired;

            for (int i = 0; i < moveChildren.Count; i++)
            {
                moveChildren[i].position += offset;
            }

            target.position = desired;
        }
    }
}
