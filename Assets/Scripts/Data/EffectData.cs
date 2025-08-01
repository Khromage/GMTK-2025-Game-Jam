using UnityEngine;

[System.Serializable]
public class EffectData
{
    [Header("Effect Configuration")]
    public float BaseValue = 0f;           // Starting value for the effect
    public float ValuePerLevel = 1f;       // How much the effect increases per upgrade level
    public EffectType EffectType;          // What type of effect this is
    public AnimationCurve ScalingCurve;    // Optional: non-linear scaling curve
    
    public float CalculateEffectValue(int level)
    {
        if (ScalingCurve != null && ScalingCurve.keys.Length > 0)
        {
            return BaseValue + (ScalingCurve.Evaluate(level) * ValuePerLevel);
        }
        
        return BaseValue + (level * ValuePerLevel);
    }
}

public enum EffectType
{
    ManualClickPower,
    AutoClickRate,
    AutoClickPower,
    CriticalChance,
    CriticalMultiplier,
    ComboMultiplier,
    FlatBonus
}