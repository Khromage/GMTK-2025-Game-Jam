using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class AmbientClipData
{
    [Header("Audio Configuration")]
    public AmbientType AmbientType;
    public AudioClip Clip;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float Volume = 1f;
    
    [Header("Playback Settings")]
    public bool Loop = true;
    public bool FadeIn = true;
    public bool FadeOut = true;
    [Range(0.1f, 10f)] public float FadeTime = 2f;
    
    [Header("Spatial Settings")]
    public bool Is3D = false;
    [Range(0f, 500f)] public float MaxDistance = 100f;
    [Range(0f, 1f)] public float SpatialBlend = 0f;
    
    [Header("Randomization")]
    public bool RandomizeVolume = false;
    [Range(0f, 0.5f)] public float VolumeVariation = 0.1f;
    public bool RandomizePitch = false;
    [Range(0f, 0.2f)] public float PitchVariation = 0.05f;
    
    [Header("Description")]
    [TextArea(2, 3)] public string Description;
    
    public float GetRandomizedVolume()
    {
        if (!RandomizeVolume || VolumeVariation <= 0f) return Volume;
        
        float variation = Random.Range(-VolumeVariation, VolumeVariation);
        return Mathf.Clamp01(Volume + variation);
    }
    
    public float GetRandomizedPitch()
    {
        if (!RandomizePitch || PitchVariation <= 0f) return 1f;
        
        float variation = Random.Range(-PitchVariation, PitchVariation);
        return Mathf.Clamp(1f + variation, 0.5f, 2f);
    }
}