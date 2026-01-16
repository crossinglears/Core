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
            ParentSystem();
            ScatterChildren();
            Replacers();
        }
    }
}