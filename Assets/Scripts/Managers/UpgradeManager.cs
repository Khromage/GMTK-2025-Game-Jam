using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
public class UpgradeManager : MonoBehaviour
{
    [Header("Upgrade Configuration")]
    [SerializeField] private UpgradeDataSO _upgradeDataSO;

    [Header("Current Upgrade Levels")]
    [SerializeField] private Dictionary<UpgradeType, int> _normalUpgradeLevels = new Dictionary<UpgradeType, int>();
    [SerializeField] private Dictionary<PrestigeUpgradeType, int> _prestigeUpgradeLevels = new Dictionary<PrestigeUpgradeType, int>();

    [Header("Upgrade Unlock States")]
    [SerializeField] private Dictionary<UpgradeType, bool> _previousUnlockStates = new Dictionary<UpgradeType, bool>();

    private CurrencyManager _currencyManager;

    public UnityAction<UpgradeType, int> OnNormalUpgradePurchased;
    public UnityAction<PrestigeUpgradeType, int> OnPrestigeUpgradePurchased;
    public UnityAction<UpgradeType, bool> OnUpgradeUnlocked;

    public void Initialize(CurrencyManager currencyManager)
    {
        _currencyManager = currencyManager;

        // setup events for buying & unlocking upgrades
        OnNormalUpgradePurchased = new UnityAction<UpgradeType, int>((type, level) => { });
        OnPrestigeUpgradePurchased = new UnityAction<PrestigeUpgradeType, int>((type, level) => { });
        OnUpgradeUnlocked = new UnityAction<UpgradeType, bool>((type, unlocked) => { });

        InitializeUpgradeDictionaries();
    }

    private void InitializeUpgradeDictionaries()
    {
        // Initialize all normal upgrade levels to 0
        foreach (UpgradeType upgradeType in System.Enum.GetValues(typeof(UpgradeType)))
        {
            _normalUpgradeLevels[upgradeType] = 0;
        }

        // Initialize all prestige upgrade levels to 0
        foreach (PrestigeUpgradeType upgradeType in System.Enum.GetValues(typeof(PrestigeUpgradeType)))
        {
            _prestigeUpgradeLevels[upgradeType] = 0;
        }

        // Initialize unlock states
        foreach (UpgradeType upgradeType in System.Enum.GetValues(typeof(UpgradeType)))
        {
            _previousUnlockStates[upgradeType] = false;
        }
    }

    // public method exposed to call InitializeUpgradeDictionaries() from external classes
    public void SetDefaultValues()
    {
        InitializeUpgradeDictionaries();
        UpdatePreviousUnlockStates();
    }

    public void LoadFromSaveData(SaveData saveData)
    {
        // Start fresh with current upgrade types all at level 0
        InitializeUpgradeDictionaries();

        // Apply saved normal upgrades if they exist and are still valid
        if (saveData.normalUpgradeLevels != null)
        {
            foreach (var upgradeLevel in saveData.normalUpgradeLevels)
            {
                // Check to make sure upgrade still exists in game
                if (_normalUpgradeLevels.ContainsKey(upgradeLevel.upgradeType))
                {
                    _normalUpgradeLevels[upgradeLevel.upgradeType] = upgradeLevel.level;
                }
                else
                {
                    Debug.LogWarning($"Saved upgrade type {upgradeLevel.upgradeType} no longer exists in game");
                }
            }
        }

        // Same logic for prestige upgrades
        if (saveData.prestigeUpgradeLevels != null)
        {
            foreach (var upgradeLevel in saveData.prestigeUpgradeLevels)
            {
                if (_prestigeUpgradeLevels.ContainsKey(upgradeLevel.upgradeType))
                {
                    _prestigeUpgradeLevels[upgradeLevel.upgradeType] = upgradeLevel.level;
                }
                else
                {
                    Debug.LogWarning($"Saved prestige upgrade type {upgradeLevel.upgradeType} no longer exists in game");
                }
            }
        }

        // IMPORTANT: After loading all upgrade levels, update the previous unlock states
        // to reflect what's actually unlocked so we don't fire false unlock events
        UpdatePreviousUnlockStates();
    }

    private void UpdatePreviousUnlockStates()
    {
        foreach (UpgradeType upgradeType in System.Enum.GetValues(typeof(UpgradeType)))
        {
            _previousUnlockStates[upgradeType] = IsNormalUpgradeUnlocked(upgradeType);
        }
    }

    public bool PurchaseNormalUpgrade(UpgradeType upgradeType)
    {
        UpgradeData upgradeData = _upgradeDataSO.GetNormalUpgrade(upgradeType);
        if (upgradeData == null)
            return false;

        int currentLevel = GetNormalUpgradeLevel(upgradeType);

        // check if upgrade has reached max level (and not infinite)
        if (!upgradeData.IsInfinite && currentLevel >= upgradeData.MaxLevel)
            return false;

        // check if upgrade is unlocked. more of a logic safety check
        if (!IsNormalUpgradeUnlocked(upgradeType))
            return false;

        int cost = CalculateNormalUpgradeCost(upgradeType, currentLevel);

        if (_currencyManager.SpendGrit(cost))
        {
            _normalUpgradeLevels[upgradeType] = currentLevel + 1;
            OnNormalUpgradePurchased?.Invoke(upgradeType, currentLevel + 1);

            // check if this purchase unlocks other upgrades
            CheckUpgradeUnlocks();

            return true;
        }

        return false;
    }

    // currently not setup to handle infinite unlockable prestige upgrades
    // can be modified to match PurchaseNormalUpgrade() logic if needed
    public bool PurchasePrestigeUpgrade(PrestigeUpgradeType upgradeType)
    {
        PrestigeUpgradeData upgradeData = _upgradeDataSO.GetPrestigeUpgrade(upgradeType);
        if (upgradeData == null)
            return false;

        int currentLevel = GetPrestigeUpgradeLevel(upgradeType);

        // check if upgrade has reached max level
        if (currentLevel >= upgradeData.MaxLevel)
            return false;

        int cost = CalculatePrestigeUpgradeCost(upgradeType, currentLevel);

        if (_currencyManager.SpendFavors(cost))
        {
            _prestigeUpgradeLevels[upgradeType] = currentLevel + 1;
            OnPrestigeUpgradePurchased?.Invoke(upgradeType, currentLevel + 1);
            return true;
        }

        return false;
    }

    // normals scale exponentially atm
    public int CalculateNormalUpgradeCost(UpgradeType upgradeType, int currentLevel)
    {
        UpgradeData upgradeData = _upgradeDataSO.GetNormalUpgrade(upgradeType);
        return Mathf.RoundToInt(upgradeData.BaseCost * Mathf.Pow(upgradeData.CostMultiplier, currentLevel));
    }

    // prestiges scale linearly atm
    public int CalculatePrestigeUpgradeCost(PrestigeUpgradeType upgradeType, int currentLevel)
    {
        PrestigeUpgradeData upgradeData = _upgradeDataSO.GetPrestigeUpgrade(upgradeType);
        return upgradeData.BaseCost + (currentLevel * upgradeData.CostIncrease);
    }

    private void CheckUpgradeUnlocks()
    {
        foreach (UpgradeType upgradeType in System.Enum.GetValues(typeof(UpgradeType)))
        {
            bool isCurrentlyUnlocked = IsNormalUpgradeUnlocked(upgradeType);
            bool wasAlreadyUnlocked = _previousUnlockStates.GetValueOrDefault(upgradeType, false);
            
            // Only invoke event if unlock state changed from false to true
            if (isCurrentlyUnlocked && !wasAlreadyUnlocked)
            {
                OnUpgradeUnlocked?.Invoke(upgradeType, true);
            }
            
            // Update the previous state
            _previousUnlockStates[upgradeType] = isCurrentlyUnlocked;
        }
    }

    public void ManualCheckUnlocks()
    {
        Debug.Log("[UpgradeManager] ManualCheckUnlocks called");
        CheckUpgradeUnlocks();
    }

    public bool IsNormalUpgradeUnlocked(UpgradeType upgradeType)
    {
        UpgradeData upgradeData = _upgradeDataSO.GetNormalUpgrade(upgradeType);

        foreach (UnlockCondition condition in upgradeData.UnlockConditions)
        {
            if (!IsConditionMet(condition))
                return false;
        }

        return true;
    }

    private bool IsConditionMet(UnlockCondition condition)
    {
        switch (condition.ConditionType)
        {
            case UnlockConditionType.UpgradeLevel:
                return GetNormalUpgradeLevel(condition.RequiredUpgrade) >= condition.RequiredLevel;
            // placeholder for future upgrade unlocks based on distance
            case UnlockConditionType.Distance:
                return GameManager.Instance.ProgressManager.GetCurrentDistance() >= condition.RequiredDistance;
            // placeholder for future upgrade unlocks based on prestige count
            case UnlockConditionType.PrestigeCount:
                return GameManager.Instance.PrestigeManager.GetPrestigeCount() >= condition.RequiredPrestigeCount;
            default:
                return true;
        }
    }


    // Calculation methods for other systems to use
    public float GetManualClickPower()
    {
        int shoveLevel = GetNormalUpgradeLevel(UpgradeType.Shove);
        UpgradeData shoveData = _upgradeDataSO.GetNormalUpgrade(UpgradeType.Shove);
        return shoveData.EffectData.BaseValue + (shoveLevel * shoveData.EffectData.ValuePerLevel);
    }

    public float GetAutoClickRate()
    {
        int determinationLevel = GetNormalUpgradeLevel(UpgradeType.Determination);
        UpgradeData determinationData = _upgradeDataSO.GetNormalUpgrade(UpgradeType.Determination);
        return determinationLevel * determinationData.EffectData.ValuePerLevel;
    }

    public float GetAutoClickPower()
    {
        int muscleLevel = GetNormalUpgradeLevel(UpgradeType.StrongerMuscles);
        UpgradeData muscleData = _upgradeDataSO.GetNormalUpgrade(UpgradeType.StrongerMuscles);
        return muscleData.EffectData.BaseValue + (muscleLevel * muscleData.EffectData.ValuePerLevel);
    }

    public float GetCriticalChance()
    {
        int sleightLevel = GetNormalUpgradeLevel(UpgradeType.SleightOfForce);
        UpgradeData sleightData = _upgradeDataSO.GetNormalUpgrade(UpgradeType.SleightOfForce);
        return sleightLevel * sleightData.EffectData.ValuePerLevel; // 1% per level
    }

    public float GetCriticalMultiplier()
    {
        int titanicLevel = GetNormalUpgradeLevel(UpgradeType.TitanicForce);
        UpgradeData titanicData = _upgradeDataSO.GetNormalUpgrade(UpgradeType.TitanicForce);
        float baseMultiplier = 2f; // Base crit multiplier
        return baseMultiplier + (titanicLevel * titanicData.EffectData.ValuePerLevel); // 0.05x per level
    }

    public float GetMomentumMultiplier()
    {
        int momentumLevel = GetNormalUpgradeLevel(UpgradeType.Momentum);
        UpgradeData momentumData = _upgradeDataSO.GetNormalUpgrade(UpgradeType.Momentum);
        return 1f + (momentumLevel * momentumData.EffectData.ValuePerLevel);
    }

    public float GetHerculesMultiplier()
    {
        int herculesLevel = GetPrestigeUpgradeLevel(PrestigeUpgradeType.MightOfHercules);
        PrestigeUpgradeData herculesData = _upgradeDataSO.GetPrestigeUpgrade(PrestigeUpgradeType.MightOfHercules);
        return 1f + (herculesLevel * herculesData.EffectData.ValuePerLevel);
    }

    public float GetAtlasMultiplier()
    {
        int atlasLevel = GetPrestigeUpgradeLevel(PrestigeUpgradeType.HandsOfAtlas);
        PrestigeUpgradeData atlasData = _upgradeDataSO.GetPrestigeUpgrade(PrestigeUpgradeType.HandsOfAtlas);
        return 1f + (atlasLevel * atlasData.EffectData.ValuePerLevel);
    }
    public float GetGaiaMultiplier()
    {
        int gaiaLevel = GetPrestigeUpgradeLevel(PrestigeUpgradeType.BlessingOfGaia);
        PrestigeUpgradeData gaiaData = _upgradeDataSO.GetPrestigeUpgrade(PrestigeUpgradeType.BlessingOfGaia);
        return 1f + (gaiaLevel * gaiaData.EffectData.ValuePerLevel);
    }
    /* public float GetHermesMultiplier()
    {
        int hermesLevel = GetPrestigeUpgradeLevel(PrestigeUpgradeType.EchoesOfHermes);
        PrestigeUpgradeData hermesData = _upgradeDataSO.GetPrestigeUpgrade(PrestigeUpgradeType.EchoesOfHermes);
        return 1f + (hermesLevel * hermesData.EffectData.ValuePerLevel);
    }
 */
    public int GetNormalUpgradeLevel(UpgradeType upgradeType) => _normalUpgradeLevels.GetValueOrDefault(upgradeType, 0);
    public int GetPrestigeUpgradeLevel(PrestigeUpgradeType upgradeType) => _prestigeUpgradeLevels.GetValueOrDefault(upgradeType, 0);

    public List<SerializableUpgradeLevel> GetNormalUpgradeLevels()
    {
        List<SerializableUpgradeLevel> upgradeLevels = new List<SerializableUpgradeLevel>();
        foreach (var kvp in _normalUpgradeLevels)
        {
            upgradeLevels.Add(new SerializableUpgradeLevel(kvp.Key, kvp.Value));
        }
        return upgradeLevels;
    }

    public List<SerializablePrestigeUpgradeLevel> GetPrestigeUpgradeLevels()
    {
        List<SerializablePrestigeUpgradeLevel> upgradeLevels = new List<SerializablePrestigeUpgradeLevel>();
        foreach (var kvp in _prestigeUpgradeLevels)
        {
            upgradeLevels.Add(new SerializablePrestigeUpgradeLevel(kvp.Key, kvp.Value));
        }
        return upgradeLevels;
    }
    
    // called on prestige
    public void ResetNormalUpgrades()
    {
        foreach (UpgradeType upgradeType in System.Enum.GetValues(typeof(UpgradeType)))
        {
            _normalUpgradeLevels[upgradeType] = 0;
        }

        // Update unlock states after reset
        UpdatePreviousUnlockStates();
    }

}


[System.Serializable]
public class SerializableUpgradeLevel
{
    public UpgradeType upgradeType;
    public int level;
    
    public SerializableUpgradeLevel(UpgradeType type, int lvl)
    {
        upgradeType = type;
        level = lvl;
    }
}

[System.Serializable]
public class SerializablePrestigeUpgradeLevel
{
    public PrestigeUpgradeType upgradeType;
    public int level;
    
    public SerializablePrestigeUpgradeLevel(PrestigeUpgradeType type, int lvl)
    {
        upgradeType = type;
        level = lvl;
    }
}