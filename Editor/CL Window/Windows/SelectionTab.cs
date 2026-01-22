using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public partial class SelectionTab : CL_WindowTab
    {
        public override string TabName => "Selection";

        public override void DrawContent()
        {
            SelectionBasic();

            GUILayout.Space(10);
            CL_GUILayout.HorizontalSeparator();

            ParentSystem();
            GUILayout.Space(10);
            CL_GUILayout.HorizontalSeparator();

            ScatterChildren();
            
            GUILayout.Space(10);
            CL_GUILayout.HorizontalSeparator();

            Replacers();
        }
    }
}