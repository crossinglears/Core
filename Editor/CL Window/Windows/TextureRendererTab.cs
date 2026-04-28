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

        private void BuildAssetList()
        {
            assetList = new ReorderableList(assets, typeof(Object), true, true, true, true);
            assetList.drawHeaderCallback = DrawAssetListHeader;
            assetList.drawElementCallback = DrawAssetListElement;
        }

        private void DrawAssetListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Assets");
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

                Texture2D preview = AssetPreview.GetAssetPreview(asset);

                if (preview == null)
                {
                    continue;
                }

                SaveAssetPreview(asset, preview, folderPath);
            }

            AssetDatabase.Refresh();
        }

        private void SaveAssetPreview(Object asset, Texture2D preview, string folderPath)
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

            byte[] bytes = texture.EncodeToPNG();
            string path = AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + asset.name + ".png");
            System.IO.File.WriteAllBytes(path, bytes);

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
