using UnityEngine;

namespace CrossingLears
{
    public static class EditorUtilities
    {
        public static void SetDirty(Object target)
        {
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(target);
            #endif
        }
    }
}
