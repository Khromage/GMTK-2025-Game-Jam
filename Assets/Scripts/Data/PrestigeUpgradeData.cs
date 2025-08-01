using UnityEngine;

[System.Serializable]
public class PrestigeUpgradeData
{
    [Header("Basic Info")]
    public PrestigeUpgradeType UpgradeType;
    public string Name;
    [TextArea] public string Description;
    public Sprite Icon;
    
    [Header("Cost Configuration")]
    public int BaseCost = 1;               // Starting cost for first upgrade
    public int CostIncrease = 1;           // Linear cost increase per level
    
    [Header("Level Configuration")]
    public int MaxLevel = 10;              // Maximum level (prestige upgrades are always capped)
    
    [Header("Effect")]
    public EffectData EffectData;          // What this upgrade actually does
    
    [Header("Visual")]
    public Color UpgradeColor = Color.white;
    public string FlavorText;              // Additional lore text
    
    public int CalculateCost(int currentLevel)
    {
        return BaseCost + (currentLevel * CostIncrease);
    }
    
    public bool IsMaxLevel(int currentLevel)
    {
        return currentLevel >= MaxLevel;
    }
}