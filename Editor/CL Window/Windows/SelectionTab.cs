using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public partial class SelectionTab : CL_WindowTab
    {
        public override string TabName => "Selection";

        public override void DrawContent()
        {
            SelectionBasic();
            CL_GUILayout.HorizontalSeparator();

            ParentSystem();
            CL_GUILayout.HorizontalSeparator();

            ScatterChildren();
            CL_GUILayout.HorizontalSeparator();
            
            Replacers();
        }
    }
}