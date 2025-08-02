using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    [Header("Currencies")]
    public int grit = 0;
    public int favors = 0;

    [Header("Upgrade Levels")]
    public List<SerializableUpgradeLevel> normalUpgradeLevels = new List<SerializableUpgradeLevel>();
    public List<SerializablePrestigeUpgradeLevel> prestigeUpgradeLevels = new List<SerializablePrestigeUpgradeLevel>();

    [Header("Progress")]
    public float currentDistance = 0f;
    public float currentSlope = 1f;
    public ComboState comboState = new ComboState();

    [Header("Meta Data")]
    public long lastSaveTime = 0;
    public string gameVersion = "1.0";
    public int prestigeCount = 0;
}