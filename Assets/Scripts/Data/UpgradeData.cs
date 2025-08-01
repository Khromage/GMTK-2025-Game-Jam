using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UpgradeData
{
    [Header("Basic Info")]
    public UpgradeType UpgradeType;
    public string Name;
    [TextArea] public string Description;
    public Sprite Icon;
    
    [Header("Cost Configuration")]
    public int BaseCost = 10;              // Starting cost for first upgrade
    public float CostMultiplier = 1.5f;    // How much cost increases per level
    
    [Header("Level Configuration")]
    public bool IsInfinite = true;         // Can be upgraded indefinitely
    public int MaxLevel = 1;               // Maximum level if not infinite
    
    [Header("Unlock Conditions")]
    public List<UnlockCondition> UnlockConditions = new List<UnlockCondition>();
    
    [Header("Effect")]
    public EffectData EffectData;          // What this upgrade actually does
}