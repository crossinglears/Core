using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEditorInternal;

namespace CrossingLears.Editor
{
    public class GeneralTab : CL_WindowTab
    {
        public override string TabName => "General";
        public string WebsiteLink => "https://crossinglears.com/";

        private AddRequest addRequest;
        private string feedbackMessage = "";
        private float nextSendTime = 0f;
        private const float cooldownDuration = 5f;
        private Vector2 scrollPos;
        private const int maxVisibleLines = 5;

        private ReorderableList tabList;

        public override void DrawTitle()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(TabName, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            InstallCorePackageButton();
        }

        public override void DrawContent()
        {
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
                if (EditorUtility.DisplayDialog("Open URL", $"Open \"{WebsiteLink}\" with your browser?", "Yes", "No"))
                {
                    Application.OpenURL(WebsiteLink);
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
            if (GUILayout.Button(new GUIContent("Update Toolbox", "Installs or updates https://github.com/crossinglears/Core.git#main"), GUILayout.Height(30)))
            {
                addRequest = Client.Add("https://github.com/crossinglears/Core.git#main");
                EditorApplication.update += PackageProgress;

                void PackageProgress()
                {
                    if (addRequest.IsCompleted)
                    {
                        if (addRequest.Status == StatusCode.Success)
                        {
                            Debug.Log("Crossing Lears Toolbox updated successfully!");
                        }
                        else
                        {
                            Debug.LogError("Failed to install Toolbox package: " + addRequest.Error.message);
                        }
                        EditorApplication.update -= PackageProgress;
                    }
                }
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            InitTabList();
        }

        private void TabsChecklist()
        {
            GUILayout.Label("Menus to open", EditorStyles.boldLabel);
            if(tabList != null)
            {
                tabList.DoLayoutList();
            }
        }
        
        private void InitTabList()
        {
            CL_Window cL_Window = CL_Window.current;
            if (tabList != null) return;
            
            System.Collections.Generic.List<CL_WindowTab> otherTabs = cL_Window.tabs.FindAll(tab => !(tab is GeneralTab));

            tabList = new ReorderableList(otherTabs, typeof(CL_WindowTab), true, false, false, false);

            // Draw each element
            tabList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                CL_WindowTab tab = otherTabs[index];
                bool active = !cL_Window.IgnoredTabs.Contains(tab.TabName);

                Rect toggleRect = new Rect(rect.x, rect.y, 20, rect.height);
                Rect labelRect = new Rect(rect.x + 25, rect.y, rect.width - 25, rect.height);

                bool newActive = EditorGUI.Toggle(toggleRect, active);
                if (newActive != active)
                {
                    if (newActive) cL_Window.IgnoredTabs.Remove(tab.TabName);
                    else cL_Window.IgnoredTabs.Add(tab.TabName);
                }

                EditorGUI.LabelField(labelRect, tab.TabName);
            };

            tabList.onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
            {
                CL_WindowTab movedTab = cL_Window.tabs[oldIndex + 1]; // +1 to skip GeneralTab at index 0
                cL_Window.tabs.RemoveAt(oldIndex + 1);
                cL_Window.tabs.Insert(newIndex + 1, movedTab);
            };
        }

    }
}
