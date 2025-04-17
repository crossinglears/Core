using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace CrossingLearsEditor
{
    public class GeneralTab : CL_WindowTab
    {
        public override string TabName => "General";
        public override int Order => -1000;

        private AddRequest addRequest;
        private string feedbackMessage = "";
        private float nextSendTime = 0f;
        private const float cooldownDuration = 5f;
        private Vector2 scrollPos;
        private const int maxVisibleLines = 5;

        public override void DrawTitle()
        {
            base.DrawTitle();
            InstallCorePackageButton();
        }

        public override void DrawContent()
        {
            // InstallCorePackageButton();

            GUILayout.Space(10);
            TabsChecklist();

            GUILayout.Space(10);
            WebsiteAndFeedbackSection();
            GUILayout.Space(40);
        }

        private void WebsiteAndFeedbackSection()
        {
            GUILayout.Label("Thank you for supporting this tool\nFor inquiries, feedback, or suggestions:");

            if (GUILayout.Button("Visit Website"))
            {
                if (EditorUtility.DisplayDialog("Open URL", "Open \"https://crossinglears.carrd.co/\" with your browser?", "Yes", "No"))
                {
                    Application.OpenURL("https://crossinglears.carrd.co/");
                }
            }

            GUILayout.Label("Send Feedback:");

            int lineCount = feedbackMessage.Split('\n').Length + 1;
            int clampedLines = Mathf.Clamp(lineCount, 1, maxVisibleLines);
            float fieldHeight = clampedLines * 20f;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(fieldHeight + 10));
            feedbackMessage = EditorGUILayout.TextArea(feedbackMessage, new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            }, GUILayout.MinHeight(fieldHeight));
            EditorGUILayout.EndScrollView();

            GUI.enabled = Time.realtimeSinceStartup >= nextSendTime;
            if (GUILayout.Button("Send Feedback", GUILayout.Height(25)))
            {
                if (!string.IsNullOrWhiteSpace(feedbackMessage))
                {
                    CoreFeedbackSender.SendFeedback(feedbackMessage);
                    feedbackMessage = "";
                    nextSendTime = Time.realtimeSinceStartup + cooldownDuration;
                }
                else
                {
                    Debug.LogWarning("Feedback message is empty!");
                    nextSendTime = Time.realtimeSinceStartup + cooldownDuration;
                }
            }
            GUI.enabled = true;

            if (Time.realtimeSinceStartup < nextSendTime)
            {
                GUILayout.Label($"Please wait {Mathf.CeilToInt(nextSendTime - Time.realtimeSinceStartup)}s before resending.", EditorStyles.helpBox);
            }
        }

        private void InstallCorePackageButton()
        {
            if (GUILayout.Button(new GUIContent("Update Core", "Installs or updates https://github.com/crossinglears/Core.git#main"), GUILayout.Height(30)))
            {
                addRequest = Client.Add("https://github.com/crossinglears/Core.git#main");
                EditorApplication.update += PackageProgress;
                
                void PackageProgress()
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

        private void TabsChecklist()
        {
            CL_Window cL_Window = CL_Window.current;
            GUILayout.Label("Menus to open", EditorStyles.boldLabel);

            for (int i = 1; i < cL_Window.tabs.Count; i++)
            {
                CL_WindowTab tab = cL_Window.tabs[i];
                bool isActive = !cL_Window.IgnoredTabs.Contains(tab.TabName); // Check if the tab is ignored

                GUILayout.BeginHorizontal();
                bool newIsActive = EditorGUILayout.Toggle(isActive, GUILayout.Width(20));
                
                if (newIsActive != isActive)
                {
                    if (newIsActive)
                    {
                        cL_Window.IgnoredTabs.Remove(tab.TabName);
                    }
                    else
                    {
                        cL_Window.IgnoredTabs.Add(tab.TabName);
                    }
                }
                GUILayout.Label(tab.TabName);
                GUILayout.EndHorizontal();
            }
        }

    }
}