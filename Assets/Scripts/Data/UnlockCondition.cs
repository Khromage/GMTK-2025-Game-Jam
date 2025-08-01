using UnityEngine;

[System.Serializable]
public class UnlockCondition
{
    public UnlockConditionType ConditionType;
    
    [Header("Upgrade Level Condition")]
    public UpgradeType RequiredUpgrade;
    public int RequiredLevel = 1;
    
    [Header("Distance Condition")]
    public float RequiredDistance = 100f;
    
    [Header("Prestige Condition")]
    public int RequiredPrestigeCount = 1;
    
    [Header("Currency Condition")]
    public int RequiredGrit = 0;
    public int RequiredFavors = 0;
}

public enum UnlockConditionType
{
    UpgradeLevel,    // Requires another upgrade to be at a certain level
    Distance,        // Requires reaching a certain distance
    PrestigeCount,   // Requires a certain number of prestiges
    Currency         // Requires having a certain amount of currency
}