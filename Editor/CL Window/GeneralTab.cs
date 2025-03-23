using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace CrossingLears
{
    public class GeneralTab : CL_WindowTab
    {
        private AddRequest addRequest;

        public override string TabName => "General";

        public override void DrawContent()
        {
            GUILayout.Label("General Settings");
            if (GUILayout.Button("Update Core", GUILayout.Height(30)))
            {
                InstallCorePackage();
            }
        }

        private void InstallCorePackage()
        {
            addRequest = Client.Add("https://github.com/crossinglears/Core.git#main");
            EditorApplication.update += PackageProgress;
        }

        private void PackageProgress()
        {
            if (addRequest.IsCompleted)
            {
                if (addRequest.Status == StatusCode.Success)
                {
                    Debug.Log("Crossing Lears Core updated successfully!");
                }
                else
                {
                    Debug.LogError("Failed to install Core package: " + addRequest.Error.message);
                }
                EditorApplication.update -= PackageProgress;
            }
        }
    }

}
