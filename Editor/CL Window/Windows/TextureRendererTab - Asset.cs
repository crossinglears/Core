using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;

namespace CrossingLears.Editor
{
    public partial class TextureRendererTab
    {
        private void DrawAssetContent()
        {
            folder = (DefaultAsset)EditorGUILayout.ObjectField("Folder", folder, typeof(DefaultAsset), false);

            GUILayout.Space(10f);

            DrawAssetRenderSettingsFields();

            GUILayout.Space(10f);

            if (GUILayout.Button("Angle / Lighting"))
            {
                TextureRendererAssetSettingsWindow.Open(this);
            }

            GUILayout.Space(10f);

            if (assetList == null)
            {
                BuildAssetList();
            }

            assetList.DoLayoutList();

            GUILayout.Space(10f);

            if (GUILayout.Button("Generate Textures") && folder != null)
            {
                GenerateAssetTextures();
            }
        }

        private void DrawAssetRenderSettingsFields()
        {
            assetBackgroundColor = EditorGUILayout.ColorField("Background Color", assetBackgroundColor);

            EditorGUILayout.LabelField("Lighting Settings", EditorStyles.boldLabel);
            assetAmbientColor = EditorGUILayout.ColorField("Ambient Color", assetAmbientColor);

            EditorGUILayout.BeginHorizontal();
            assetLightColor = EditorGUILayout.ColorField("Light Color", assetLightColor);
            assetLightIntensity = EditorGUILayout.FloatField("Light Intensity", assetLightIntensity);
            EditorGUILayout.EndHorizontal();

            assetCameraSize = Mathf.Max(0.01f, EditorGUILayout.FloatField("Camera Size", assetCameraSize));
            assetObjectRotation = EditorGUILayout.Vector3Field("Object Rotation", assetObjectRotation);
            assetLightRotation = EditorGUILayout.Vector3Field("Light Rotation", assetLightRotation);
        }

        private void DrawAssetAdjustModeField()
        {
            bool objectMode = textureRendererAdjustMode == TextureRendererAdjustMode.Object;
            bool lightMode = textureRendererAdjustMode == TextureRendererAdjustMode.Light;

            EditorGUILayout.BeginHorizontal();
            bool nextObjectMode = GUILayout.Toggle(objectMode, "Object", EditorStyles.radioButton, GUILayout.Width(80f));
            bool nextLightMode = GUILayout.Toggle(lightMode, "Light", EditorStyles.radioButton, GUILayout.Width(80f));
            EditorGUILayout.EndHorizontal();

            if (nextObjectMode && nextObjectMode != objectMode)
            {
                textureRendererAdjustMode = TextureRendererAdjustMode.Object;
            }

            if (nextLightMode && nextLightMode != lightMode)
            {
                textureRendererAdjustMode = TextureRendererAdjustMode.Light;
            }
        }

        private void DrawAssetPreviewSelector()
        {
            if (assets.Count <= 1)
            {
                return;
            }

            NormalizeAssetPreviewIndex();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("<", GUILayout.Width(32f)))
            {
                assetPreviewIndex--;
                NormalizeAssetPreviewIndex();
            }

            GUILayout.Label((assetPreviewIndex + 1).ToString() + " / " + assets.Count.ToString(), EditorStyles.centeredGreyMiniLabel);

            if (GUILayout.Button(">", GUILayout.Width(32f)))
            {
                assetPreviewIndex++;
                NormalizeAssetPreviewIndex();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void NormalizeAssetPreviewIndex()
        {
            if (assets.Count == 0)
            {
                assetPreviewIndex = 0;
                return;
            }

            if (assetPreviewIndex < 0)
            {
                assetPreviewIndex = assets.Count - 1;
                return;
            }

            if (assetPreviewIndex >= assets.Count)
            {
                assetPreviewIndex = 0;
            }
        }

        private void BuildAssetList()
        {
            assetList = new ReorderableList(assets, typeof(Object), true, true, true, true);
            assetList.drawHeaderCallback = DrawAssetListHeader;
            assetList.drawElementCallback = DrawAssetListElement;
        }

        private void DrawAssetListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Assets");

            Event currentEvent = Event.current;

            if (!rect.Contains(currentEvent.mousePosition))
            {
                return;
            }

            if (currentEvent.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                currentEvent.Use();
                return;
            }

            if (currentEvent.type != EventType.DragPerform)
            {
                return;
            }

            DragAndDrop.AcceptDrag();

            Object[] droppedObjects = DragAndDrop.objectReferences;

            for (int i = 0; i < droppedObjects.Length; i++)
            {
                assets.Add(droppedObjects[i]);
            }

            currentEvent.Use();
            CL_Window.current.Repaint();
        }

        private void DrawAssetListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;
            assets[index] = EditorGUI.ObjectField(rect, assets[index], typeof(Object), false);
        }

        private void GenerateAssetTextures()
        {
            string folderPath = AssetDatabase.GetAssetPath(folder);

            for (int i = 0; i < assets.Count; i++)
            {
                Object asset = assets[i];

                if (asset == null)
                {
                    continue;
                }

                if (CanRenderAsset(asset))
                {
                    continue;
                }

                Texture2D preview = AssetPreview.GetAssetPreview(asset);

                if (preview == null && AssetPreview.IsLoadingAssetPreview(asset.GetEntityId()))
                {
                    EditorApplication.delayCall += GenerateAssetTextures;
                    return;
                }
            }

            for (int i = 0; i < assets.Count; i++)
            {
                Object asset = assets[i];

                if (asset == null)
                {
                    continue;
                }

                Texture2D renderedTexture = RenderAssetTexture(asset);

                if (renderedTexture != null)
                {
                    SaveAssetPreview(asset, renderedTexture, folderPath, false);
                    Object.DestroyImmediate(renderedTexture);
                    continue;
                }

                Texture2D preview = AssetPreview.GetAssetPreview(asset);

                if (preview == null)
                {
                    continue;
                }

                SaveAssetPreview(asset, preview, folderPath, true);
            }

            AssetDatabase.Refresh();
        }

        private bool CanRenderAsset(Object asset)
        {
            if (asset is GameObject)
            {
                return true;
            }

            if (asset is Mesh)
            {
                return true;
            }

            return false;
        }

        private Texture2D RenderAssetTexture(Object asset)
        {
            int outputWidth = Mathf.RoundToInt(outputResolution.x);
            int outputHeight = Mathf.RoundToInt(outputResolution.y);

            return RenderAssetTexture(asset, outputWidth, outputHeight);
        }

        private Texture2D RenderAssetTexture(Object asset, int outputWidth, int outputHeight)
        {
            GameObject previewObject = CreatePreviewObject(asset);

            if (previewObject == null)
            {
                return null;
            }

            outputWidth = Mathf.Max(1, outputWidth);
            outputHeight = Mathf.Max(1, outputHeight);

            int previewLayer = 31;

            GameObject cameraObject = new GameObject("Texture Renderer Camera");
            Camera previewCamera = cameraObject.AddComponent<Camera>();

            GameObject lightObject = new GameObject("Texture Renderer Light");
            Light previewLight = lightObject.AddComponent<Light>();

            RenderTexture previousActive = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(outputWidth, outputHeight, 24, RenderTextureFormat.ARGB32);

            Color previousAmbientLight = RenderSettings.ambientLight;
            UnityEngine.Rendering.AmbientMode previousAmbientMode = RenderSettings.ambientMode;

            try
            {
                SetPreviewLayer(previewObject.transform, previewLayer);

                previewObject.transform.rotation = Quaternion.Euler(assetObjectRotation);

                Bounds bounds = GetPreviewBounds(previewObject);

                previewObject.transform.position -= bounds.center;

                bounds = GetPreviewBounds(previewObject);

                cameraObject.hideFlags = HideFlags.HideAndDontSave;
                lightObject.hideFlags = HideFlags.HideAndDontSave;

                previewCamera.clearFlags = CameraClearFlags.Color;
                previewCamera.backgroundColor = assetBackgroundColor;
                previewCamera.fieldOfView = 30f;
                previewCamera.nearClipPlane = 0.01f;
                previewCamera.cullingMask = 1 << previewLayer;
                previewCamera.targetTexture = renderTexture;

                float aspect = (float)outputWidth / outputHeight;
                float fitSize = Mathf.Max(bounds.extents.y, bounds.extents.x / aspect);

                if (fitSize <= 0f)
                {
                    fitSize = 1f;
                }

                float distance = fitSize / Mathf.Tan(previewCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                distance += bounds.extents.z;
                distance *= assetCameraSize;

                previewCamera.transform.position = new Vector3(0f, 0f, -distance);
                previewCamera.transform.LookAt(Vector3.zero);
                previewCamera.farClipPlane = distance + bounds.extents.magnitude + 10f;

                previewLight.type = LightType.Directional;
                previewLight.color = assetLightColor;
                previewLight.intensity = assetLightIntensity;
                previewLight.cullingMask = 1 << previewLayer;
                lightObject.transform.rotation = Quaternion.Euler(assetLightRotation);

                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = assetAmbientColor;

                previewCamera.Render();

                RenderTexture.active = renderTexture;

                Texture2D texture = new Texture2D(outputWidth, outputHeight, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0f, 0f, outputWidth, outputHeight), 0, 0);
                texture.Apply();

                return texture;
            }
            finally
            {
                previewCamera.targetTexture = null;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);

                RenderSettings.ambientMode = previousAmbientMode;
                RenderSettings.ambientLight = previousAmbientLight;

                Object.DestroyImmediate(lightObject);
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(previewObject);
            }
        }
    }
}