using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CrossingLearsEditor
{
    public class CL_EditorEvents : MonoBehaviour
    {
        [InitializeOnLoadMethod]
        static void OnScriptReloaded()
        {
            Debug.Log("Crossing Lears: Script Reloaded!");
        }
    }
    
    public class CrossingLearsBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report) 
        {
            Debug.Log("Crossing Lears: Build");
        }
    }
}
