using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SFXClipData
{
    [Header("Audio Configuration")]
    public SFXType SFXType;
    public AudioClip Clip;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float Volume = 1f;
    [Range(0.1f, 3f)] public float Pitch = 1f;
    [Range(0f, 1f)] public float PitchVariation = 0f;
    
    [Header("Spatial Settings")]
    public bool Is3D = false;
    [Range(0f, 500f)] public float MaxDistance = 50f;
    [Range(0f, 1f)] public float SpatialBlend = 0f;
    
    [Header("Advanced Settings")]
    public bool RandomizePitch = false;
    public int Priority = 128;
    
    [Header("Description")]
    [TextArea(2, 3)] public string Description;
    
    public float GetRandomizedPitch()
    {
        if (!RandomizePitch || PitchVariation <= 0f) return Pitch;
        
        float variation = Random.Range(-PitchVariation, PitchVariation);
        return Mathf.Clamp(Pitch + variation, 0.1f, 3f);
    }
}