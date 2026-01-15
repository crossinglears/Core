using UnityEngine;
using UnityEditor;

namespace CrossingLearsEditor
{
    public class PlayModeTab : CL_WindowTab
    {
        public override string TabName => "Play Mode";

#if UNITY_2019_3_OR_NEWER
        private int selectedEnterPlayMode;
        private readonly string[] enterPlayModeOption =
        {
            "Default",
            "Reload All",
            "Reload Scene",
            "Reload Domain",
            "FastMode"
        };
#endif

        private int fps = 60;
        private readonly int minFPS = 1;
        private readonly int maxFPS = 120;

        public override void DrawContent()
        {
            PlayModeEnterSetting();
            TimeAndFPS();
        }

        void TimeAndFPS()
        {
            EditorGUI.BeginChangeCheck();
            fps = EditorGUILayout.IntSlider("FPS Limit", fps, minFPS, maxFPS);
            if (EditorGUI.EndChangeCheck())
            {
                Application.targetFrameRate = fps;
            }

            EditorGUI.BeginChangeCheck();
            float newTimeScale = EditorGUILayout.Slider("Time Speed", Time.timeScale, 0f, 10f);
            if (EditorGUI.EndChangeCheck())
            {
                if (!Mathf.Approximately(newTimeScale, Time.timeScale)) {
                    // Only update Time.timeScale if the user changes the slider
                    Time.timeScale = newTimeScale;
                }
            }
        }

        void PlayModeEnterSetting()
        {
#if UNITY_2019_3_OR_NEWER
            if (EditorSettings.enterPlayModeOptionsEnabled)
            {
                EnterPlayModeOptions option = EditorSettings.enterPlayModeOptions;
                selectedEnterPlayMode = (int)option + 1;
            }
            else
            {
                selectedEnterPlayMode = 0;
            }

            EditorGUI.BeginChangeCheck();
            selectedEnterPlayMode = EditorGUILayout.Popup("Play Mode Reload Setting", selectedEnterPlayMode, enterPlayModeOption);
            if (EditorGUI.EndChangeCheck() && 0 <= selectedEnterPlayMode && selectedEnterPlayMode < enterPlayModeOption.Length)
            {
                EditorSettings.enterPlayModeOptionsEnabled = selectedEnterPlayMode != 0;
                EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)(selectedEnterPlayMode - 1);
            }
#endif
        }
    }

}
