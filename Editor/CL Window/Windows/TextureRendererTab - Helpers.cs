using UnityEditor;
using UnityEngine;

namespace CrossingLears.Editor
{
    public partial class TextureRendererTab
    {
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
    }
}