using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Configuration")]
    [SerializeField] private AudioClipLibrarySO _audioClipLibrary;
    [SerializeField] private AudioMixerGroup _sfxMixerGroup;
    [SerializeField] private AudioMixerGroup _musicMixerGroup;
    [SerializeField] private AudioMixerGroup _ambientMixerGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AudioSource _ambientAudioSource;
    [SerializeField] private int _sfxAudioSourcePoolSize = 10;

    [Header("Volume Settings")]
    [Range(0f, 1f)][SerializeField] private float _masterVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float _sfxVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float _musicVolume = 0.5f;
    [Range(0f, 1f)][SerializeField] private float _ambientVolume = 0.5f;

    [Header("Music Settings")]
    [SerializeField] private float _musicFadeTime = 1f;
    [SerializeField] private bool _musicEnabled = true;
    [SerializeField] private bool _sfxEnabled = true;
    [SerializeField] private bool _ambientEnabled = true;

    // Audio source pools for SFX
    private Queue<AudioSource> _availableSFXSources = new Queue<AudioSource>();
    private List<AudioSource> _activeSFXSources = new List<AudioSource>();

    // Current audio state
    private MusicType _currentMusicType = MusicType.None;
    private AmbientType _currentAmbientType = AmbientType.None;
    private Coroutine _musicFadeCoroutine;

    // Events for audio state changes
    public UnityAction<float> OnMasterVolumeChanged;
    public UnityAction<float> OnSFXVolumeChanged;
    public UnityAction<float> OnMusicVolumeChanged;
    public UnityAction<float> OnAmbientVolumeChanged;
    public UnityAction<MusicType> OnMusicChanged;

    public void Initialize()
    {
        // setup events
        OnMasterVolumeChanged = new UnityEngine.Events.UnityAction<float>((volume) => { });
        OnSFXVolumeChanged = new UnityEngine.Events.UnityAction<float>((volume) => { });
        OnMusicVolumeChanged = new UnityEngine.Events.UnityAction<float>((volume) => { });
        OnAmbientVolumeChanged = new UnityEngine.Events.UnityAction<float>((volume) => { });
        OnMusicChanged = new UnityEngine.Events.UnityAction<MusicType>((type) => { });

        // Validate audio clip library
        if (_audioClipLibrary == null)
        {
            Debug.LogError("AudioClipLibrarySO is not assigned in AudioManager!");
            return;
        }

        // Initialize audio sources
        InitializeAudioSources();

        // Create SFX audio source pool
        CreateSFXAudioSourcePool();

        // Load saved audio settings
        LoadAudioSettings();

        // Apply initial volume settings
        ApplyVolumeSettings();

        Debug.Log("AudioManager initialized successfully");
    }

    private void InitializeAudioSources()
    {
        // Initialize music audio source if not assigned
        if (_musicAudioSource == null)
        {
            GameObject musicSourceObj = new GameObject("MusicAudioSource");
            musicSourceObj.transform.SetParent(transform);
            _musicAudioSource = musicSourceObj.AddComponent<AudioSource>();
        }

        // Configure music audio source
        _musicAudioSource.outputAudioMixerGroup = _musicMixerGroup;
        _musicAudioSource.loop = true;
        _musicAudioSource.playOnAwake = false;

        // Initialize ambient audio source if not assigned
        if (_ambientAudioSource == null)
        {
            GameObject ambientSourceObj = new GameObject("AmbientAudioSource");
            ambientSourceObj.transform.SetParent(transform);
            _ambientAudioSource = ambientSourceObj.AddComponent<AudioSource>();
        }

        // Configure ambient audio source
        _ambientAudioSource.outputAudioMixerGroup = _ambientMixerGroup;
        _ambientAudioSource.loop = true;
        _ambientAudioSource.playOnAwake = false;
    }

    private void CreateSFXAudioSourcePool()
    {
        for (int i = 0; i < _sfxAudioSourcePoolSize; i++)
        {
            GameObject sfxSourceObj = new GameObject($"SFXAudioSource_{i}");
            sfxSourceObj.transform.SetParent(transform);

            AudioSource sfxSource = sfxSourceObj.AddComponent<AudioSource>();
            sfxSource.outputAudioMixerGroup = _sfxMixerGroup;
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;

            _availableSFXSources.Enqueue(sfxSource);
        }
    }

    #region SFX Methods

    public void PlaySFX(SFXType sfxType)
    {
        if (!_sfxEnabled) return;

        AudioClip clip = _audioClipLibrary.GetSFXClip(sfxType);
        if (clip == null)
        {
            Debug.LogWarning($"SFX clip not found for type: {sfxType}");
            return;
        }

        PlaySFXClip(clip, _sfxVolume, 1f);
    }

    public void PlaySFX(SFXType sfxType, float volumeMultiplier)
    {
        if (!_sfxEnabled) return;

        AudioClip clip = _audioClipLibrary.GetSFXClip(sfxType);
        if (clip == null)
        {
            Debug.LogWarning($"SFX clip not found for type: {sfxType}");
            return;
        }

        PlaySFXClip(clip, _sfxVolume * volumeMultiplier, 1f);
    }

    public void PlaySFX(SFXType sfxType, float volumeMultiplier, float pitchMultiplier)
    {
        if (!_sfxEnabled) return;

        AudioClip clip = _audioClipLibrary.GetSFXClip(sfxType);
        if (clip == null)
        {
            Debug.LogWarning($"SFX clip not found for type: {sfxType}");
            return;
        }

        PlaySFXClip(clip, _sfxVolume * volumeMultiplier, pitchMultiplier);
    }

    private void PlaySFXClip(AudioClip clip, float volume, float pitch)
    {
        AudioSource sfxSource = GetAvailableSFXSource();
        if (sfxSource == null)
        {
            Debug.LogWarning("No available SFX audio sources in pool");
            return;
        }

        sfxSource.clip = clip;
        sfxSource.volume = volume * _masterVolume;
        sfxSource.pitch = pitch;
        sfxSource.Play();

        // Start coroutine to return source to pool when finished
        StartCoroutine(ReturnSFXSourceToPool(sfxSource, clip.length / pitch));
    }

    private AudioSource GetAvailableSFXSource()
    {
        if (_availableSFXSources.Count > 0)
        {
            AudioSource source = _availableSFXSources.Dequeue();
            _activeSFXSources.Add(source);
            return source;
        }

        // If no sources available, try to find one that's finished playing
        for (int i = _activeSFXSources.Count - 1; i >= 0; i--)
        {
            if (!_activeSFXSources[i].isPlaying)
            {
                AudioSource source = _activeSFXSources[i];
                _activeSFXSources.RemoveAt(i);
                _activeSFXSources.Add(source);
                return source;
            }
        }

        return null;
    }

    private IEnumerator ReturnSFXSourceToPool(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_activeSFXSources.Contains(source))
        {
            _activeSFXSources.Remove(source);
            _availableSFXSources.Enqueue(source);
        }
    }

    #endregion

    #region Music Methods

    public void PlayMusic(MusicType musicType)
    {
        if (!_musicEnabled || musicType == _currentMusicType) return;

        AudioClip clip = _audioClipLibrary.GetMusicClip(musicType);
        if (clip == null)
        {
            Debug.LogWarning($"Music clip not found for type: {musicType}");
            return;
        }

        if (_musicFadeCoroutine != null)
        {
            StopCoroutine(_musicFadeCoroutine);
        }

        _musicFadeCoroutine = StartCoroutine(FadeMusicTo(clip, musicType));
    }

    public void StopMusic()
    {
        if (_musicFadeCoroutine != null)
        {
            StopCoroutine(_musicFadeCoroutine);
        }

        _musicFadeCoroutine = StartCoroutine(FadeMusicOut());
    }

    private IEnumerator FadeMusicTo(AudioClip newClip, MusicType newMusicType)
    {
        // Fade out current music
        float startVolume = _musicAudioSource.volume;
        for (float t = 0; t < _musicFadeTime; t += Time.deltaTime)
        {
            _musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, t / _musicFadeTime);
            yield return null;
        }

        // Change to new clip
        _musicAudioSource.Stop();
        _musicAudioSource.clip = newClip;
        _currentMusicType = newMusicType;
        _musicAudioSource.Play();

        // Fade in new music
        float targetVolume = _musicVolume * _masterVolume;
        for (float t = 0; t < _musicFadeTime; t += Time.deltaTime)
        {
            _musicAudioSource.volume = Mathf.Lerp(0f, targetVolume, t / _musicFadeTime);
            yield return null;
        }

        _musicAudioSource.volume = targetVolume;
        OnMusicChanged?.Invoke(newMusicType);
        _musicFadeCoroutine = null;
    }

    private IEnumerator FadeMusicOut()
    {
        float startVolume = _musicAudioSource.volume;
        for (float t = 0; t < _musicFadeTime; t += Time.deltaTime)
        {
            _musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, t / _musicFadeTime);
            yield return null;
        }

        _musicAudioSource.Stop();
        _musicAudioSource.volume = 0f;
        _currentMusicType = MusicType.None;
        OnMusicChanged?.Invoke(MusicType.None);
        _musicFadeCoroutine = null;
    }

    #endregion

    #region Ambient Methods

    public void PlayAmbient(AmbientType ambientType)
    {
        if (!_ambientEnabled || ambientType == _currentAmbientType) return;

        AudioClip clip = _audioClipLibrary.GetAmbientClip(ambientType);
        if (clip == null)
        {
            Debug.LogWarning($"Ambient clip not found for type: {ambientType}");
            return;
        }

        _ambientAudioSource.Stop();
        _ambientAudioSource.clip = clip;
        _ambientAudioSource.volume = _ambientVolume * _masterVolume;
        _ambientAudioSource.Play();
        _currentAmbientType = ambientType;
    }

    public void StopAmbient()
    {
        _ambientAudioSource.Stop();
        _currentAmbientType = AmbientType.None;
    }

    #endregion

    #region Volume Control Methods

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        ApplyVolumeSettings();
        SaveAudioSettings();
        OnMasterVolumeChanged?.Invoke(_masterVolume);
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        SaveAudioSettings();
        OnSFXVolumeChanged?.Invoke(_sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume();
        SaveAudioSettings();
        OnMusicVolumeChanged?.Invoke(_musicVolume);
    }

    public void SetAmbientVolume(float volume)
    {
        _ambientVolume = Mathf.Clamp01(volume);
        ApplyAmbientVolume();
        SaveAudioSettings();
        OnAmbientVolumeChanged?.Invoke(_ambientVolume);
    }

    private void ApplyVolumeSettings()
    {
        ApplyMusicVolume();
        ApplyAmbientVolume();
        // SFX volume is applied per-clip when played
    }

    private void ApplyMusicVolume()
    {
        if (_musicAudioSource != null && _musicAudioSource.isPlaying)
        {
            _musicAudioSource.volume = _musicVolume * _masterVolume;
        }
    }

    private void ApplyAmbientVolume()
    {
        if (_ambientAudioSource != null)
        {
            _ambientAudioSource.volume = _ambientVolume * _masterVolume;
        }
    }

    #endregion

    #region Audio Toggle Methods

    public void SetMusicEnabled(bool enabled)
    {
        _musicEnabled = enabled;
        if (!enabled)
        {
            StopMusic();
        }
        SaveAudioSettings();
    }

    public void SetSFXEnabled(bool enabled)
    {
        _sfxEnabled = enabled;
        if (!enabled)
        {
            StopAllSFX();
        }
        SaveAudioSettings();
    }

    public void SetAmbientEnabled(bool enabled)
    {
        _ambientEnabled = enabled;
        if (!enabled)
        {
            StopAmbient();
        }
        SaveAudioSettings();
    }

    private void StopAllSFX()
    {
        foreach (AudioSource source in _activeSFXSources)
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    #endregion

    #region Audio Settings Persistence

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("Audio_MasterVolume", _masterVolume);
        PlayerPrefs.SetFloat("Audio_SFXVolume", _sfxVolume);
        PlayerPrefs.SetFloat("Audio_MusicVolume", _musicVolume);
        PlayerPrefs.SetFloat("Audio_AmbientVolume", _ambientVolume);
        PlayerPrefs.SetInt("Audio_MusicEnabled", _musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Audio_SFXEnabled", _sfxEnabled ? 1 : 0);
        PlayerPrefs.SetInt("Audio_AmbientEnabled", _ambientEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        _masterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1f);
        _sfxVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", 1f);
        _musicVolume = PlayerPrefs.GetFloat("Audio_MusicVolume", 1f);
        _ambientVolume = PlayerPrefs.GetFloat("Audio_AmbientVolume", 1f);
        _musicEnabled = PlayerPrefs.GetInt("Audio_MusicEnabled", 1) == 1;
        _sfxEnabled = PlayerPrefs.GetInt("Audio_SFXEnabled", 1) == 1;
        _ambientEnabled = PlayerPrefs.GetInt("Audio_AmbientEnabled", 1) == 1;
    }

    #endregion

    #region Getters

    public float GetMasterVolume() => _masterVolume;
    public float GetSFXVolume() => _sfxVolume;
    public float GetMusicVolume() => _musicVolume;
    public float GetAmbientVolume() => _ambientVolume;
    public bool IsMusicEnabled() => _musicEnabled;
    public bool IsSFXEnabled() => _sfxEnabled;
    public bool IsAmbientEnabled() => _ambientEnabled;
    public MusicType GetCurrentMusicType() => _currentMusicType;
    public AmbientType GetCurrentAmbientType() => _currentAmbientType;

    #endregion

    #region Unity Lifecycle
    
    void Update()
    {
        // Check for finished SFX sources and return them to pool
        for (int i = _activeSFXSources.Count - 1; i >= 0; i--)
        {
            if (!_activeSFXSources[i].isPlaying)
            {
                AudioSource source = _activeSFXSources[i];
                _activeSFXSources.RemoveAt(i);
                _availableSFXSources.Enqueue(source);
            }
        }
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // Pause all audio when app is paused
            AudioListener.pause = true;
        }
        else
        {
            // Resume audio when app is unpaused
            AudioListener.pause = false;
        }
    }
    
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            AudioListener.pause = true;
        }
        else
        {
            AudioListener.pause = false;
        }
    }
    
    #endregion

}