using UnityEngine;
using UnityEngine.Events;

public class ProgressManager : MonoBehaviour
{
    [Header("Progress Values")]
    [SerializeField] private float _currentDistance = 0f;
    [SerializeField] private float _currentSlope = 1f;
    [SerializeField] private ComboState _comboState = new ComboState();
    [SerializeField] private float _maxComboTime = 5f; 

    [Header("Push Configuration")]
    [SerializeField] private float _basePushPower = 1f;
    [SerializeField] private float _slopeIncreaseRate = 6f;
    [SerializeField] private float _slopeIncreaseInterval = 10000f;

    private UpgradeManager _upgradeManager;

    public UnityAction<float> OnDistanceChanged;
    public UnityAction<float> OnSlopeChanged;
    public UnityAction<ComboState> OnComboStateChanged;
    public UnityAction<bool> OnCriticalHit;

    // public getters for private values/data
    public float GetCurrentDistance() => _currentDistance;
    public float GetCurrentSlope() => _currentSlope;
    public ComboState GetComboState() => _comboState;
    public float GetMaxComboTime() => _maxComboTime;

    public void Initialize(UpgradeManager upgradeManager)
    {
        _upgradeManager = upgradeManager;

        // setup events
        OnDistanceChanged = new UnityAction<float>((distance) => { });
        OnSlopeChanged = new UnityAction<float>((slope) => { });
        OnComboStateChanged = new UnityAction<ComboState>((combo) => { });
        OnCriticalHit = new UnityAction<bool>((isCrit) => { });

        // initialize combo state
        _comboState = new ComboState
        {
            CurrentCombo = 0,
            ComboTimer = 0f,
            SpeedThreshold = 1f,
            Multiplier = 1f,
            IsActive = false
        };
    }

    public void SetDefaultValues()
    {
        _currentDistance = 0f;
        _currentSlope = 1f;
        _comboState = new ComboState
        {
            CurrentCombo = 0,
            ComboTimer = 0f,
            SpeedThreshold = 1f,
            Multiplier = 1f,
            IsActive = false
        };

        NotifyDistanceChanged();
        NotifySlopeChanged();
        NotifyComboStateChanged();
    }

    public void LoadFromSaveData(SaveData saveData)
    {
        _currentDistance = saveData.currentDistance;
        _currentSlope = saveData.currentSlope;
        _comboState = saveData.comboState ?? new ComboState();

        NotifyDistanceChanged();
        NotifySlopeChanged();
        NotifyComboStateChanged();
    }

    public void ManualPush()
    {
        float pushPower = CalculateManualPushPower();
        ProcessPush(pushPower, true);
    }

    public void AutoPush()
    {
        float pushPower = CalculateAutoPushPower();
        ProcessPush(pushPower, false);
    }

    private void ProcessPush(float pushPower, bool isManualPush)
    {
        // Apply slope resistance
        float effectivePower = pushPower / _currentSlope;

        // Check for critical hit
        bool isCritical = RollForCritical();
        if (isCritical)
        {
            effectivePower *= _upgradeManager.GetCriticalMultiplier();
            OnCriticalHit?.Invoke(true);
        }

        // Update combo, regardless of manual or auto push
        UpdateComboState(effectivePower);

        // Apply combo multiplier
        effectivePower *= GetCurrentComboMultiplier();

        // Add distance
        float previousDistance = _currentDistance;
        _currentDistance += effectivePower;

        // Update slope based on distance
        UpdateSlope();

        // Award Grit based on distance progressed
        int gritEarned = CalculateGritReward(effectivePower);
        GameManager.Instance.CurrencyManager.AddGrit(gritEarned);

        NotifyDistanceChanged();
    }

    private float CalculateManualPushPower()
    {
        float basePower = _basePushPower;
        float upgradePower = _upgradeManager.GetManualClickPower();

        // Apply prestige bonuses
        float prestigeMultiplier = CalculatePrestigeMultiplier();

        return (basePower + upgradePower) * prestigeMultiplier;
    }

    private float CalculateAutoPushPower()
    {
        float upgradePower = _upgradeManager.GetAutoClickPower();

        // Apply prestige bonuses
        float prestigeMultiplier = CalculatePrestigeMultiplier();

        return upgradePower * prestigeMultiplier;
    }

    private float CalculatePrestigeMultiplier()
    {
        float multiplier = 1f;

        // Might of Hercules - flat power multiplier
        int herculesLevel = _upgradeManager.GetPrestigeUpgradeLevel(PrestigeUpgradeType.MightOfHercules);
        multiplier += herculesLevel * 0.5f; // 50% per level

        // Other prestige bonuses can be added here

        return multiplier;
    }

    private bool RollForCritical()
    {
        float critChance = _upgradeManager.GetCriticalChance();
        return UnityEngine.Random.Range(0f, 100f) < critChance;
    }

    private void UpdateComboState(float pushPower)
    {
        float speedThreshold = 15f; // Base threshold, can be modified by upgrades

        // maybe instead of pushPower here we use collective manual + auto push power over the past second
        if (pushPower >= speedThreshold)
        {
            _comboState.CurrentCombo++;
            _comboState.ComboTimer = _maxComboTime; // Reset timer
            _comboState.IsActive = true;
            _comboState.Multiplier = CalculateComboMultiplier();
        }

        NotifyComboStateChanged();
    }

    private float CalculateComboMultiplier()
    {
        float baseMultiplier = _upgradeManager.GetMomentumMultiplier();
        return baseMultiplier + (_comboState.CurrentCombo * 0.1f); // 10% per combo level
    }

    private float GetCurrentComboMultiplier()
    {
        return _comboState.IsActive ? _comboState.Multiplier : 1f;
    }

    private void ResetCombo()
    {
        _comboState.CurrentCombo = 0;
        _comboState.ComboTimer = 0f;
        _comboState.IsActive = false;
        _comboState.Multiplier = 1f;
        NotifyComboStateChanged();
    }

    // DIFFICULTY CURVE
    private void UpdateSlope()
    {
        float targetSlope = 1f + (_currentDistance / _slopeIncreaseInterval) * _slopeIncreaseRate;

        // Apply prestige bonus for slope reduction
        int gaiaLevel = _upgradeManager.GetPrestigeUpgradeLevel(PrestigeUpgradeType.BlessingOfGaia);
        float slopeReduction = gaiaLevel * 0.05f; // 5% reduction per level
        targetSlope *= (1f - slopeReduction);

        if (Mathf.Abs(targetSlope - _currentSlope) > 0.01f)
        {
            _currentSlope = Mathf.Lerp(_currentSlope, targetSlope, Time.deltaTime);
            NotifySlopeChanged();
        }
    }

    private int CalculateGritReward(float effectivePower)
    {
        // Base grit per distance unit
        float baseGritRate = 1f;

        // Scale with distance for higher rewards at greater distances
        float distanceMultiplier = 1f + (_currentDistance / 10000f);

        return Mathf.RoundToInt(effectivePower * baseGritRate * distanceMultiplier);
    }

    public void ResetProgress()
    {
        _currentDistance = 0f;
        _currentSlope = 1f;
        ResetCombo();
        NotifyDistanceChanged();
        NotifySlopeChanged();
    }
    
    private void NotifyDistanceChanged() => OnDistanceChanged?.Invoke(_currentDistance);
    private void NotifySlopeChanged() => OnSlopeChanged?.Invoke(_currentSlope);
    private void NotifyComboStateChanged() => OnComboStateChanged?.Invoke(_comboState);
    
    void Update()
    {
        // Update combo timer
        if (_comboState.IsActive && _comboState.ComboTimer > 0f)
        {
            _comboState.ComboTimer -= Time.deltaTime;
            if (_comboState.ComboTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

}
