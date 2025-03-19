using UnityEngine;
using UnityEditor;

namespace CrossingLears
{
    public class MoveToTop
    {
        [MenuItem("CONTEXT/MonoBehaviour/MoveToTop")]
        private static void MoveComponentToTop(MenuCommand menuCommand)
        {
            MonoBehaviour monoBehaviour = menuCommand.context as MonoBehaviour;
            if (monoBehaviour != null)
            {
                while(UnityEditorInternal.ComponentUtility.MoveComponentUp(monoBehaviour));
            }
        }
    }
}