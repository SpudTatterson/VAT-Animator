using System.Collections;
using UnityEngine;

public class AnimationChangerTest : MonoBehaviour
{
    [SerializeField] VATAnimationData animationData;

    VatAnimator[] vatAnimators;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        vatAnimators = FindObjectsByType<VatAnimator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    // Update is called once per frame

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
            BlendToRandomAnimation();
    }

    [ContextMenu("Randomize All")]
    void BlendToRandomAnimation()
    {
        StartCoroutine(ChangeAnimationsRandom());
    }

    IEnumerator ChangeAnimationsRandom()
    {
        int i = 0;
        foreach (var vat in vatAnimators)
        {
            int random = Random.Range(0, animationData.animations.Length);
            StartCoroutine(vat.PlayAnimationBlended(random, 0.3f));
            i++;
            if (i >= 350)
            {
                i = 0;
                yield return null;
            }
        }
    }
}
