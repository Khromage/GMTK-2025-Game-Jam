using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipLibrary", menuName = "Sisyphus/Audio Clip Library")]
public class AudioClipLibrarySO : ScriptableObject
{
    [Header("SFX Clips")]
    [SerializeField] private SFXClipData[] _sfxClips;
    
    [Header("Music Clips")]
    [SerializeField] private MusicClipData[] _musicClips;
    
    [Header("Ambient Clips")]
    [SerializeField] private AmbientClipData[] _ambientClips;
    
    // Cached dictionaries for fast lookup (populated at runtime)
    private Dictionary<SFXType, SFXClipData> _sfxCache;
    private Dictionary<MusicType, MusicClipData> _musicCache;
    private Dictionary<AmbientType, AmbientClipData> _ambientCache;
    
    void OnEnable()
    {
        BuildAudioCache();
    }
    
    private void BuildAudioCache()
    {
        // Build SFX cache
        _sfxCache = new Dictionary<SFXType, SFXClipData>();
        if (_sfxClips != null)
        {
            foreach (SFXClipData clipData in _sfxClips)
            {
                if (clipData.Clip != null)
                {
                    if (!_sfxCache.ContainsKey(clipData.SFXType))
                    {
                        _sfxCache[clipData.SFXType] = clipData;
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate SFX type found: {clipData.SFXType}. Using first occurrence.");
                    }
                }
                else
                {
                    Debug.LogWarning($"SFX clip is null for type: {clipData.SFXType}");
                }
            }
        }
        
        // Build music cache
        _musicCache = new Dictionary<MusicType, MusicClipData>();
        if (_musicClips != null)
        {
            foreach (MusicClipData clipData in _musicClips)
            {
                if (clipData.Clip != null)
                {
                    if (!_musicCache.ContainsKey(clipData.MusicType))
                    {
                        _musicCache[clipData.MusicType] = clipData;
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate Music type found: {clipData.MusicType}. Using first occurrence.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Music clip is null for type: {clipData.MusicType}");
                }
            }
        }
        
        // Build ambient cache
        _ambientCache = new Dictionary<AmbientType, AmbientClipData>();
        if (_ambientClips != null)
        {
            foreach (AmbientClipData clipData in _ambientClips)
            {
                if (clipData.Clip != null)
                {
                    if (!_ambientCache.ContainsKey(clipData.AmbientType))
                    {
                        _ambientCache[clipData.AmbientType] = clipData;
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate Ambient type found: {clipData.AmbientType}. Using first occurrence.");
                    }
                }
                else
                {
                    Debug.LogWarning($"Ambient clip is null for type: {clipData.AmbientType}");
                }
            }
        }
    }
    
    #region Public Getter Methods
    
    public AudioClip GetSFXClip(SFXType sfxType)
    {
        if (_sfxCache == null) BuildAudioCache();
        return _sfxCache.TryGetValue(sfxType, out SFXClipData clipData) ? clipData.Clip : null;
    }
    
    public SFXClipData GetSFXClipData(SFXType sfxType)
    {
        if (_sfxCache == null) BuildAudioCache();
        return _sfxCache.TryGetValue(sfxType, out SFXClipData clipData) ? clipData : null;
    }
    
    public AudioClip GetMusicClip(MusicType musicType)
    {
        if (_musicCache == null) BuildAudioCache();
        return _musicCache.TryGetValue(musicType, out MusicClipData clipData) ? clipData.Clip : null;
    }
    
    public MusicClipData GetMusicClipData(MusicType musicType)
    {
        if (_musicCache == null) BuildAudioCache();
        return _musicCache.TryGetValue(musicType, out MusicClipData clipData) ? clipData : null;
    }
    
    public AudioClip GetAmbientClip(AmbientType ambientType)
    {
        if (_ambientCache == null) BuildAudioCache();
        return _ambientCache.TryGetValue(ambientType, out AmbientClipData clipData) ? clipData.Clip : null;
    }
    
    public AmbientClipData GetAmbientClipData(AmbientType ambientType)
    {
        if (_ambientCache == null) BuildAudioCache();
        return _ambientCache.TryGetValue(ambientType, out AmbientClipData clipData) ? clipData : null;
    }
    
    #endregion
    
    #region Validation Methods
    
    public bool ValidateAudioLibrary()
    {
        bool isValid = true;
        
        // Check for missing SFX clips
        foreach (SFXType sfxType in System.Enum.GetValues(typeof(SFXType)))
        {
            if (!HasSFXClip(sfxType))
            {
                Debug.LogWarning($"Missing SFX clip for type: {sfxType}");
                isValid = false;
            }
        }
        
        // Check for missing Music clips
        foreach (MusicType musicType in System.Enum.GetValues(typeof(MusicType)))
        {
            if (musicType != MusicType.None && !HasMusicClip(musicType))
            {
                Debug.LogWarning($"Missing Music clip for type: {musicType}");
                isValid = false;
            }
        }
        
        // Check for missing Ambient clips
        foreach (AmbientType ambientType in System.Enum.GetValues(typeof(AmbientType)))
        {
            if (ambientType != AmbientType.None && !HasAmbientClip(ambientType))
            {
                Debug.LogWarning($"Missing Ambient clip for type: {ambientType}");
                isValid = false;
            }
        }
        
        return isValid;
    }
    
    public bool HasSFXClip(SFXType sfxType)
    {
        if (_sfxCache == null) BuildAudioCache();
        return _sfxCache.ContainsKey(sfxType) && _sfxCache[sfxType].Clip != null;
    }
    
    public bool HasMusicClip(MusicType musicType)
    {
        if (_musicCache == null) BuildAudioCache();
        return _musicCache.ContainsKey(musicType) && _musicCache[musicType].Clip != null;
    }
    
    public bool HasAmbientClip(AmbientType ambientType)
    {
        if (_ambientCache == null) BuildAudioCache();
        return _ambientCache.ContainsKey(ambientType) && _ambientCache[ambientType].Clip != null;
    }
    
    #endregion
    
    #region Editor Utility Methods
    
    #if UNITY_EDITOR
    [ContextMenu("Auto-Populate Missing Entries")]
    private void AutoPopulateMissingEntries()
    {
        // Auto-populate SFX entries
        var sfxTypes = System.Enum.GetValues(typeof(SFXType));
        List<SFXClipData> sfxList = new List<SFXClipData>(_sfxClips ?? new SFXClipData[0]);
        
        foreach (SFXType sfxType in sfxTypes)
        {
            bool exists = false;
            foreach (var clipData in sfxList)
            {
                if (clipData.SFXType == sfxType)
                {
                    exists = true;
                    break;
                }
            }
            
            if (!exists)
            {
                sfxList.Add(new SFXClipData { SFXType = sfxType });
            }
        }
        _sfxClips = sfxList.ToArray();
        
        // Auto-populate Music entries
        var musicTypes = System.Enum.GetValues(typeof(MusicType));
        List<MusicClipData> musicList = new List<MusicClipData>(_musicClips ?? new MusicClipData[0]);
        
        foreach (MusicType musicType in musicTypes)
        {
            if (musicType == MusicType.None) continue;
            
            bool exists = false;
            foreach (var clipData in musicList)
            {
                if (clipData.MusicType == musicType)
                {
                    exists = true;
                    break;
                }
            }
            
            if (!exists)
            {
                musicList.Add(new MusicClipData { MusicType = musicType });
            }
        }
        _musicClips = musicList.ToArray();
        
        // Auto-populate Ambient entries
        var ambientTypes = System.Enum.GetValues(typeof(AmbientType));
        List<AmbientClipData> ambientList = new List<AmbientClipData>(_ambientClips ?? new AmbientClipData[0]);
        
        foreach (AmbientType ambientType in ambientTypes)
        {
            if (ambientType == AmbientType.None) continue;
            
            bool exists = false;
            foreach (var clipData in ambientList)
            {
                if (clipData.AmbientType == ambientType)
                {
                    exists = true;
                    break;
                }
            }
            
            if (!exists)
            {
                ambientList.Add(new AmbientClipData { AmbientType = ambientType });
            }
        }
        _ambientClips = ambientList.ToArray();
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("Auto-populated missing audio entries");
    }
    
    [ContextMenu("Sort All Entries")]
    private void SortAllEntries()
    {
        // Sort SFX clips by enum order
        if (_sfxClips != null)
        {
            System.Array.Sort(_sfxClips, (a, b) => a.SFXType.CompareTo(b.SFXType));
        }
        
        // Sort Music clips by enum order
        if (_musicClips != null)
        {
            System.Array.Sort(_musicClips, (a, b) => a.MusicType.CompareTo(b.MusicType));
        }
        
        // Sort Ambient clips by enum order
        if (_ambientClips != null)
        {
            System.Array.Sort(_ambientClips, (a, b) => a.AmbientType.CompareTo(b.AmbientType));
        }
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log("Sorted all audio entries");
    }
    
    [ContextMenu("Validate Audio Library")]
    private void ValidateAudioLibraryEditor()
    {
        bool isValid = ValidateAudioLibrary();
        if (isValid)
        {
            Debug.Log("Audio Library validation passed - all clips are properly assigned!");
        }
        else
        {
            Debug.LogWarning("Audio Library validation failed - check console for missing clips");
        }
    }
    #endif
    
    #endregion
    
    #region Statistics
    
    public int GetTotalSFXCount() => _sfxClips?.Length ?? 0;
    public int GetTotalMusicCount() => _musicClips?.Length ?? 0;
    public int GetTotalAmbientCount() => _ambientClips?.Length ?? 0;
    public int GetTotalClipCount() => GetTotalSFXCount() + GetTotalMusicCount() + GetTotalAmbientCount();
    
    public int GetAssignedSFXCount()
    {
        if (_sfxClips == null) return 0;
        int count = 0;
        foreach (var clipData in _sfxClips)
        {
            if (clipData.Clip != null) count++;
        }
        return count;
    }
    
    public int GetAssignedMusicCount()
    {
        if (_musicClips == null) return 0;
        int count = 0;
        foreach (var clipData in _musicClips)
        {
            if (clipData.Clip != null) count++;
        }
        return count;
    }
    
    public int GetAssignedAmbientCount()
    {
        if (_ambientClips == null) return 0;
        int count = 0;
        foreach (var clipData in _ambientClips)
        {
            if (clipData.Clip != null) count++;
        }
        return count;
    }
    
    #endregion
}