using UnityEditor;

namespace CrossingLearsEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class CL_Editor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            this.DrawButtons();
            DrawDefaultInspector();
        }
    }
}
