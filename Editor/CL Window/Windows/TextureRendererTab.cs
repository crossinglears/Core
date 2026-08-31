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
        Asset,
        Screenshot
    }

    public partial class TextureRendererTab : CL_WindowTab
    {
        private enum TextureRendererAdjustMode
        {
            Object,
            Light
        }

        private static readonly string[] RenderModeLabels = new string[] { "Camera", "Asset", "Screenshot" };

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

        [SerializeField] private bool useTransparentBackground;

        private bool controllingCamera;
        private Vector2 lastMousePosition;
        private HashSet<KeyCode> pressedKeys = new HashSet<KeyCode>();
        private ReorderableList assetList;

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
                camera = UnityEditor.EditorUtility.EntityIdToObject(EntityId.FromULong(ulong.Parse(cameraID))) as Camera;
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
                EditorPrefs.SetString("TextureRendererCamera", EntityId.ToULong(camera.GetEntityId()).ToString());
            }
        }

        public override void DrawTitle()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(TabName, EditorStyles.boldLabel))
            {
                PingScript();
            }

            textureRendererMode = (TextureRendererMode)GUILayout.Toolbar((int)textureRendererMode, RenderModeLabels, GUILayout.Width(250));

            EditorGUILayout.EndHorizontal();
        }

        public override void DrawContent()
        {
            CL_Window.current.Repaint();

            if (textureRendererMode == TextureRendererMode.Camera)
            {
                DrawCameraContent();
            }
            else if(textureRendererMode == TextureRendererMode.Asset)
            {
                DrawAssetContent();
            }
            else if(textureRendererMode == TextureRendererMode.Screenshot)
            {
                DrawScreenShotContent();
            }
        }
    }
}