using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class SaveManager : MonoBehaviour
{
    [Header("Save Configuration")]
    [SerializeField] private string _saveFileName = "sisyphus_save.json";
    [SerializeField] private bool _useEncryption = false;

    private string _saveFilePath;

    public void Initialize()
    {
        _saveFilePath = Path.Combine(Application.persistentDataPath, _saveFileName);
    }

    public void SaveGame(SaveData saveData)
    {
        try
        {
            saveData.lastSaveTime = System.DateTimeOffset.Now.ToUnixTimeSeconds();
            saveData.gameVersion = Application.version;

            string jsonData = JsonUtility.ToJson(saveData, true);

            if (_useEncryption)
            {
                jsonData = EncryptData(jsonData);
            }

            File.WriteAllText(_saveFilePath, jsonData);

            Debug.Log($"Game saved successfully at {_saveFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public SaveData LoadGame()
    {
        try
        {
            if (!File.Exists(_saveFilePath))
            {
                Debug.Log("No save file found, returning default save data");
                return CreateDefaultSaveData();
            }

            string jsonData = File.ReadAllText(_saveFilePath);

            if (_useEncryption)
            {
                jsonData = DecryptData(jsonData);
            }

            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);

            if (ValidateSaveData(saveData))
            {
                // Calculate offline progress
                SaveData processedSaveData = ProcessOfflineProgress(saveData);
                Debug.Log("Game loaded successfully");
                return processedSaveData;
            }
            else
            {
                Debug.LogWarning("Save data validation failed, using default values");
                return CreateDefaultSaveData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load game: {e.Message}");
            return CreateDefaultSaveData();
        }
    }

    private SaveData ProcessOfflineProgress(SaveData saveData)
    {
        long currentTime = System.DateTimeOffset.Now.ToUnixTimeSeconds();
        long offlineTimeSeconds = currentTime - saveData.lastSaveTime;

        // Only process offline progress if offline for more than 1 minute
        if (offlineTimeSeconds > 60)
        {
            float offlineHours = offlineTimeSeconds / 3600f;

            // Calculate offline auto-push progress
            // This requires reconstructing some upgrade state to calculate auto-push rate
            float autoClickRate = CalculateAutoClickRateFromSave(saveData);

            if (autoClickRate > 0)
            {
                float totalAutoPushes = autoClickRate * offlineTimeSeconds;
                float autoPushPower = CalculateAutoPushPowerFromSave(saveData);

                // Apply offline progress (simplified calculation)
                float offlineDistance = totalAutoPushes * autoPushPower;
                saveData.currentDistance += offlineDistance;

                // Award offline grit
                int offlineGrit = Mathf.RoundToInt(offlineDistance * 1f); // 1 grit per distance unit
                saveData.grit += offlineGrit;

                Debug.Log($"Offline for {offlineHours:F1} hours. Gained {offlineDistance:F1} distance and {offlineGrit} grit");
            }
        }

        return saveData;
    }

    private float CalculateAutoClickRateFromSave(SaveData saveData)
    {
        // Reconstruct auto-click rate from saved upgrade levels
        int determinationLevel = saveData.normalUpgradeLevels.GetValueOrDefault(UpgradeType.Determination, 0);
        return determinationLevel * 0.5f; // 0.5 clicks per second per level (example)
    }

    private float CalculateAutoPushPowerFromSave(SaveData saveData)
    {
        // Reconstruct auto-push power from saved upgrade levels
        int muscleLevel = saveData.normalUpgradeLevels.GetValueOrDefault(UpgradeType.StrongerMuscles, 0);
        float basePower = 1f + (muscleLevel * 1f); // 1 power per level (example)

        // Apply prestige multipliers
        int herculesLevel = saveData.prestigeUpgradeLevels.GetValueOrDefault(PrestigeUpgradeType.MightOfHercules, 0);
        float prestigeMultiplier = 1f + (herculesLevel * 0.5f);

        return basePower * prestigeMultiplier;
    }

    private SaveData CreateDefaultSaveData()
    {
        return new SaveData
        {
            grit = 0,
            favors = 0,
            normalUpgradeLevels = new Dictionary<UpgradeType, int>(),
            prestigeUpgradeLevels = new Dictionary<PrestigeUpgradeType, int>(),
            currentDistance = 0f,
            currentSlope = 1f,
            comboState = new ComboState(),
            lastSaveTime = System.DateTimeOffset.Now.ToUnixTimeSeconds(),
            gameVersion = Application.version,
            prestigeCount = 0
        };
    }

    private bool ValidateSaveData(SaveData saveData)
    {
        // Basic validation checks
        if (saveData == null)
            return false;
        if (saveData.grit < 0 || saveData.favors < 0)
            return false;
        if (saveData.currentDistance < 0 || saveData.currentSlope <= 0)
            return false;

        // Check if save data is from a compatible version
        if (!string.IsNullOrEmpty(saveData.gameVersion))
        {
            // Add version compatibility checks here if needed
        }

        return true;
    }

    public bool HasSaveFile()
    {
        return File.Exists(_saveFilePath);
    }

    public void DeleteSave()
    {
        if (File.Exists(_saveFilePath))
        {
            File.Delete(_saveFilePath);
            Debug.Log("Save file deleted");
        }
    }

    private string EncryptData(string data)
    {
        // Simple encryption implementation (you might want something more robust)
        byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
        return System.Convert.ToBase64String(dataBytes);
    }

    private string DecryptData(string encryptedData)
    {
        // Simple decryption implementation
        byte[] dataBytes = System.Convert.FromBase64String(encryptedData);
        return System.Text.Encoding.UTF8.GetString(dataBytes);
    }

    public int GetValueOrDefault(Dictionary<UpgradeType, int> dictionary, UpgradeType key, int defaultValue = 0)
    {
        return dictionary != null && dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
    }
    
    public int GetValueOrDefault(Dictionary<PrestigeUpgradeType, int> dictionary, PrestigeUpgradeType key, int defaultValue = 0)
    {
        return dictionary != null && dictionary.ContainsKey(key) ? dictionary[key] : defaultValue;
    }
}
