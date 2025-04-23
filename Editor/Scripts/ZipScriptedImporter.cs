// using UnityEditor;
// using UnityEngine;
// using System.IO;
// using System.IO.Compression;

// public class ZipImporter : AssetPostprocessor
// {
//     // TODO: Optimize this by using ScriptedImporter
//     static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
//     {
//         foreach (string assetPath in importedAssets)
//         {
//             if (assetPath.EndsWith(".zip"))
//             {
//                 string fullPath = Path.GetFullPath(assetPath);
//                 string extractPath = Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath));

//                 if (!Directory.Exists(extractPath))
//                 {
//                     ZipFile.ExtractToDirectory(fullPath, extractPath);
//                     File.Delete(fullPath);
//                     File.Delete(fullPath + ".meta");
//                     AssetDatabase.Refresh();
//                 }
//                 else
//                 {
//                     Debug.Log($"Directory '{Path.GetFileName(extractPath)}' already exists. Zip was not extracted.");
//                 }
//             }
//         }
//     }
// }

using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using System.IO;
using System.IO.Compression;

[ScriptedImporter(1, "zip")]
public class ZipScriptedImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        Debug.Log("ZipScriptedImporter.OnImportAsset");
        
        string extractPath = Path.Combine(Path.GetDirectoryName(ctx.assetPath), Path.GetFileNameWithoutExtension(ctx.assetPath));
        string fullPath = Path.GetFullPath(ctx.assetPath);

        if (!Directory.Exists(extractPath))
        {
            ZipFile.ExtractToDirectory(fullPath, extractPath);
            // File.Delete(fullPath);
            // File.Delete(fullPath + ".meta");
            // AssetDatabase.Refresh();

            EditorApplication.delayCall += () =>
            {
                File.Delete(fullPath);
                File.Delete(fullPath + ".meta");
                AssetDatabase.Refresh();
            };
        }
        else
        {
            Debug.Log($"Directory '{Path.GetFileName(extractPath)}' already exists. Zip was not extracted.");
            EditorApplication.delayCall += () =>
            {
                File.Delete(fullPath);
                File.Delete(fullPath + ".meta");
            };
        }
    }
}
