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
