using UnityEditor;
using UnityEngine;

namespace CrossingLearsEditor
{
    public partial class LevelDesignTab : CL_WindowTab_WIP
    {
        public static Vector3 itempos1;
        public static Vector3 itempos2;
        public static Vector3 itempos3;

        void RelaterMenu()
        {
            GUILayout.BeginHorizontal();
            B1();
            B2();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            B3();
            B4();
            GUILayout.EndHorizontal();
        }

        void B1()
        {
            if(GUILayout.Button("B1"))
            itempos1 = Selection.activeGameObject.transform.position;
        }

        void B2()
        {
            if(GUILayout.Button("B2"))
            itempos2 = Selection.activeGameObject.transform.position;
        }

        void B3()
        {
            if(GUILayout.Button("B3"))
            itempos3 = Selection.activeGameObject.transform.position;
        }
        
        void B4()
        {
            if(GUILayout.Button("B4"))
            {
                // itempos3 - itempos1 + itempos2;
                Vector3 pos = itempos3 - itempos1 + itempos2;
                Selection.activeGameObject.transform.position = pos;
            }
        }
    }
}
