using UnityEngine;
using UnityEditor;

namespace CrossingLears.Editor
{
    public class MoveToTop
    {
        [MenuItem("CONTEXT/MonoBehaviour/MoveToTop")]
        private static void MoveComponentToTop(MenuCommand menuCommand)
        {
            MoveMonobehaviourToTop(menuCommand.context as MonoBehaviour);
        }

        public static void MoveMonobehaviourToTop(MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour != null)
            {
                while (UnityEditorInternal.ComponentUtility.MoveComponentUp(monoBehaviour)) ;
            }
        }
    }
}