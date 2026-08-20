using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public partial class TextureRendererTab
    {
        private void DrawCameraContent()
        {
            camera = (Camera)EditorGUILayout.ObjectField("Camera", camera, typeof(Camera), true);
            folder = (DefaultAsset)EditorGUILayout.ObjectField("Folder", folder, typeof(DefaultAsset), false);

            useTransparentBackground = EditorGUILayout.Toggle("Use Transparent Background", useTransparentBackground);

            GUILayout.Space(20f);

            EditorGUILayout.BeginHorizontal();
            fileName = EditorGUILayout.TextField("Name", fileName);
            fileType = (FileType)EditorGUILayout.EnumPopup(fileType, GUILayout.Width(70));
            EditorGUILayout.EndHorizontal();

            outputResolution = EditorGUILayout.Vector2Field("Output Resolution", outputResolution);

            EditorGUILayout.BeginHorizontal();
            cameraMoveSpeed = EditorGUILayout.FloatField("Camera MoveSpeed", cameraMoveSpeed);
            cameraLookSpeed = EditorGUILayout.FloatField("Camera LookSpeed", cameraLookSpeed);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(20f);

            Rect previewRect = GUILayoutUtility.GetRect(0f, float.MaxValue, 0f, float.MaxValue, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (camera != null)
            {
                float targetAspect = outputResolution.x / outputResolution.y;
                float rectAspect = previewRect.width / previewRect.height;

                Rect fittedRect = previewRect;

                if (targetAspect > rectAspect)
                {
                    float fittedHeight = previewRect.width / targetAspect;
                    fittedRect.y += (previewRect.height - fittedHeight) * 0.5f;
                    fittedRect.height = fittedHeight;
                }
                else
                {
                    float fittedWidth = previewRect.height * targetAspect;
                    fittedRect.x += (previewRect.width - fittedWidth) * 0.5f;
                    fittedRect.width = fittedWidth;
                }

                HandleCameraInput(fittedRect);

                int previewWidth = Mathf.RoundToInt(outputResolution.x);
                int previewHeight = Mathf.RoundToInt(outputResolution.y);

                RenderTexture previewRT = RenderTexture.GetTemporary(previewWidth, previewHeight, 16);
                camera.targetTexture = previewRT;
                camera.Render();
                camera.targetTexture = null;

                EditorGUI.DrawPreviewTexture(fittedRect, previewRT, null, ScaleMode.ScaleToFit);
                RenderTexture.ReleaseTemporary(previewRT);
            }
            else
            {
                EditorGUI.DrawRect(previewRect, Color.black);
            }

            GUILayout.Space(10f);

            if (!GUILayout.Button("Render"))
            {
                return;
            }

            if (camera == null || folder == null)
            {
                return;
            }

            int outputWidth = Mathf.RoundToInt(outputResolution.x);
            int outputHeight = Mathf.RoundToInt(outputResolution.y);

            RenderTexture output = new RenderTexture(outputWidth, outputHeight, 24, RenderTextureFormat.ARGB32);
            Texture2D texture = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false);

            Color backgroundColor = camera.backgroundColor;

            camera.targetTexture = output;
            camera.Render();

            RenderTexture.active = output;
            texture.ReadPixels(new Rect(0f, 0f, outputWidth, outputHeight), 0, 0);
            texture.Apply();

            if (useTransparentBackground)
            {
                Color[] pixels = texture.GetPixels();

                for (int i = 0; i < pixels.Length; i++)
                {
                    Color pixel = pixels[i];

                    if (Mathf.Abs(pixel.r - backgroundColor.r) < 0.001f &&
                        Mathf.Abs(pixel.g - backgroundColor.g) < 0.001f &&
                        Mathf.Abs(pixel.b - backgroundColor.b) < 0.001f)
                    {
                        pixels[i] = new Color(pixel.r, pixel.g, pixel.b, 0f);
                    }
                }

                texture.SetPixels(pixels);
                texture.Apply();
            }

            camera.targetTexture = null;
            RenderTexture.active = null;

            byte[] bytes = fileType == FileType.PNG ? texture.EncodeToPNG() : texture.EncodeToJPG();
            string extension = fileType == FileType.PNG ? ".png" : ".jpg";

            string path = AssetDatabase.GetAssetPath(folder);
            string finalName = fileName;
            string fullPath = path + "/" + finalName + extension;

            int index = 1;

            while (System.IO.File.Exists(fullPath))
            {
                finalName = fileName + " (" + index + ")";
                fullPath = path + "/" + finalName + extension;
                index++;
            }

            System.IO.File.WriteAllBytes(fullPath, bytes);
            AssetDatabase.Refresh();

            AssetDatabase.ImportAsset(fullPath);

            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(fullPath);
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.SaveAndReimport();

            Object.DestroyImmediate(output);
            Object.DestroyImmediate(texture);
        }

        private void HandleCameraInput(Rect rect)
        {
            Event e = Event.current;

            if (camera == null)
            {
                return;
            }

            if (e.type == EventType.MouseDown && e.button == 1 && rect.Contains(e.mousePosition))
            {
                controllingCamera = true;
                lastMousePosition = e.mousePosition;
                e.Use();
            }

            if (e.type == EventType.MouseUp && e.button == 1)
            {
                controllingCamera = false;
                e.Use();
            }

            if (e.type == EventType.KeyDown)
            {
                pressedKeys.Add(e.keyCode);
            }

            if (e.type == EventType.KeyUp)
            {
                pressedKeys.Remove(e.keyCode);
            }

            if (controllingCamera && e.type == EventType.MouseDrag && e.button == 1)
            {
                Vector2 delta = e.mousePosition - lastMousePosition;
                lastMousePosition = e.mousePosition;

                Vector3 euler = camera.transform.rotation.eulerAngles;
                euler.x += delta.y * cameraLookSpeed;
                euler.y += delta.x * cameraLookSpeed;
                camera.transform.rotation = Quaternion.Euler(euler);

                e.Use();
            }

            float speedMultiplier = pressedKeys.Contains(KeyCode.LeftShift) || pressedKeys.Contains(KeyCode.RightShift) ? 2f : 1f;
            float moveSpeed = cameraMoveSpeed * speedMultiplier * 0.016f;

            Vector3 move = Vector3.zero;

            if (pressedKeys.Contains(KeyCode.W))
            {
                move += camera.transform.forward;
            }

            if (pressedKeys.Contains(KeyCode.S))
            {
                move -= camera.transform.forward;
            }

            if (pressedKeys.Contains(KeyCode.A))
            {
                move -= camera.transform.right;
            }

            if (pressedKeys.Contains(KeyCode.D))
            {
                move += camera.transform.right;
            }

            if (pressedKeys.Contains(KeyCode.E))
            {
                move += camera.transform.up;
            }

            if (pressedKeys.Contains(KeyCode.Q))
            {
                move -= camera.transform.up;
            }

            if (move != Vector3.zero)
            {
                camera.transform.position += move.normalized * moveSpeed;
            }
        }
    }
}