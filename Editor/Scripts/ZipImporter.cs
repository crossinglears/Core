using UnityEditor;
using UnityEngine;
using System.IO;
using System.IO.Compression;

public class ZipImporter : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        Debug.Log("ZipImporter");
        foreach (string assetPath in importedAssets)
        {
            if (assetPath.EndsWith(".zip"))
            {
                string fullPath = Path.GetFullPath(assetPath);
                string extractPath = Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath));

                if (!Directory.Exists(extractPath))
                {
                    ZipFile.ExtractToDirectory(fullPath, extractPath);
                    File.Delete(fullPath);
                    File.Delete(fullPath + ".meta");
                    AssetDatabase.Refresh();
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Zip Import",
                        $"Directory '{Path.GetFileName(extractPath)}' already exists. Zip was not extracted.",
                        "OK"
                    );
                }
            }
        }
    }
}
