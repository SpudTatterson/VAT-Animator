using UnityEngine;

[CreateAssetMenu(menuName = "VATS/AnimationData")]
public class VATAnimationData : ScriptableObject
{
    public AnimationInfo[] animations;
    public Texture2D VATTexture;
    public Vector3 minBounds;
    public Vector3 maxBounds;
    
}

[System.Serializable]
public class AnimationInfo
{
    public string name;
    public int startFrame;
    public int endFrame;
}