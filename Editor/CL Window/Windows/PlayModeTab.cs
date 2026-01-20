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
            "Fast Mode"
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
                if (!Mathf.Approximately(newTimeScale, Time.timeScale))
                    Time.timeScale = newTimeScale;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EditorGUIUtility.labelWidth);

            float[] timeScaleValues = { 0f, 1f, 2f, 4f, 10f };
            string[] timeScaleLabels = { "0", "1", "2", "4", "10" };

            // Find nearest value
            int nearestIndex = 0;
            float minDiff = Mathf.Abs(Time.timeScale - timeScaleValues[0]);
            for (int i = 1; i < timeScaleValues.Length; i++)
            {
                float diff = Mathf.Abs(Time.timeScale - timeScaleValues[i]);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    nearestIndex = i;
                }
            }

            int newIndex = GUILayout.Toolbar(nearestIndex, timeScaleLabels);
            if (newIndex != nearestIndex)
                Time.timeScale = timeScaleValues[newIndex];

            EditorGUILayout.EndHorizontal();
        }



        GUIContent PlayModeEnterSettingLabel = new GUIContent(
            "Play Mode Reload Setting",
            "Controls what Unity reloads when entering Play Mode.\n\n" +
            "Default: Uses Unity default behavior.\n" +
            "Reload All: Reloads both Scene and Domain.\n" +
            "Reload Scene: Reloads Scene only.\n" +
            "Reload Domain: Reloads Domain only.\n" +
            "Fast Mode: Skips both for faster Play Mode entry."
        );
        
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

            selectedEnterPlayMode = EditorGUILayout.Popup(PlayModeEnterSettingLabel, selectedEnterPlayMode, enterPlayModeOption);

            if (EditorGUI.EndChangeCheck() && 0 <= selectedEnterPlayMode && selectedEnterPlayMode < enterPlayModeOption.Length)
            {
                EditorSettings.enterPlayModeOptionsEnabled = selectedEnterPlayMode != 0;
                EditorSettings.enterPlayModeOptions = (EnterPlayModeOptions)(selectedEnterPlayMode - 1);
            }
        #endif
        }
    }

}
