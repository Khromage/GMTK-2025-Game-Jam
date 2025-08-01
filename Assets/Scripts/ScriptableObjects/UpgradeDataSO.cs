using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Sisyphus/Upgrade Data")]
public class UpgradeDataSO : ScriptableObject
{
    [Header("Normal Upgrades")]
    [SerializeField] private List<UpgradeData> _normalUpgrades = new List<UpgradeData>();
    
    [Header("Prestige Upgrades")]  
    [SerializeField] private List<PrestigeUpgradeData> _prestigeUpgrades = new List<PrestigeUpgradeData>();

    // Dictionary caching for fast lookups (populated at runtime)
    // Faster to lookup upgrades by kvp rather than iterate through list
    private Dictionary<UpgradeType, UpgradeData> _normalUpgradeCache;
    private Dictionary<PrestigeUpgradeType, PrestigeUpgradeData> _prestigeUpgradeCache;
    
    void OnEnable()
    {
        BuildUpgradeCache();
    }
    
    private void BuildUpgradeCache()
    {
        // Build normal upgrade cache
        _normalUpgradeCache = new Dictionary<UpgradeType, UpgradeData>();
        foreach (UpgradeData upgrade in _normalUpgrades)
        {
            if (!_normalUpgradeCache.ContainsKey(upgrade.UpgradeType))
            {
                _normalUpgradeCache[upgrade.UpgradeType] = upgrade;
            }
            else
            {
                Debug.LogError($"Duplicate normal upgrade type: {upgrade.UpgradeType}");
            }
        }
        
        // Build prestige upgrade cache
        _prestigeUpgradeCache = new Dictionary<PrestigeUpgradeType, PrestigeUpgradeData>();
        foreach (PrestigeUpgradeData upgrade in _prestigeUpgrades)
        {
            if (!_prestigeUpgradeCache.ContainsKey(upgrade.UpgradeType))
            {
                _prestigeUpgradeCache[upgrade.UpgradeType] = upgrade;
            }
            else
            {
                Debug.LogError($"Duplicate prestige upgrade type: {upgrade.UpgradeType}");
            }
        }
    }
    
    public UpgradeData GetNormalUpgrade(UpgradeType upgradeType)
    {
        if (_normalUpgradeCache == null) BuildUpgradeCache();
        return _normalUpgradeCache.GetValueOrDefault(upgradeType, null);
    }
    
    public PrestigeUpgradeData GetPrestigeUpgrade(PrestigeUpgradeType upgradeType)
    {
        if (_prestigeUpgradeCache == null) BuildUpgradeCache();
        return _prestigeUpgradeCache.GetValueOrDefault(upgradeType, null);
    }
    
    public List<UpgradeData> GetAllNormalUpgrades() => new List<UpgradeData>(_normalUpgrades);
    public List<PrestigeUpgradeData> GetAllPrestigeUpgrades() => new List<PrestigeUpgradeData>(_prestigeUpgrades);
    
    // Validation method for editor
    public bool ValidateUpgradeData()
    {
        bool isValid = true;
        
        // Check for missing normal upgrades
        foreach (UpgradeType upgradeType in System.Enum.GetValues(typeof(UpgradeType)))
        {
            if (!_normalUpgradeCache.ContainsKey(upgradeType))
            {
                Debug.LogError($"Missing normal upgrade definition for: {upgradeType}");
                isValid = false;
            }
        }
        
        // Check for missing prestige upgrades
        foreach (PrestigeUpgradeType upgradeType in System.Enum.GetValues(typeof(PrestigeUpgradeType)))
        {
            if (!_prestigeUpgradeCache.ContainsKey(upgradeType))
            {
                Debug.LogError($"Missing prestige upgrade definition for: {upgradeType}");
                isValid = false;
            }
        }
        
        return isValid;
    }
}