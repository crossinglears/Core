using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CrossingLears.Editor
{
    public class TextEditorTab : CL_WindowTab
    {
        public override string TabName => "Text Edit";

        public Text text;
        public TMP_Text tmp_text;

        bool alsoChangeSelectedObjectName;

        public override void DrawContent()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                CL_Window.current.Repaint();
                return;
            }

            Text foundText = selected.GetComponentInChildren<Text>(true);
            TMP_Text foundTMP = selected.GetComponentInChildren<TMP_Text>(true);
            if (foundText == null && foundTMP == null)
            {
                CL_Window.current.Repaint();
                return;
            }


            alsoChangeSelectedObjectName = GUILayout.Toggle(alsoChangeSelectedObjectName, "Also Change Selected Object Name");
            GUILayout.Space(10);
            string currentValue = foundText != null ? foundText.text : foundTMP.text;

            EditorGUI.BeginChangeCheck();
            string newValue = EditorGUILayout.TextArea(currentValue, GUILayout.Height(80f));
            if (EditorGUI.EndChangeCheck())
            {
                if (foundText != null)
                {
                    foundText.text = newValue;
                    EditorUtility.SetDirty(foundText);
                }
                if (foundTMP != null)
                {
                    foundTMP.text = newValue;
                    EditorUtility.SetDirty(foundTMP);
                }
                if (alsoChangeSelectedObjectName)
                {
                    selected.name = newValue;
                    EditorUtility.SetDirty(selected);
                }
            }
            CL_Window.current.Repaint();
        }

    }
}
