using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace CrossingLears.Editor
{
    public partial class CoreMethodsTab : CL_WindowTab
    {
        void RenameAllSelected()
        {                
            if (!GUILayout.Button(new GUIContent("Rename All Selected", "Renames all selected objects based on their text components, stripping tags and newlines.")))
                return;

            foreach (Transform item in Selection.transforms)
            {
                GameObject gameObject = item.gameObject;
                Undo.RecordObject(gameObject, "Rename All Selected");

                string text = null;

                Text textComponent = gameObject.GetComponentInChildren<Text>();
                if (textComponent != null && !string.IsNullOrEmpty(textComponent.text))
                    text = textComponent.text;

                if (text == null)
                {
                    TMPro.TextMeshProUGUI textMeshProUGUI = gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (textMeshProUGUI != null && !string.IsNullOrEmpty(textMeshProUGUI.text))
                        text = textMeshProUGUI.text;
                }

                if (text == null)
                {
                    TMPro.TextMeshPro textMeshPro = gameObject.GetComponentInChildren<TMPro.TextMeshPro>();
                    if (textMeshPro != null && !string.IsNullOrEmpty(textMeshPro.text))
                        text = textMeshPro.text;
                }

                if (text == null)
                {
                    UnityEngine.UI.Image image = gameObject.GetComponentInChildren<UnityEngine.UI.Image>();
                    if (image != null && image.sprite != null)
                        text = image.sprite.name;
                }

                if (text == null)
                {
                    SpriteRenderer spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null)
                        text = spriteRenderer.sprite.name;
                }

                if (string.IsNullOrEmpty(text))
                    continue;

                gameObject.name = System.Text.RegularExpressions.Regex
                    .Replace(text, "<.*?>", string.Empty)
                    .Replace("\n", " ");

                EditorUtility.SetDirty(gameObject);
            }

            if (!Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }
    }
}
