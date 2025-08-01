using UnityEngine;
using UnityEngine.Events;

public class PrestigeManager : MonoBehaviour
{
    [Header("Prestige Configuration")]
    [SerializeField] private float _prestigeThreshold = 10000f;
    [SerializeField] private int _prestigeCount = 0;

    private CurrencyManager _currencyManager;
    private ProgressManager _progressManager;
    private UpgradeManager _upgradeManager;

    // Events
    public UnityAction OnPrestigeAvailable;
    public UnityAction<int> OnPrestigeCompleted;
    public UnityAction<int> OnPrestigeRewardCalculated;

    public int GetPrestigeCount() => _prestigeCount;
    public float GetPrestigeThreshold() => _prestigeThreshold;

    public void Initialize(CurrencyManager currencyManager, ProgressManager progressManager, UpgradeManager upgradeManager)
    {
        _currencyManager = currencyManager;
        _progressManager = progressManager;
        _upgradeManager = upgradeManager;

        // setup events
        OnPrestigeAvailable = new UnityAction(() => { });
        OnPrestigeCompleted = new UnityAction<int>((favors) => { });
        OnPrestigeRewardCalculated = new UnityAction<int>((favors) => { });
    }

    public void SetDefaultValues()
    {
        _prestigeCount = 0;
    }

    public void LoadFromSaveData(SaveData saveData)
    {
        _prestigeCount = saveData.prestigeCount;
    }

    void Update()
    {
        CheckPrestigeAvailability();
    }

    private void CheckPrestigeAvailability()
    {
        if (CanPrestige())
        {
            OnPrestigeAvailable?.Invoke();
        }
    }

    public bool CanPrestige()
    {
        return _progressManager.GetCurrentDistance() >= _prestigeThreshold;
    }

    public void ExecutePrestige()
    {
        if (!CanPrestige()) // safety net
            return;

        // Calculate and award favors
        int favorsEarned = CalculatePrestigeReward();
        _currencyManager.AddFavors(favorsEarned);

        // Increment prestige count
        _prestigeCount++;

        // Reset progress data
        ResetProgressData();

        // Reset normal upgrades
        ResetNormalUpgrades();

        // Apply prestige bonuses
        ApplyPrestigeBonuses();

        // Notify completion
        OnPrestigeCompleted?.Invoke(favorsEarned);

        // Play prestige sound
        GameManager.Instance.AudioManager.PlaySFX(SFXType.PrestigeCompleted);
    }

    public int CalculatePrestigeReward()
    {
        float currentDistance = _progressManager.GetCurrentDistance();

        if (currentDistance < _prestigeThreshold) // safety net, theoretically shouldn't be reached
            return 0;

        // Calculate favors based on distance past threshold
        // Favors = divNum^2 as specified
        int favors = (int)Mathf.Pow(Mathf.FloorToInt(currentDistance / _prestigeThreshold), 2f);

        OnPrestigeRewardCalculated?.Invoke(favors);
        return favors;
    }

    private void ResetProgressData()
    {
        _progressManager.ResetProgress();
    }

    private void ResetNormalUpgrades()
    {
        _upgradeManager.ResetNormalUpgrades();
    }

    private void ApplyPrestigeBonuses()
    {
        // Prestige bonuses are applied automatically through upgrade calculations
        // This method could be used for immediate effects that need to be applied
    }

}
