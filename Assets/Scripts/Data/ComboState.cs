using UnityEngine;

[System.Serializable]
public class ComboState
{
    public float ComboTimer = 0f;          // Time remaining before combo expires
    public float Multiplier = 1f;          // Current combo multiplier
    public bool IsActive = false;          // Whether combo is currently active
}