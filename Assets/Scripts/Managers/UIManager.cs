using UnityEngine;

public class UIManager : MonoBehaviour
{
    
    private CurrencyManager _currencyManager;
    private UpgradeManager _upgradeManager;
    private ProgressManager _progressManager;
    private PrestigeManager _prestigeManager;

    public void Initialize(CurrencyManager currencyManager, UpgradeManager upgradeManager, ProgressManager progressManager, PrestigeManager prestigeManager)
    {
        _currencyManager = currencyManager;
        _upgradeManager = upgradeManager;
        _progressManager = progressManager;
        _prestigeManager = prestigeManager;

        return;
    }
}
