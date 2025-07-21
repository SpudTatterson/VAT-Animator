using UnityEngine;

public class BlendLooper : MonoBehaviour
{
    [SerializeField] private Renderer vatRenderer;
    [SerializeField] private string blendProperty = "_BlendFactor"; // Name of the blend property in the shader
    [SerializeField] private float loopDuration = 2f; // Time to loop from 0 to 1 and back
    [SerializeField] private float pauseDuration = 1f; // Pause duration at the end of each direction

    private Material cachedMaterial;
    private float elapsedTime;
    private bool isPaused;
    private float pauseTimer;
    private bool isReversing; // Tracks if we are blending backward

    void Start()
    {
        if (vatRenderer != null)
        {
            cachedMaterial = vatRenderer.material;
        }
    }

    void Update()
    {
        if (cachedMaterial == null)
        {
            Debug.LogWarning("Target Material is not assigned!");
            return;
        }

        if (isPaused)
        {
            pauseTimer += Time.deltaTime;
            if (pauseTimer >= pauseDuration)
            {
                isPaused = false;
                pauseTimer = 0f;
            }

            cachedMaterial.SetFloat(blendProperty, isReversing ? 1f : 0f); // Maintain final value during pause
            return; // Skip blending while paused
        }

        elapsedTime += Time.deltaTime;
        if (elapsedTime > loopDuration)
        {
            elapsedTime -= loopDuration;
            isPaused = true; // Start the pause phase
            isReversing = !isReversing; // Reverse direction
            cachedMaterial.SetFloat(blendProperty, isReversing ? 1f : 0f); // Ensure final value
            return;
        }

        // Calculate blend factor based on direction
        float blendFactor = isReversing
            ? 1f - Mathf.Clamp01(elapsedTime / loopDuration) // Reverse blending
            : Mathf.Clamp01(elapsedTime / loopDuration);     // Forward blending

        cachedMaterial.SetFloat(blendProperty, blendFactor);
    }
}
