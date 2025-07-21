using UnityEngine;

public class SampleAnimation : MonoBehaviour
{
    [SerializeField] AnimationClip clip;
    [SerializeField] float time;
    [SerializeField] SkinnedMeshRenderer skinnedMeshRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Sample();
    }

    [ContextMenu("Sample")]
    private void Sample()
    {
        clip.SampleAnimation(gameObject, time);
        skinnedMeshRenderer.rootBone.transform.localPosition = new Vector3(0, skinnedMeshRenderer.rootBone.transform.localPosition.y, 0);
    }
}
