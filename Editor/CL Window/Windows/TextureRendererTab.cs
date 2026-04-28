using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditorInternal;

namespace CrossingLears.Editor
{
    public enum FileType
    {
        PNG,
        JPEG
    }

    public enum TextureRendererMode
    {
        Camera,
        Asset
    }

    public class TextureRendererTab : CL_WindowTab
    {
        private enum TextureRendererAdjustMode
        {
            Object,
            Light
        }

        private static readonly string[] RenderModeLabels = new string[] { "Camera", "Asset" };

        public override void Awake()
        {
            base.Awake();

            string folderPath = EditorPrefs.GetString("TextureRendererFolder", "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
            }

            string cameraID = EditorPrefs.GetString("TextureRendererCamera", "");
            if (!string.IsNullOrEmpty(cameraID))
            {
                camera = EditorUtility.InstanceIDToObject(int.Parse(cameraID)) as Camera;
            }

            BuildAssetList();
        }

        public override void OnDisable()
        {
            base.OnDisable();

            if (folder != null)
            {
                EditorPrefs.SetString("TextureRendererFolder", AssetDatabase.GetAssetPath(folder));
            }

            if (camera != null)
            {
                EditorPrefs.SetString("TextureRendererCamera", camera.GetInstanceID().ToString());
            }
        }

        public override string TabName => "Renderer";

        [SerializeField] private Camera camera;
        [SerializeField] private DefaultAsset folder;
        [SerializeField] private string fileName = "Render";
        [SerializeField] private FileType fileType = FileType.PNG;
        [SerializeField] private Vector2 outputResolution = new Vector2(1024f, 1024f);
        [SerializeField] private TextureRendererMode textureRendererMode = TextureRendererMode.Camera;
        [SerializeField] private List<Object> assets = new List<Object>();
        [SerializeField] private Color assetBackgroundColor = Color.clear;
        [SerializeField] private Color assetAmbientColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color assetLightColor = Color.white;
        [SerializeField] private float assetLightIntensity = 1f;
        [SerializeField] private float assetCameraSize = 1.25f;
        [SerializeField] private Vector3 assetObjectRotation = Vector3.zero;
        [SerializeField] private Vector3 assetLightRotation = new Vector3(50f, -30f, 0f);
        [SerializeField] private TextureRendererAdjustMode textureRendererAdjustMode = TextureRendererAdjustMode.Object;
        [SerializeField] private int assetPreviewIndex;

        [SerializeField] private float cameraMoveSpeed = 5f;
        [SerializeField] private float cameraLookSpeed = 0.2f;

        private bool controllingCamera;
        private Vector2 lastMousePosition;
        private HashSet<KeyCode> pressedKeys = new HashSet<KeyCode>();
        private ReorderableList assetList;

        public override void DrawTitle()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(TabName, EditorStyles.boldLabel))
            {
                PingScript();
            }
            textureRendererMode = (TextureRendererMode)GUILayout.Toolbar((int)textureRendererMode, RenderModeLabels, GUILayout.Width(160f));
            EditorGUILayout.EndHorizontal();
        }

        public override void DrawContent()
        {
            CL_Window.current.Repaint();

            if (textureRendererMode == TextureRendererMode.Camera)
            {
                DrawCameraContent();
            }
            else
            {
                DrawAssetContent();
            }
        }

        private void DrawCameraContent()
        {
            camera = (Camera)EditorGUILayout.ObjectField("Camera", camera, typeof(Camera), true);
            folder = (DefaultAsset)EditorGUILayout.ObjectField("Folder", folder, typeof(DefaultAsset), false);

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

            RenderTexture output = new RenderTexture(outputWidth, outputHeight, 24);
            Texture2D texture = new Texture2D(outputWidth, outputHeight, TextureFormat.RGB24, false);

            camera.targetTexture = output;
            camera.Render();

            RenderTexture.active = output;
            texture.ReadPixels(new Rect(0f, 0f, outputWidth, outputHeight), 0, 0);
            texture.Apply();

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

            Object.DestroyImmediate(output);
            Object.DestroyImmediate(texture);
        }

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

                if (preview == null && AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()))
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

        private GameObject CreatePreviewObject(Object asset)
        {
            GameObject gameObjectAsset = asset as GameObject;

            if (gameObjectAsset != null)
            {
                GameObject instance = Object.Instantiate(gameObjectAsset);
                instance.hideFlags = HideFlags.HideAndDontSave;
                return instance;
            }

            Mesh meshAsset = asset as Mesh;

            if (meshAsset != null)
            {
                GameObject meshObject = new GameObject(asset.name);
                MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = meshAsset;
                MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                meshObject.hideFlags = HideFlags.HideAndDontSave;
                return meshObject;
            }

            return null;
        }

        private Bounds GetPreviewBounds(GameObject previewObject)
        {
            Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private void SetPreviewLayer(Transform root, int layer)
        {
            root.gameObject.layer = layer;
            root.gameObject.hideFlags = HideFlags.HideAndDontSave;

            for (int i = 0; i < root.childCount; i++)
            {
                SetPreviewLayer(root.GetChild(i), layer);
            }
        }

        private void SaveAssetPreview(Object asset, Texture2D preview, string folderPath, bool applyBackgroundColor)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture renderTexture = RenderTexture.GetTemporary(preview.width, preview.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(preview, renderTexture);

            RenderTexture.active = renderTexture;

            Texture2D texture = new Texture2D(preview.width, preview.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0f, 0f, preview.width, preview.height), 0, 0);
            texture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);

            if (applyBackgroundColor)
            {
                ApplyBackgroundColor(texture);
            }

            byte[] bytes = texture.EncodeToPNG();
            string path = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + asset.name + ".png");
            System.IO.File.WriteAllBytes(path, bytes);

            AssetDatabase.ImportAsset(path);
            TextureImporter textureImporter = (TextureImporter)AssetImporter.GetAtPath(path);
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.spriteImportMode = SpriteImportMode.Single;
            textureImporter.SaveAndReimport();

            Object.DestroyImmediate(texture);
        }

        private void ApplyBackgroundColor(Texture2D texture)
        {
            Color[] pixels = texture.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                Color source = pixels[i];
                float outputAlpha = source.a + assetBackgroundColor.a * (1f - source.a);

                if (outputAlpha <= 0f)
                {
                    pixels[i] = Color.clear;
                    continue;
                }

                float red = (source.r * source.a + assetBackgroundColor.r * assetBackgroundColor.a * (1f - source.a)) / outputAlpha;
                float green = (source.g * source.a + assetBackgroundColor.g * assetBackgroundColor.a * (1f - source.a)) / outputAlpha;
                float blue = (source.b * source.a + assetBackgroundColor.b * assetBackgroundColor.a * (1f - source.a)) / outputAlpha;
                pixels[i] = new Color(red, green, blue, outputAlpha);
            }

            texture.SetPixels(pixels);
            texture.Apply();
        }

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
