using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    static class FastEnterPlayModeSupport
    {
        [InitializeOnEnterPlayMode]
        static void OnEnterPlayMode(EnterPlayModeOptions options)
        {
            if (CL_Window.current != null)
            {
                for (int i = 0; i < CL_Window.current.tabs.Count; i++)
                {
                    CL_Window.current.tabs[i].OnUnfocus();
                }
            }

            CL_Window.ResetStaticState();
            AutosaveTab.ResetStaticState();
            ClipboardEditWindow.ResetStaticState();
            LevelDesignTab.ResetStaticState();
            PackagesTab.ResetStaticState();
            EditorExtensions.ResetStaticState();
            VersioningCommands.ResetStaticState();
            CL_Design.ResetStaticState();
            CoreFeedbackSender.ResetStaticState();
        }
    }
}
