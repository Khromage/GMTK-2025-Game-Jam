using UnityEngine;

[System.Serializable]
public class ComboState
{
    public int CurrentCombo = 0;           // Current combo level
    public float ComboTimer = 0f;          // Time remaining before combo expires
    public float SpeedThreshold = 5f;      // Minimum push power needed to maintain combo
    public float Multiplier = 1f;          // Current combo multiplier
    public bool IsActive = false;          // Whether combo is currently active
}