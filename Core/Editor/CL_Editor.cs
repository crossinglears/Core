using UnityEditor;

namespace CrossingLears
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class CL_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            this.DrawButtons();
            DrawDefaultInspector();
        }
    }
}
