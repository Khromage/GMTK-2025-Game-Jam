using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class MusicClipData
{
    [Header("Audio Configuration")]
    public MusicType MusicType;
    public AudioClip Clip;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float Volume = 1f;
    
    [Header("Playback Settings")]
    public bool Loop = true;
    public bool FadeIn = true;
    public bool FadeOut = true;
    [Range(0.1f, 10f)] public float FadeTime = 1f;
    
    [Header("Timing")]
    public float IntroLength = 0f;
    public float LoopStartTime = 0f;
    
    [Header("Description")]
    [TextArea(2, 3)] public string Description;
}