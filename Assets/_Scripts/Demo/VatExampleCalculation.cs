using System.Collections;
using UnityEngine;

public class VatExampleCalculation : MonoBehaviour
{
    int timeScale = 1;
    int curAnimStartFrame = 0;
    int curAnimEndFrame = 30;
    int frameRate = 30;
    int vatHeight = 100;

    float vertexID = 500;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void CalculateVertexIndex()
    {
        float textureWidth = 8048;
        float textureWithMinus1 = textureWidth - 1;
        float normalizedVertexIndex = vertexID / textureWithMinus1;
        Debug.Log(normalizedVertexIndex);
    }

    private void CalculateNormalizedFrame()
    {
        float scaledTime = Time.time * timeScale * frameRate;
        float totalFramesMinusOne = curAnimEndFrame - curAnimStartFrame;
        float loopedFrameIndex = scaledTime % totalFramesMinusOne;
        float frameIndex = loopedFrameIndex + curAnimStartFrame;

        float normalizedFrameIndex = Mathf.InverseLerp(0, vatHeight, frameIndex);
        Debug.Log($"scaledTime = {scaledTime} Total frames -1 = {totalFramesMinusOne} looped frame index = {loopedFrameIndex} frame index = {frameIndex} the thing we need = {normalizedFrameIndex}");
    }
}

