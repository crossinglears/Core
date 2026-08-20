using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public partial class TextureRendererTab
    {
        private Object GetAssetSettingsPreviewAsset()
        {
            if (assets.Count == 0)
            {
                return null;
            }

            NormalizeAssetPreviewIndex();
            return assets[assetPreviewIndex];
        }

        private Rect GetAssetSettingsPreviewFrameRect(Rect rect)
        {
            float targetAspect = Mathf.Max(1f, outputResolution.x) / Mathf.Max(1f, outputResolution.y);
            float rectAspect = rect.width / rect.height;
            Rect fittedRect = rect;

            if (targetAspect > rectAspect)
            {
                float fittedHeight = rect.width / targetAspect;
                fittedRect.y += (rect.height - fittedHeight) * 0.5f;
                fittedRect.height = fittedHeight;
            }
            else
            {
                float fittedWidth = rect.height * targetAspect;
                fittedRect.x += (rect.width - fittedWidth) * 0.5f;
                fittedRect.width = fittedWidth;
            }

            return fittedRect;
        }

        private void DrawAssetSettingsPreviewFrame(Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));

            Rect frameRect = GetAssetSettingsPreviewFrameRect(rect);
            DrawAssetSettingsPreview(frameRect);
            GUI.Box(frameRect, GUIContent.none);
        }

        private void DrawAssetSettingsPreview(Rect rect)
        {
            EditorGUI.DrawRect(rect, assetBackgroundColor);

            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Object previewAsset = GetAssetSettingsPreviewAsset();

            if (previewAsset == null)
            {
                return;
            }

            int previewWidth = Mathf.RoundToInt(rect.width);
            int previewHeight = Mathf.RoundToInt(rect.height);

            if (CanRenderAsset(previewAsset))
            {
                Texture2D renderedPreview = RenderAssetTexture(previewAsset, previewWidth, previewHeight);

                if (renderedPreview != null)
                {
                    GUI.DrawTexture(rect, renderedPreview, ScaleMode.ScaleToFit, true);
                    Object.DestroyImmediate(renderedPreview);
                }

                return;
            }

            Texture2D assetPreview = AssetPreview.GetAssetPreview(previewAsset);

            if (assetPreview == null)
            {
                return;
            }

            GUI.DrawTexture(rect, assetPreview, ScaleMode.ScaleToFit, true);
        }

        private sealed class TextureRendererAssetSettingsWindow : EditorWindow
        {
            private TextureRendererTab tab;
            private bool dragging;
            private Vector2 lastMousePosition;

            public static void Open(TextureRendererTab tab)
            {
                TextureRendererAssetSettingsWindow window = GetWindow<TextureRendererAssetSettingsWindow>("Angle / Lighting");
                window.tab = tab;
                window.minSize = new Vector2(360f, 480f);
                window.Show();
            }

            private void OnGUI()
            {
                if (tab == null)
                {
                    Close();
                    return;
                }

                tab.DrawAssetAdjustModeField();
                tab.DrawAssetRenderSettingsFields();
                tab.DrawAssetPreviewSelector();

                GUILayout.Space(10f);

                Rect previewRect = GUILayoutUtility.GetRect(0f, float.MaxValue, 0f, float.MaxValue, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                Rect frameRect = tab.GetAssetSettingsPreviewFrameRect(previewRect);

                tab.DrawAssetSettingsPreviewFrame(previewRect);

                HandleDrag(frameRect);
            }

            private void HandleDrag(Rect rect)
            {
                Event currentEvent = Event.current;

                if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
                {
                    dragging = true;
                    lastMousePosition = currentEvent.mousePosition;
                    currentEvent.Use();
                    return;
                }

                if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 && dragging)
                {
                    dragging = false;
                    currentEvent.Use();
                    return;
                }

                if (!dragging || currentEvent.type != EventType.MouseDrag || currentEvent.button != 0)
                {
                    return;
                }

                Vector2 delta = currentEvent.mousePosition - lastMousePosition;
                lastMousePosition = currentEvent.mousePosition;

                if (tab.textureRendererAdjustMode == TextureRendererAdjustMode.Object)
                {
                    tab.assetObjectRotation.x += delta.y * 0.4f;
                    tab.assetObjectRotation.y += delta.x * 0.4f;
                }
                else
                {
                    tab.assetLightRotation.x += delta.y * 0.4f;
                    tab.assetLightRotation.y += delta.x * 0.4f;
                }

                currentEvent.Use();
                Repaint();
                CL_Window.current.Repaint();
            }
        }
    }
}