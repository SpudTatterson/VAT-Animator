using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VATBaker : MonoBehaviour
{
    [Header("Bake Settings")]
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public GameObject animatedGameObject;
    public AnimationClip animationClip;
    public int frameRate = 30;     // Frames per second

    [Header("Output Settings")]
    public string outputPath = "Assets/VAT_Baked";

    [ContextMenu("Bake")]
    public Texture2D BakeVAT()
    {
        if (!skinnedMeshRenderer || !animationClip)
        {
            Debug.LogError("Please assign a SkinnedMeshRenderer and an AnimationClip.");
            return null;
        }

        Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
        int vertexCount = sharedMesh.vertexCount;

        // Calculate texture dimensions
        float clipLength = animationClip.length;
        int frameCount = Mathf.CeilToInt(clipLength * frameRate);
        int textureHeight = frameCount;

        // Create the texture
        Texture2D vatTexture = new Texture2D(vertexCount, textureHeight, TextureFormat.RGBAFloat, false);
        vatTexture.filterMode = FilterMode.Point;
        vatTexture.wrapMode = TextureWrapMode.Clamp;

        // Prepare baked mesh
        Mesh bakedMesh = new Mesh();
        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        // First pass: Compute bounds across all frames to normalize positions
        for (int frame = 0; frame < frameCount; frame++)
        {
            float time = frame / (float)frameRate;
            animationClip.SampleAnimation(animatedGameObject, time);
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            foreach (Vector3 vertex in bakedMesh.vertices)
            {
                Vector3 localVertex = skinnedMeshRenderer.transform.InverseTransformPoint(vertex);
                minBounds = Vector3.Min(minBounds, localVertex);
                maxBounds = Vector3.Max(maxBounds, localVertex);
            }
        }

        Vector3 boundsSize = maxBounds - minBounds;

        // Second pass: Bake data into the texture with normalized positions
        for (int frame = 0; frame < frameCount; frame++)
        {
            float time = frame / (float)frameRate;
            animationClip.SampleAnimation(animatedGameObject, time);
            skinnedMeshRenderer.BakeMesh(bakedMesh);

            Vector3[] vertices = bakedMesh.vertices;

            for (int vertex = 0; vertex < vertexCount; vertex++)
            {
                // Normalize vertex positions to [0, 1]
                Vector3 localVertex = skinnedMeshRenderer.transform.InverseTransformPoint(vertices[vertex]);
                Vector3 normalizedVertex = new Vector3(
                    (localVertex.x - minBounds.x) / boundsSize.x,
                    (localVertex.y - minBounds.y) / boundsSize.y,
                    (localVertex.z - minBounds.z) / boundsSize.z
                );

                // Write to texture: Columns are vertices, rows are frames
                vatTexture.SetPixel(vertex, frame, new Color(normalizedVertex.x, normalizedVertex.y, normalizedVertex.z, 1.0f));
            }
        }

        // Apply changes and save
        vatTexture.Apply();

#if UNITY_EDITOR
        string filePath = Path.Combine(outputPath, $"{animationClip.name}_VAT_{vertexCount}x{textureHeight}.png");
        Directory.CreateDirectory(outputPath);
        File.WriteAllBytes(filePath, vatTexture.EncodeToPNG());
        Debug.Log($"VAT texture saved to {filePath}");
        Debug.Log($"min bounds: {minBounds} max bounds: {maxBounds}");

        // Set import settings for the texture
        AssetDatabase.Refresh();
        TextureImporter textureImporter = AssetImporter.GetAtPath(filePath) as TextureImporter;

        if (textureImporter != null)
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.sRGBTexture = false; 
            textureImporter.maxTextureSize =  8192;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            textureImporter.mipmapEnabled = false;

            EditorUtility.SetDirty(textureImporter);
            textureImporter.SaveAndReimport();
        }
#endif

        return vatTexture;
    }
}
