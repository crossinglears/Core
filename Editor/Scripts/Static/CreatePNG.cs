using UnityEngine;
using UnityEditor;
using System.IO;

namespace CrossingLears.Editor
{
    public class CreateEmptyPNG
    {
        [MenuItem("Assets/Create/CrossingLears/Empty PNG", false, 100)]
        private static void CreatePNG()
        {
            string path = GetSelectedPathOrFallback() + "/NewImage.png";

            if (File.Exists(path)) // Avoid overwriting existing files
            {
                path = AssetDatabase.GenerateUniqueAssetPath(path);
            }

            byte[] emptyPNG = GenerateEmptyPNG(256, 256); // Default size
            File.WriteAllBytes(path, emptyPNG);

            AssetDatabase.Refresh();
        }

        private static byte[] GenerateEmptyPNG(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            Color clear = new Color(0, 0, 0, 0); // Transparent
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, clear);
                }
            }
            texture.Apply();
            return texture.EncodeToPNG();
        }

        private static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                }
                break;
            }
            return path;
        }
    }

}
