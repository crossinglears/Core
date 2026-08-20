using UnityEditor;
using UnityEngine;
using System.IO;

namespace CrossingLears.Editor
{
    public partial class TextureRendererTab
    {
        public void DrawScreenShotContent()
        {
            EditorGUILayout.LabelField("Screenshot Folder", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            folder = (DefaultAsset)EditorGUILayout.ObjectField(folder, typeof(DefaultAsset), false);

            if (GUILayout.Button("Select", GUILayout.Width(60f)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Screenshot Folder", "", "");

                if (!string.IsNullOrEmpty(path))
                {
                    string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                    folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(relativePath);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Screenshot"))
            {
                if (folder == null)
                {
                    string path = EditorUtility.SaveFolderPanel("Select Screenshot Folder", "", "");

                    if (!string.IsNullOrEmpty(path))
                    {
                        string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                        folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(relativePath);
                    }
                }

                if (folder != null)
                {
                    string folderPath = AssetDatabase.GetAssetPath(folder);
                    // string filePath = Path.Combine(folderPath, "Screenshot.png");
                    string fileName = System.DateTime.Now.ToString("yy-MM-dd-HH-mm-ss-fff") + ".png";
                    string filePath = Path.Combine(folderPath, fileName);
                    ScreenCapture.CaptureScreenshot(filePath);

                    AssetDatabase.Refresh();
                    EditorApplication.delayCall += () =>
                    {
                        AssetDatabase.Refresh();
                    };
                }
            }
        }
    }
}