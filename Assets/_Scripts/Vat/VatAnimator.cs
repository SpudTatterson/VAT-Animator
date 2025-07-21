using System.Collections;
using UnityEngine;

public class VatAnimator : MonoBehaviour
{
    const int Frame_Rate = 30;
    const int Time_Scale = 1;

    readonly int _startFrameRef = Shader.PropertyToID("_CurrentAnimStartFrame");
    readonly int _endFrameRef = Shader.PropertyToID("_CurrentAnimEndFrame");
    readonly int _blendFactorRef = Shader.PropertyToID("_BlendFactor");
    readonly int _nextAnimFrameIndexRef = Shader.PropertyToID("_NextAnimFrameIndex");
    readonly int _VatPositionRef = Shader.PropertyToID("_VatPosition");
    readonly int _offsetRef = Shader.PropertyToID("_Offset");

    [SerializeField] Renderer vatRenderer;
    [SerializeField] VATAnimationData animationData;
    [SerializeField] Material blendingMat;

    Material initialMaterial;
    int currentAnimationIndex;

    void Awake()
    {
        initialMaterial = vatRenderer.sharedMaterial;
    }

    public void PlayAnimation(int animationIndex)
    {
        AnimationInfo animationInfo = animationData.animations[animationIndex];

        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        vatRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetFloat(_startFrameRef, animationInfo.startFrame);
        propertyBlock.SetFloat(_endFrameRef, animationInfo.endFrame);

        vatRenderer.SetPropertyBlock(propertyBlock);
        currentAnimationIndex = animationIndex;
    }

    public IEnumerator PlayAnimationBlended(int animationIndex, float blendTime = 0.2f)
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        vatRenderer.GetPropertyBlock(propertyBlock);

        float currentOffset = propertyBlock.GetFloat(_offsetRef);

        // Switch to the blending material
        vatRenderer.sharedMaterial = blendingMat;

        AnimationInfo nextAnimationInfo = animationData.animations[animationIndex];
        AnimationInfo currentAnimationInfo = animationData.animations[currentAnimationIndex];

        // Set the current animation frames on the blending mat
        propertyBlock.SetFloat(_startFrameRef, currentAnimationInfo.startFrame);
        propertyBlock.SetFloat(_endFrameRef, currentAnimationInfo.endFrame);


        propertyBlock.SetFloat(_offsetRef, currentOffset);


        int vatHeight = vatRenderer.sharedMaterial.GetTexture(_VatPositionRef).height;

        float elapsedTime = 0f;
        while (elapsedTime < blendTime)
        {
            elapsedTime += Time.deltaTime;
            float blendFactor = Mathf.Clamp01(elapsedTime / blendTime);
            propertyBlock.SetFloat(_blendFactorRef, blendFactor);

            // Calculate the normalized frame index for the next animation
            float scaledTime = elapsedTime * Time_Scale * Frame_Rate;
            float normalizedFrameIndex = Mathf.InverseLerp(0, vatHeight, scaledTime + nextAnimationInfo.startFrame);

            propertyBlock.SetFloat(_nextAnimFrameIndexRef, normalizedFrameIndex);

            vatRenderer.SetPropertyBlock(propertyBlock);
            yield return null;
        }

        // Blending finished. Now we switch back to the original material and synchronize offsets.
        vatRenderer.sharedMaterial = initialMaterial;
        propertyBlock.Clear();
        vatRenderer.GetPropertyBlock(propertyBlock);

        // Set up the next animation frames on the original material
        propertyBlock.SetFloat(_startFrameRef, nextAnimationInfo.startFrame);
        propertyBlock.SetFloat(_endFrameRef, nextAnimationInfo.endFrame);

        // Now we compute the offset so that the animation continues at the correct frame.

        float finalFrameIndex = blendTime * Time_Scale * Frame_Rate;
        float totalFramesMinusOne = nextAnimationInfo.endFrame - nextAnimationInfo.startFrame;

        float offset = (Time.time * Time_Scale * Frame_Rate) - finalFrameIndex;
        offset = offset % totalFramesMinusOne;


        propertyBlock.SetFloat(_offsetRef, offset);


        vatRenderer.SetPropertyBlock(propertyBlock);
        currentAnimationIndex = animationIndex;
    }

}
