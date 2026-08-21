using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_6000_0_OR_NEWER
using UnityEditor.SceneManagement;
#endif

namespace CrossingLears.Editor
{
    public class TextEditorTab : CL_WindowTab
    {
        public override string TabName => "Text Edit";

        public Text text;
        public TMP_Text tmp_text;

        bool alsoChangeSelectedObjectName = true;

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
                    CommitInstanceEdit(foundText, () => foundText.text = newValue);
                }
                if (foundTMP != null)
                {
                    CommitInstanceEdit(foundTMP, () => foundTMP.text = newValue);
                }
                if (alsoChangeSelectedObjectName)
                {
                    CommitInstanceEdit(selected, () => selected.name = newValue);
                }
            }
  
            CoreMethodsTab.RenameAllSelected();
            CL_Window.current.Repaint();
        }

        static void CommitInstanceEdit(Object obj, System.Action apply)
        {
#if UNITY_6000_0_OR_NEWER
            if (PrefabUtility.IsPartOfPrefabAsset(obj) && !PrefabUtility.IsPartOfNonAssetPrefabInstance(obj))
            {
                return;
            }
            GameObject go = obj as GameObject;
            if (go == null)
            {
                go = ((Component)obj).gameObject;
            }
            if (PrefabStageUtility.GetPrefabStage(go) != null && !PrefabUtility.IsPartOfNonAssetPrefabInstance(obj))
            {
                return;
            }
            apply();
            if (PrefabUtility.IsPartOfNonAssetPrefabInstance(obj))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
                return;
            }
#else
            apply();
#endif
            UnityEditor.EditorUtility.SetDirty(obj);
        }

    }
}
