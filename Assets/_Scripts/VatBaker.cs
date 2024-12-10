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
    public AnimationClip[] animationClips;
    public int frameRate = 30;     // Frames per second

    [Header("Output Settings")]
    public string outputPath = "Assets/VAT_Baked";
    public bool generatePOTTexture = false; // Add this option for POT textures


    public List<AnimationInfo> animationsInfo = new List<AnimationInfo>();


    [ContextMenu("Bake")]
    public Texture2D BakeVAT()
    {
        if (!skinnedMeshRenderer || animationClips.Length == 0)
        {
            Debug.LogError("Please assign a SkinnedMeshRenderer and at least one AnimationClip.");
            return null;
        }

        Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
        if (sharedMesh == null)
        {
            Debug.LogError("SkinnedMeshRenderer does not have a shared mesh assigned.");
            return null;
        }

        int vertexCount = sharedMesh.vertexCount;

        int totalFrames = 0;
        foreach (var animation in animationClips)
        {
            float clipLength = animation.length;
            int frameCount = Mathf.CeilToInt(clipLength * frameRate);
            totalFrames += frameCount;
        }

        // Ensure the dimensions are POT if required
        int textureWidth = generatePOTTexture ? Mathf.NextPowerOfTwo(vertexCount) : vertexCount + 1;
        int textureHeight = generatePOTTexture ? Mathf.NextPowerOfTwo(totalFrames) : totalFrames;

        Texture2D vatTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBAFloat, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Mesh bakedMesh = new Mesh();
        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        foreach (var animation in animationClips)
        {
            float clipLength = animation.length;
            int frameCount = Mathf.CeilToInt(clipLength * frameRate);
            for (int frame = 0; frame < frameCount; frame++)
            {
                float time = frame / (float)frameRate;
                animation.SampleAnimation(animatedGameObject, time);
                skinnedMeshRenderer.rootBone.transform.localPosition = new Vector3(0, skinnedMeshRenderer.rootBone.transform.localPosition.y, 0);
                skinnedMeshRenderer.BakeMesh(bakedMesh);

                foreach (Vector3 vertex in bakedMesh.vertices)
                {
                    Vector3 localVertex = skinnedMeshRenderer.transform.InverseTransformPoint(vertex);
                    minBounds = Vector3.Min(minBounds, localVertex);
                    maxBounds = Vector3.Max(maxBounds, localVertex);
                }
            }
        }

        Vector3 boundsSize = maxBounds - minBounds;

        int currentFrame = 0;
        animationsInfo.Clear();

        foreach (var animation in animationClips)
        {
            float clipLength = animation.length;
            int frameCount = Mathf.CeilToInt(clipLength * frameRate);

            AnimationInfo metadata = new AnimationInfo
            {
                name = animation.name,
                startFrame = currentFrame,
                endFrame = currentFrame + frameCount,
            };
            animationsInfo.Add(metadata);

            for (int frame = 0; frame < frameCount; frame++)
            {
                float time = frame / (float)frameRate;
                animation.SampleAnimation(animatedGameObject, time);
                skinnedMeshRenderer.rootBone.transform.localPosition = new Vector3(0, skinnedMeshRenderer.rootBone.transform.localPosition.y, 0);
                skinnedMeshRenderer.BakeMesh(bakedMesh);

                Vector3[] vertices = bakedMesh.vertices;

                for (int vertex = 0; vertex < vertexCount; vertex++)
                {
                    Vector3 localVertex = skinnedMeshRenderer.transform.InverseTransformPoint(vertices[vertex]);
                    Vector3 normalizedVertex = new Vector3(
                        (localVertex.x - minBounds.x) / boundsSize.x,
                        (localVertex.y - minBounds.y) / boundsSize.y,
                        (localVertex.z - minBounds.z) / boundsSize.z
                    );

                    // Write to texture: Columns are vertices, rows are frames
                    if (vertex < textureWidth && currentFrame < textureHeight)
                    {
                        vatTexture.SetPixel(vertex, currentFrame, new Color(normalizedVertex.x, normalizedVertex.y, normalizedVertex.z, 1.0f));
                    }
                }

                currentFrame++;
            }
        }

        vatTexture.Apply();

#if UNITY_EDITOR
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string baseName = $"{sharedMesh.name}_VAT_{textureWidth}x{textureHeight}";

        string textureFileName = $"{baseName}.png";
        string textureFilePath = Path.Combine(outputPath, textureFileName);
        File.WriteAllBytes(textureFilePath, vatTexture.EncodeToPNG());
        Debug.Log($"VAT texture saved to {textureFilePath}");

        AssetDatabase.Refresh();
        TextureImporter textureImporter = AssetImporter.GetAtPath(textureFilePath) as TextureImporter;

        if (textureImporter != null)
        {
            textureImporter.textureType = TextureImporterType.Default;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.sRGBTexture = false;
            textureImporter.maxTextureSize = Mathf.NextPowerOfTwo(Mathf.Max(textureWidth, textureHeight));
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            textureImporter.mipmapEnabled = false;

            EditorUtility.SetDirty(textureImporter);
            textureImporter.SaveAndReimport();
        }
#endif

        VATAnimationData animationData = ScriptableObject.CreateInstance<VATAnimationData>();
        animationData.VATTexture = vatTexture;
        animationData.minBounds = minBounds;
        animationData.maxBounds = maxBounds;
        animationData.animations = animationsInfo.ToArray();
#if UNITY_EDITOR
        // Save the ScriptableObject as an asset
        string assetFileName = $"{sharedMesh.name}_VATAnimationData.asset";
        string assetFilePath = Path.Combine(outputPath, assetFileName);

        AssetDatabase.CreateAsset(animationData, assetFilePath);
        Debug.Log($"VATAnimationData saved to {assetFilePath}");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif

        return vatTexture;
    }
}
