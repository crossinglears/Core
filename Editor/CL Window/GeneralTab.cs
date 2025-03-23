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
            
            if (GUILayout.Button(new GUIContent("Update Core", "Installs or update https://github.com/crossinglears/Core.git#main"), GUILayout.Height(30)))
            {
                InstallCorePackage();
            }

            GUILayout.Space(30);            
            GUILayout.Label("Thank you for supporting this tool\nFor inquiries, feedback or suggestions:");
            if(GUILayout.Button("Visit Website"))
            {
                if (EditorUtility.DisplayDialog("Open url", 
                    "Open \"https://crossinglears.carrd.co/\" with your browser?", "Yes", "No"))
                {
                    Application.OpenURL("https://crossinglears.carrd.co/");
                }
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
