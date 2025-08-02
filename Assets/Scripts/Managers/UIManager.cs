using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Currency UI")]
    [SerializeField] private TextMeshProUGUI _gritText;
    [SerializeField] private TextMeshProUGUI _favorsText;
    [SerializeField] private Image _gritIcon;
    [SerializeField] private Image _favorsIcon;

    [Header("Progress UI")]
    [SerializeField] private TextMeshProUGUI _distanceText;
    [SerializeField] private TextMeshProUGUI _slopeText;
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private Image _progressFill;
    [SerializeField] private TextMeshProUGUI _progressPercentText;

    [Header("Combo UI")]
    [SerializeField] private GameObject _comboPanel;
    [SerializeField] private TextMeshProUGUI _comboCountText;
    [SerializeField] private TextMeshProUGUI _comboMultiplierText;
    [SerializeField] private Slider _comboTimerSlider;
    [SerializeField] private Image _comboTimerFill;
    [SerializeField] private ParticleSystem _comboParticles;

    [Header("Upgrade UI Panels")]
    [SerializeField] private GameObject _normalUpgradesPanel;
    [SerializeField] private GameObject _prestigeUpgradesPanel;
    [SerializeField] private Transform _normalUpgradeContainer;
    [SerializeField] private Transform _prestigeUpgradeContainer;
    [SerializeField] private GameObject _upgradeButtonPrefab;
    [SerializeField] private GameObject _prestigeUpgradeButtonPrefab;

    [Header("Prestige UI")]
    [SerializeField] private GameObject _prestigePanel;
    [SerializeField] private Button _prestigeButton;
    [SerializeField] private TextMeshProUGUI _prestigeRewardText;
    [SerializeField] private TextMeshProUGUI _prestigeCountText;
    [SerializeField] private TextMeshProUGUI _prestigeThresholdText;
    [SerializeField] private GameObject _prestigeAvailableIndicator;

    [Header("Boulder UI")]
    [SerializeField] private GameObject _boulderClickEffect;
    [SerializeField] private GameObject _criticalHitEffect;
    [SerializeField] private Transform _damageNumberParent;
    [SerializeField] private GameObject _damageNumberPrefab;

    [Header("Notification UI")]
    [SerializeField] private GameObject _notificationPanel;
    [SerializeField] private Transform _notificationContainer;
    [SerializeField] private GameObject _notificationPrefab;
    [SerializeField] private float _notificationDisplayTime = 3f;

    [Header("Menu UI")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _pauseButton;

    [Header("Audio Settings UI")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _ambientVolumeSlider;
    [SerializeField] private Toggle _musicToggle;
    [SerializeField] private Toggle _sfxToggle;
    [SerializeField] private Toggle _ambientToggle;
    [SerializeField] private TextMeshProUGUI _masterVolumeText;
    [SerializeField] private TextMeshProUGUI _sfxVolumeText;
    [SerializeField] private TextMeshProUGUI _musicVolumeText;
    [SerializeField] private TextMeshProUGUI _ambientVolumeText;

    [Header("Animation Settings")]
    [SerializeField] private float _currencyAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve _currencyAnimationCurve;
    [SerializeField] private float _upgradeButtonPulseScale = 1.1f;
    [SerializeField] private float _upgradeButtonPulseDuration = 0.3f;

    // Manager references
    private CurrencyManager _currencyManager;
    private UpgradeManager _upgradeManager;
    private ProgressManager _progressManager;
    private PrestigeManager _prestigeManager;
    private AudioManager _audioManager;

    // UI Element Collections
    private Dictionary<UpgradeType, UpgradeButtonUI> _normalUpgradeButtons = new Dictionary<UpgradeType, UpgradeButtonUI>();
    private Dictionary<PrestigeUpgradeType, PrestigeUpgradeButtonUI> _prestigeUpgradeButtons = new Dictionary<PrestigeUpgradeType, PrestigeUpgradeButtonUI>();

    // Animation tracking
    private Coroutine _gritAnimationCoroutine;
    private Coroutine _favorsAnimationCoroutine;
    private Coroutine _comboTimerCoroutine;

    // Current values for smooth animations
    private int _displayedGrit = 0;
    private int _displayedFavors = 0;

    public void Initialize(CurrencyManager currencyManager, UpgradeManager upgradeManager,
                          ProgressManager progressManager, PrestigeManager prestigeManager,
                          AudioManager audioManager)
    {
        _currencyManager = currencyManager;
        _upgradeManager = upgradeManager;
        _progressManager = progressManager;
        _prestigeManager = prestigeManager;
        _audioManager = audioManager;

        // Initialize UI elements
        InitializeUpgradeButtons();
        InitializeAudioSettingsUI();
        InitializeMenuButtons();

        // Subscribe to all relevant events
        SubscribeToEvents();

        // Set initial UI values
        UpdateInitialUI();

        //Debug.Log("UIManager initialized successfully");
    }

    #region Initialization Methods

    private void InitializeUpgradeButtons()
    {
        // Create normal upgrade buttons
        foreach (UpgradeType upgradeType in System.Enum.GetValues(typeof(UpgradeType)))
        {
            if (_upgradeButtonPrefab != null && _normalUpgradeContainer != null)
            {
                GameObject buttonObj = Instantiate(_upgradeButtonPrefab, _normalUpgradeContainer);
                UpgradeButtonUI buttonUI = buttonObj.GetComponent<UpgradeButtonUI>();
                
                if (buttonUI != null)
                {
                    buttonUI.Initialize(upgradeType, this);
                    _normalUpgradeButtons[upgradeType] = buttonUI;
                }
            }
        }
        
        // Create prestige upgrade buttons
        foreach (PrestigeUpgradeType upgradeType in System.Enum.GetValues(typeof(PrestigeUpgradeType)))
        {
            if (_prestigeUpgradeButtonPrefab != null && _prestigeUpgradeContainer != null)
            {
                GameObject buttonObj = Instantiate(_prestigeUpgradeButtonPrefab, _prestigeUpgradeContainer);
                PrestigeUpgradeButtonUI buttonUI = buttonObj.GetComponent<PrestigeUpgradeButtonUI>();
                
                if (buttonUI != null)
                {
                    buttonUI.Initialize(upgradeType, this);
                    _prestigeUpgradeButtons[upgradeType] = buttonUI;
                }
            }
        }
    }

    private void InitializeAudioSettingsUI()
    {
        if (_audioManager == null) return;
        
        // Initialize volume sliders with current values and set up callbacks
        if (_masterVolumeSlider != null)
        {
            _masterVolumeSlider.value = _audioManager.GetMasterVolume();
            _masterVolumeSlider.onValueChanged.AddListener(_audioManager.SetMasterVolume);
            UpdateVolumeDisplayText(_masterVolumeText, _audioManager.GetMasterVolume());
        }
        
        if (_sfxVolumeSlider != null)
        {
            _sfxVolumeSlider.value = _audioManager.GetSFXVolume();
            _sfxVolumeSlider.onValueChanged.AddListener(_audioManager.SetSFXVolume);
            UpdateVolumeDisplayText(_sfxVolumeText, _audioManager.GetSFXVolume());
        }
        
        if (_musicVolumeSlider != null)
        {
            _musicVolumeSlider.value = _audioManager.GetMusicVolume();
            _musicVolumeSlider.onValueChanged.AddListener(_audioManager.SetMusicVolume);
            UpdateVolumeDisplayText(_musicVolumeText, _audioManager.GetMusicVolume());
        }
        
        if (_ambientVolumeSlider != null)
        {
            _ambientVolumeSlider.value = _audioManager.GetAmbientVolume();
            _ambientVolumeSlider.onValueChanged.AddListener(_audioManager.SetAmbientVolume);
            UpdateVolumeDisplayText(_ambientVolumeText, _audioManager.GetAmbientVolume());
        }
        
        // Initialize audio toggles with current values and set up callbacks
        if (_musicToggle != null)
        {
            _musicToggle.isOn = _audioManager.IsMusicEnabled();
            _musicToggle.onValueChanged.AddListener(_audioManager.SetMusicEnabled);
        }
        
        if (_sfxToggle != null)
        {
            _sfxToggle.isOn = _audioManager.IsSFXEnabled();
            _sfxToggle.onValueChanged.AddListener(_audioManager.SetSFXEnabled);
        }
        
        if (_ambientToggle != null)
        {
            _ambientToggle.isOn = _audioManager.IsAmbientEnabled();
            _ambientToggle.onValueChanged.AddListener(_audioManager.SetAmbientEnabled);
        }
    }

    private void InitializeMenuButtons()
    {
        if (_settingsButton != null)
        {
            _settingsButton.onClick.AddListener(ToggleSettingsMenu);
        }

        if (_pauseButton != null)
        {
            _pauseButton.onClick.AddListener(TogglePauseMenu);
        }

        if (_prestigeButton != null)
        {
            _prestigeButton.onClick.AddListener(HandlePrestigeButtonClick);
        }
    }

    private void UpdateInitialUI()
    {
        // Set initial currency values
        if (_currencyManager != null)
        {
            _displayedGrit = _currencyManager.GetGrit();
            _displayedFavors = _currencyManager.GetFavors();
            
            if (_gritText != null) _gritText.text = FormatCurrency(_displayedGrit);
            if (_favorsText != null) _favorsText.text = FormatCurrency(_displayedFavors);
        }
        
        // Set initial progress values
        if (_progressManager != null)
        {
            HandleDistanceChanged(_progressManager.GetCurrentDistance());
            HandleSlopeChanged(_progressManager.GetCurrentSlope());
            HandleComboStateChanged(_progressManager.GetComboState());
        }
        
        // Update upgrade buttons
        UpdateAllUpgradeButtonCosts();
        UpdateAllPrestigeUpgradeButtonCosts();
        
        // Update prestige UI
        UpdatePrestigeButton();
        UpdatePrestigeCount();
    }

    #endregion

    #region Event Subscription

    private void SubscribeToEvents()
    {
        // Currency events
        _currencyManager.OnGritChanged += HandleGritChanged;
        _currencyManager.OnFavorsChanged += HandleFavorsChanged;

        // Upgrade events
        _upgradeManager.OnNormalUpgradePurchased += HandleNormalUpgradePurchased;
        _upgradeManager.OnPrestigeUpgradePurchased += HandlePrestigeUpgradePurchased;
        _upgradeManager.OnUpgradeUnlocked += HandleUpgradeUnlocked;

        // Progress events
        _progressManager.OnDistanceChanged += HandleDistanceChanged;
        _progressManager.OnSlopeChanged += HandleSlopeChanged;
        _progressManager.OnComboStateChanged += HandleComboStateChanged;
        _progressManager.OnCriticalHit += HandleCriticalHit;

        // Prestige events
        _prestigeManager.OnPrestigeAvailable += HandlePrestigeAvailable;
        _prestigeManager.OnPrestigeCompleted += HandlePrestigeCompleted;
        _prestigeManager.OnPrestigeRewardCalculated += HandlePrestigeRewardCalculated;

        // Input events
        if (GameManager.Instance.InputManager != null)
        {
            GameManager.Instance.InputManager.OnBoulderClicked += HandleBoulderClicked;
        }

        // Audio events
        if (_audioManager != null)
        {
            _audioManager.OnMasterVolumeChanged += HandleMasterVolumeChanged;
            _audioManager.OnSFXVolumeChanged += HandleSFXVolumeChanged;
            _audioManager.OnMusicVolumeChanged += HandleMusicVolumeChanged;
            _audioManager.OnAmbientVolumeChanged += HandleAmbientVolumeChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        // Currency events
        if (_currencyManager != null)
        {
            _currencyManager.OnGritChanged -= HandleGritChanged;
            _currencyManager.OnFavorsChanged -= HandleFavorsChanged;
        }

        // Upgrade events
        if (_upgradeManager != null)
        {
            _upgradeManager.OnNormalUpgradePurchased -= HandleNormalUpgradePurchased;
            _upgradeManager.OnPrestigeUpgradePurchased -= HandlePrestigeUpgradePurchased;
            _upgradeManager.OnUpgradeUnlocked -= HandleUpgradeUnlocked;
        }

        // Progress events
        if (_progressManager != null)
        {
            _progressManager.OnDistanceChanged -= HandleDistanceChanged;
            _progressManager.OnSlopeChanged -= HandleSlopeChanged;
            _progressManager.OnComboStateChanged -= HandleComboStateChanged;
            _progressManager.OnCriticalHit -= HandleCriticalHit;
        }

        // Prestige events
        if (_prestigeManager != null)
        {
            _prestigeManager.OnPrestigeAvailable -= HandlePrestigeAvailable;
            _prestigeManager.OnPrestigeCompleted -= HandlePrestigeCompleted;
            _prestigeManager.OnPrestigeRewardCalculated -= HandlePrestigeRewardCalculated;
        }

        // Input events
        if (GameManager.Instance?.InputManager != null)
        {
            GameManager.Instance.InputManager.OnBoulderClicked -= HandleBoulderClicked;
        }

        // Audio events
        if (_audioManager != null)
        {
            _audioManager.OnMasterVolumeChanged -= HandleMasterVolumeChanged;
            _audioManager.OnSFXVolumeChanged -= HandleSFXVolumeChanged;
            _audioManager.OnMusicVolumeChanged -= HandleMusicVolumeChanged;
            _audioManager.OnAmbientVolumeChanged -= HandleAmbientVolumeChanged;
        }
    }

    #endregion

    #region Currency Event Handlers

    private void HandleGritChanged(int newGrit)
    {
        if (_gritAnimationCoroutine != null)
        {
            StopCoroutine(_gritAnimationCoroutine);
        }

        _gritAnimationCoroutine = StartCoroutine(AnimateCurrencyChange(_gritText, _displayedGrit, newGrit,
            (value) => _displayedGrit = value));

        // Update upgrade button affordability
        UpdateUpgradeButtonAffordability();

        // Play currency gain effect if increased
        if (newGrit > _displayedGrit)
        {
            PlayCurrencyGainEffect(_gritIcon.transform, newGrit - _displayedGrit);
        }
    }

    private void HandleFavorsChanged(int newFavors)
    {
        if (_favorsAnimationCoroutine != null)
        {
            StopCoroutine(_favorsAnimationCoroutine);
        }

        _favorsAnimationCoroutine = StartCoroutine(AnimateCurrencyChange(_favorsText, _displayedFavors, newFavors,
            (value) => _displayedFavors = value));

        // Update prestige upgrade button affordability
        UpdatePrestigeUpgradeButtonAffordability();

        // Play currency gain effect if increased
        if (newFavors > _displayedFavors)
        {
            PlayCurrencyGainEffect(_favorsIcon.transform, newFavors - _displayedFavors);
        }
    }

    #endregion

    #region Upgrade Event Handlers

    private void HandleNormalUpgradePurchased(UpgradeType upgradeType, int newLevel)
    {
        if (_normalUpgradeButtons.TryGetValue(upgradeType, out UpgradeButtonUI buttonUI))
        {
            buttonUI.UpdateLevel(newLevel);
            buttonUI.PlayPurchaseAnimation();
        }

        // Update all upgrade costs and affordability
        UpdateAllUpgradeButtonCosts();

        // Show notification
        string upgradeName = upgradeType.ToString();
        ShowNotification($"{upgradeName} upgraded to level {newLevel}!", NotificationType.Upgrade);
    }

    private void HandlePrestigeUpgradePurchased(PrestigeUpgradeType upgradeType, int newLevel)
    {
        if (_prestigeUpgradeButtons.TryGetValue(upgradeType, out PrestigeUpgradeButtonUI buttonUI))
        {
            buttonUI.UpdateLevel(newLevel);
            buttonUI.PlayPurchaseAnimation();
        }

        // Update all prestige upgrade costs and affordability
        UpdateAllPrestigeUpgradeButtonCosts();

        // Show notification
        string upgradeName = upgradeType.ToString();
        ShowNotification($"{upgradeName} upgraded to level {newLevel}!", NotificationType.PrestigeUpgrade);
    }

    private void HandleUpgradeUnlocked(UpgradeType upgradeType, bool unlocked)
    {
        if (_normalUpgradeButtons.TryGetValue(upgradeType, out UpgradeButtonUI buttonUI))
        {
            buttonUI.SetUnlocked(unlocked);

            if (unlocked)
            {
                buttonUI.PlayUnlockAnimation();
                string upgradeName = upgradeType.ToString();
                ShowNotification($"New upgrade unlocked: {upgradeName}!", NotificationType.Unlock);
            }
        }
    }

    #endregion

    #region Progress Event Handlers

    private void HandleDistanceChanged(float newDistance)
    {
        if (_distanceText != null)
        {
            _distanceText.text = $"Distance: {newDistance:F1}m";
        }

        // Update progress slider if using percentage-based progress
        UpdateProgressSlider(newDistance);

        // Update prestige availability
        UpdatePrestigeButton();
    }

    private void HandleSlopeChanged(float newSlope)
    {
        if (_slopeText != null)
        {
            _slopeText.text = $"Slope: {newSlope:F2}x";

            // Color-code slope difficulty
            Color slopeColor = GetSlopeColor(newSlope);
            _slopeText.color = slopeColor;
        }
    }

    private void HandleComboStateChanged(ComboState comboState)
    {
        if (_comboPanel != null)
        {
            _comboPanel.SetActive(comboState.IsActive);

            if (comboState.IsActive)
            {
                if (_comboCountText != null)
                {
                    _comboCountText.text = $"Combo: {comboState.CurrentCombo}";
                }

                if (_comboMultiplierText != null)
                {
                    _comboMultiplierText.text = $"{comboState.Multiplier:F1}x";
                }

                // Start combo timer animation
                if (_comboTimerCoroutine != null)
                {
                    StopCoroutine(_comboTimerCoroutine);
                }
                _comboTimerCoroutine = StartCoroutine(AnimateComboTimer(comboState.ComboTimer));

                // Play combo particles
                if (_comboParticles != null && !_comboParticles.isPlaying)
                {
                    _comboParticles.Play();
                }
            }
            else
            {
                // Stop combo particles
                if (_comboParticles != null && _comboParticles.isPlaying)
                {
                    _comboParticles.Stop();
                }
            }
        }
    }

    private void HandleCriticalHit(bool isCritical)
    {
        if (isCritical && _criticalHitEffect != null)
        {
            // Play critical hit visual effect
            GameObject critEffect = Instantiate(_criticalHitEffect, _boulderClickEffect.transform.position, Quaternion.identity);
            Destroy(critEffect, 2f);

            // Show floating damage number
            ShowDamageNumber("CRITICAL!", Color.yellow, 1.5f);
        }
    }

    #endregion

    #region Prestige Event Handlers

    private void HandlePrestigeAvailable()
    {
        if (_prestigeAvailableIndicator != null)
        {
            _prestigeAvailableIndicator.SetActive(true);
        }

        UpdatePrestigeButton();
        ShowNotification("Prestige is now available!", NotificationType.Prestige);
    }

    private void HandlePrestigeCompleted(int favorsEarned)
    {
        // Hide prestige indicator
        if (_prestigeAvailableIndicator != null)
        {
            _prestigeAvailableIndicator.SetActive(false);
        }

        // Update prestige count
        UpdatePrestigeCount();

        // Reset normal upgrade buttons
        ResetNormalUpgradeButtons();

        // Show prestige completion notification
        ShowNotification($"Prestige completed! Earned {favorsEarned} Favors!", NotificationType.Prestige);

        // Play prestige completion effect
        PlayPrestigeCompletionEffect();
    }

    private void HandlePrestigeRewardCalculated(int favors)
    {
        if (_prestigeRewardText != null)
        {
            _prestigeRewardText.text = $"Reward: {favors} Favors";
        }
    }

    #endregion

    #region Input Event Handlers

    private void HandleBoulderClicked()
    {
        if (_boulderClickEffect != null)
        {
            // Play boulder click visual effect
            GameObject clickEffect = Instantiate(_boulderClickEffect, _boulderClickEffect.transform.position, Quaternion.identity);
            Destroy(clickEffect, 1f);
        }
    }

    #endregion

    #region Audio Event Handlers

    private void HandleMasterVolumeChanged(float volume)
    {
        // Update slider only if volume changed from non-UI source (keyboard shortcut, etc.)
        if (_masterVolumeSlider != null && Mathf.Abs(_masterVolumeSlider.value - volume) > 0.01f)
        {
            _masterVolumeSlider.value = volume;
        }

        // Update numerical display text if it exists
        UpdateVolumeDisplayText(_masterVolumeText, volume);
    }

    private void HandleSFXVolumeChanged(float volume)
    {
        if (_sfxVolumeSlider != null && Mathf.Abs(_sfxVolumeSlider.value - volume) > 0.01f)
        {
            _sfxVolumeSlider.value = volume;
        }

        UpdateVolumeDisplayText(_sfxVolumeText, volume);
    }

    private void HandleMusicVolumeChanged(float volume)
    {
        if (_musicVolumeSlider != null && Mathf.Abs(_musicVolumeSlider.value - volume) > 0.01f)
        {
            _musicVolumeSlider.value = volume;
        }

        UpdateVolumeDisplayText(_musicVolumeText, volume);
    }

    private void HandleAmbientVolumeChanged(float volume)
    {
        if (_ambientVolumeSlider != null && Mathf.Abs(_ambientVolumeSlider.value - volume) > 0.01f)
        {
            _ambientVolumeSlider.value = volume;
        }

        UpdateVolumeDisplayText(_ambientVolumeText, volume);
    }

    private void UpdateVolumeDisplayText(TextMeshProUGUI textComponent, float volume)
    {
        if (textComponent != null)
        {
            textComponent.text = $"{Mathf.RoundToInt(volume * 100)}%";
        }
    }

    #endregion

    #region Menu Methods

    public void ToggleSettingsMenu()
    {
        if (_settingsPanel != null)
        {
            bool isActive = _settingsPanel.activeSelf;
            _settingsPanel.SetActive(!isActive);

            if (!isActive)
            {
                _audioManager?.PlaySFX(SFXType.MenuOpen);
            }
            else
            {
                _audioManager?.PlaySFX(SFXType.MenuClose);
            }
        }
    }

    public void TogglePauseMenu()
    {
        if (_pausePanel != null)
        {
            bool isActive = _pausePanel.activeSelf;
            _pausePanel.SetActive(!isActive);

            // Pause/unpause game
            Time.timeScale = isActive ? 1f : 0f;

            if (!isActive)
            {
                _audioManager?.PlaySFX(SFXType.MenuOpen);
            }
            else
            {
                _audioManager?.PlaySFX(SFXType.MenuClose);
            }
        }
    }

    public void ShowMainMenu()
    {
        if (_mainMenuPanel != null)
        {
            _mainMenuPanel.SetActive(true);
        }
    }

    public void HideMainMenu()
    {
        if (_mainMenuPanel != null)
        {
            _mainMenuPanel.SetActive(false);
        }
    }

    private void HandlePrestigeButtonClick()
    {
        if (_prestigeManager != null && _prestigeManager.CanPrestige())
        {
            _prestigeManager.ExecutePrestige();
        }
    }

    #endregion

    #region UI Update Methods

    private void UpdateUpgradeButtonAffordability()
    {
        int currentGrit = _currencyManager.GetGrit();
        
        foreach (var kvp in _normalUpgradeButtons)
        {
            UpgradeType upgradeType = kvp.Key;
            UpgradeButtonUI buttonUI = kvp.Value;
            
            int cost = CalculateUpgradeCost(upgradeType);
            bool canAfford = currentGrit >= cost;
            buttonUI.SetAffordable(canAfford);
        }
    }
    
    private void UpdatePrestigeUpgradeButtonAffordability()
    {
        int currentFavors = _currencyManager.GetFavors();
        
        foreach (var kvp in _prestigeUpgradeButtons)
        {
            PrestigeUpgradeType upgradeType = kvp.Key;
            PrestigeUpgradeButtonUI buttonUI = kvp.Value;
            
            int cost = CalculatePrestigeUpgradeCost(upgradeType);
            bool canAfford = currentFavors >= cost;
            buttonUI.SetAffordable(canAfford);
        }
    }
    
    private void UpdateAllUpgradeButtonCosts()
    {
        foreach (var kvp in _normalUpgradeButtons)
        {
            UpgradeType upgradeType = kvp.Key;
            UpgradeButtonUI buttonUI = kvp.Value;
            
            int cost = CalculateUpgradeCost(upgradeType);
            int level = _upgradeManager.GetNormalUpgradeLevel(upgradeType);
            
            buttonUI.UpdateCost(cost);
            buttonUI.UpdateLevel(level);
        }
        
        UpdateUpgradeButtonAffordability();
    }
    
    private void UpdateAllPrestigeUpgradeButtonCosts()
    {
        foreach (var kvp in _prestigeUpgradeButtons)
        {
            PrestigeUpgradeType upgradeType = kvp.Key;
            PrestigeUpgradeButtonUI buttonUI = kvp.Value;
            
            int cost = CalculatePrestigeUpgradeCost(upgradeType);
            int level = _upgradeManager.GetPrestigeUpgradeLevel(upgradeType);
            
            buttonUI.UpdateCost(cost);
            buttonUI.UpdateLevel(level);
        }
        
        UpdatePrestigeUpgradeButtonAffordability();
    }
    
    private void UpdatePrestigeButton()
    {
        if (_prestigeButton != null && _prestigeManager != null)
        {
            bool canPrestige = _prestigeManager.CanPrestige();
            _prestigeButton.interactable = canPrestige;
            
            if (canPrestige)
            {
                int reward = _prestigeManager.CalculatePrestigeReward();
                if (_prestigeRewardText != null)
                {
                    _prestigeRewardText.text = $"Reward: {reward} Favors";
                }
            }
        }
        
        if (_prestigeThresholdText != null && _prestigeManager != null)
        {
            float threshold = _prestigeManager.GetPrestigeThreshold();
            _prestigeThresholdText.text = $"Threshold: {threshold:F0}m";
        }
    }
    
    private void UpdatePrestigeCount()
    {
        if (_prestigeCountText != null && _prestigeManager != null)
        {
            int count = _prestigeManager.GetPrestigeCount();
            _prestigeCountText.text = $"Prestiges: {count}";
        }
    }
    
    private void UpdateProgressSlider(float distance)
    {
        if (_progressSlider != null && _prestigeManager != null)
        {
            float threshold = _prestigeManager.GetPrestigeThreshold();
            float progress = Mathf.Clamp01(distance / threshold);
            _progressSlider.value = progress;
            
            // Update progress fill color based on progress
            if (_progressFill != null)
            {
                Color fillColor = GetProgressFillColor(progress);
                _progressFill.color = fillColor;
            }
            
            if (_progressPercentText != null)
            {
                _progressPercentText.text = $"{progress * 100f:F1}%";
            }
        }
    }

    #endregion

    #region UI Animation Methods

    private IEnumerator AnimateCurrencyChange(TextMeshProUGUI textComponent, int startValue, int endValue, System.Action<int> onValueUpdate)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < _currencyAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / _currencyAnimationDuration;
            
            if (_currencyAnimationCurve != null)
            {
                progress = _currencyAnimationCurve.Evaluate(progress);
            }
            
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, progress));
            onValueUpdate(currentValue);
            
            if (textComponent != null)
            {
                textComponent.text = FormatCurrency(currentValue);
            }
            
            yield return null;
        }
        
        // Ensure final value is set
        onValueUpdate(endValue);
        if (textComponent != null)
        {
            textComponent.text = FormatCurrency(endValue);
        }
    }
    
    private IEnumerator AnimateComboTimer(float maxTime)
    {
        float currentTime = maxTime;
        
        while (currentTime > 0f && _comboTimerSlider != null)
        {
            currentTime -= Time.deltaTime;
            float progress = currentTime / maxTime;
            
            _comboTimerSlider.value = progress;
            
            // Color the timer fill based on remaining time
            if (_comboTimerFill != null)
            {
                Color timerColor = Color.Lerp(Color.red, Color.green, progress);
                _comboTimerFill.color = timerColor;
            }
            
            yield return null;
        }
    }
    
    private void PlayCurrencyGainEffect(Transform iconTransform, int amount)
    {
        // Simple scale animation for currency icons
        if (iconTransform != null)
        {
            StartCoroutine(PulseEffect(iconTransform, _upgradeButtonPulseScale, _upgradeButtonPulseDuration));
        }
    }
    
    private IEnumerator PulseEffect(Transform target, float scale, float duration)
    {
        Vector3 originalScale = target.localScale;
        Vector3 targetScale = originalScale * scale;
        
        // Scale up
        float elapsedTime = 0f;
        while (elapsedTime < duration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (duration / 2f);
            target.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        // Scale down
        elapsedTime = 0f;
        while (elapsedTime < duration / 2f)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / (duration / 2f);
            target.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        target.localScale = originalScale;
    }

    #endregion

    #region Notification System

    public void ShowNotification(string message, NotificationType type)
    {
        if (_notificationPrefab != null && _notificationContainer != null)
        {
            GameObject notificationObj = Instantiate(_notificationPrefab, _notificationContainer);
            NotificationUI notification = notificationObj.GetComponent<NotificationUI>();

            if (notification != null)
            {
                notification.Initialize(message, type, _notificationDisplayTime);
            }

            // Auto-destroy after display time
            Destroy(notificationObj, _notificationDisplayTime + 1f);
        }
    }

    public void ShowDamageNumber(string text, Color color, float scale = 1f)
    {
        if (_damageNumberPrefab != null && _damageNumberParent != null)
        {
            GameObject damageNumberObj = Instantiate(_damageNumberPrefab, _damageNumberParent);
            DamageNumberUI damageNumber = damageNumberObj.GetComponent<DamageNumberUI>();

            if (damageNumber != null)
            {
                damageNumber.Initialize(text, color, scale);
            }

            // Auto-destroy after animation
            Destroy(damageNumberObj, 2f);
        }
    }

    #endregion

    #region Special Effects

    private void PlayPrestigeCompletionEffect()
    {
        // Play screen flash effect
        StartCoroutine(ScreenFlashEffect());

        // Play particle effects
        if (_comboParticles != null)
        {
            _comboParticles.Play();
        }

        // Reset all upgrade button animations
        foreach (var buttonUI in _normalUpgradeButtons.Values)
        {
            buttonUI.PlayResetAnimation();
        }
    }

    private IEnumerator ScreenFlashEffect()
    {
        // Create a white overlay that fades out
        GameObject flashOverlay = new GameObject("FlashOverlay");
        Canvas canvas = flashOverlay.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        Image flashImage = flashOverlay.AddComponent<Image>();
        flashImage.color = Color.white;
        flashImage.rectTransform.anchorMin = Vector2.zero;
        flashImage.rectTransform.anchorMax = Vector2.one;
        flashImage.rectTransform.offsetMin = Vector2.zero;
        flashImage.rectTransform.offsetMax = Vector2.zero;

        // Fade out the flash
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            flashImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        Destroy(flashOverlay);
    }

    private void ResetNormalUpgradeButtons()
    {
        foreach (var buttonUI in _normalUpgradeButtons.Values)
        {
            buttonUI.UpdateLevel(0);
            buttonUI.PlayResetAnimation();
        }
    }

    #endregion

    #region Panel Management
    
    public void ShowPanel(UIPanel panelType)
    {
        switch (panelType)
        {
            case UIPanel.NormalUpgrades:
                if (_normalUpgradesPanel != null) _normalUpgradesPanel.SetActive(true);
                break;
            case UIPanel.PrestigeUpgrades:
                if (_prestigeUpgradesPanel != null) _prestigeUpgradesPanel.SetActive(true);
                break;
            case UIPanel.Prestige:
                if (_prestigePanel != null) _prestigePanel.SetActive(true);
                break;
            case UIPanel.Settings:
                if (_settingsPanel != null) _settingsPanel.SetActive(true);
                break;
        }
        
        _audioManager?.PlaySFX(SFXType.MenuOpen);
    }
    
    public void HidePanel(UIPanel panelType)
    {
        switch (panelType)
        {
            case UIPanel.NormalUpgrades:
                if (_normalUpgradesPanel != null) _normalUpgradesPanel.SetActive(false);
                break;
            case UIPanel.PrestigeUpgrades:
                if (_prestigeUpgradesPanel != null) _prestigeUpgradesPanel.SetActive(false);
                break;
            case UIPanel.Prestige:
                if (_prestigePanel != null) _prestigePanel.SetActive(false);
                break;
            case UIPanel.Settings:
                if (_settingsPanel != null) _settingsPanel.SetActive(false);
                break;
        }
        
        _audioManager?.PlaySFX(SFXType.MenuClose);
    }
    
    public void TogglePanel(UIPanel panelType)
    {
        bool isActive = false;
        
        switch (panelType)
        {
            case UIPanel.NormalUpgrades:
                isActive = _normalUpgradesPanel != null && _normalUpgradesPanel.activeSelf;
                break;
            case UIPanel.PrestigeUpgrades:
                isActive = _prestigeUpgradesPanel != null && _prestigeUpgradesPanel.activeSelf;
                break;
            case UIPanel.Prestige:
                isActive = _prestigePanel != null && _prestigePanel.activeSelf;
                break;
            case UIPanel.Settings:
                isActive = _settingsPanel != null && _settingsPanel.activeSelf;
                break;
        }
        
        if (isActive)
        {
            HidePanel(panelType);
        }
        else
        {
            ShowPanel(panelType);
        }
    }
    
    #endregion

    #region Helper Methods
    
    private string FormatCurrency(int amount)
    {
        if (amount >= 1000000)
        {
            return $"{amount / 1000000f:F1}M";
        }
        else if (amount >= 1000)
        {
            return $"{amount / 1000f:F1}K";
        }
        return amount.ToString();
    }
    
    private Color GetSlopeColor(float slope)
    {
        if (slope <= 15f) return Color.green;
        if (slope <= 30f) return Color.yellow;
        if (slope <= 45f) return new Color(1f, 0.5f, 0f); // Orange (RGB: 255, 128, 0)
        return Color.red;
    }

    private Color GetProgressFillColor(float progress)
    {
        // Gradient from red to yellow to green as progress increases
        if (progress < 0.5f)
        {
            // Red to yellow (0% to 50%)
            return Color.Lerp(Color.red, Color.yellow, progress * 2f);
        }
        else
        {
            // Yellow to green (50% to 100%)
            return Color.Lerp(Color.yellow, Color.green, (progress - 0.5f) * 2f);
        }
    }
    
    private int CalculateUpgradeCost(UpgradeType upgradeType)
    {
        // This should delegate to UpgradeManager's cost calculation
        // For now, using placeholder logic
        int currentLevel = _upgradeManager.GetNormalUpgradeLevel(upgradeType);
        return Mathf.RoundToInt(10 * Mathf.Pow(1.5f, currentLevel));
    }
    
    private int CalculatePrestigeUpgradeCost(PrestigeUpgradeType upgradeType)
    {
        // This should delegate to UpgradeManager's cost calculation
        // For now, using placeholder logic
        int currentLevel = _upgradeManager.GetPrestigeUpgradeLevel(upgradeType);
        return 1 + (currentLevel * 1);
    }

    // Public methods for button UI components to call
    public bool TryPurchaseNormalUpgrade(UpgradeType upgradeType)
    {
        return _upgradeManager.PurchaseNormalUpgrade(upgradeType);
    }

    public bool TryPurchasePrestigeUpgrade(PrestigeUpgradeType upgradeType)
    {
        return _upgradeManager.PurchasePrestigeUpgrade(upgradeType);
    }
    
    #endregion

    #region Unity Lifecycle
    
    void Update()
    {
        // Update combo timer if active
        if (_progressManager != null)
        {
            ComboState comboState = _progressManager.GetComboState();
            if (comboState.IsActive && _comboTimerSlider != null)
            {
                float progress = comboState.ComboTimer / 5f; // Assuming 5 second combo timer
                _comboTimerSlider.value = progress;
            }
        }
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
        
        // Clean up any running coroutines
        if (_gritAnimationCoroutine != null) StopCoroutine(_gritAnimationCoroutine);
        if (_favorsAnimationCoroutine != null) StopCoroutine(_favorsAnimationCoroutine);
        if (_comboTimerCoroutine != null) StopCoroutine(_comboTimerCoroutine);
    }
    
    #endregion
}