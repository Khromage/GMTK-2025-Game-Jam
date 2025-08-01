using UnityEngine;
using UnityEngine.Events;
public class InputManager : MonoBehaviour
{
    private ProgressManager _progressManager;
    private UpgradeManager _upgradeManager;
    private PrestigeManager _prestigeManager;

    // Events
    public UnityAction OnBoulderClicked;
    public UnityAction<UpgradeType> OnUpgradeRequested;
    public UnityAction<PrestigeUpgradeType> OnPrestigeUpgradeRequested;
    public UnityAction OnPrestigeRequested;

    public void Initialize(ProgressManager progressManager, UpgradeManager upgradeManager, PrestigeManager prestigeManager)
    {
        _progressManager = progressManager;
        _upgradeManager = upgradeManager;
        _prestigeManager = prestigeManager;

        OnBoulderClicked = new UnityAction(() => { });
        OnUpgradeRequested = new UnityAction<UpgradeType>((type) => { });
        OnPrestigeUpgradeRequested = new UnityAction<PrestigeUpgradeType>((type) => { });
        OnPrestigeRequested = new UnityAction(() => { });
    }

    void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            HandleObjectClick(hit.collider.gameObject);
        }
    }

    private void HandleObjectClick(GameObject clickedObject)
    {
        // Check what was clicked using tags or components
        if (clickedObject.CompareTag("Boulder"))
        {
            HandleBoulderPush();
        }

    }

    private void HandleBoulderPush()
    {
        _progressManager.ManualPush();
        OnBoulderClicked?.Invoke();

        // Play push sound
        //GameManager.Instance.AudioManager.PlaySFX(SFXType.BoulderPush);
    }

    public void HandleUpgradeClick(UpgradeButton upgradeButton)
    {
        Debug.Log($"upgrade button clicked. detected <<<{upgradeButton.GetUpgradeType()}>>> upgrade type");

        UpgradeType upgradeType = upgradeButton.GetUpgradeType();
        bool success = _upgradeManager.PurchaseNormalUpgrade(upgradeType);

        if (success)
        {
            OnUpgradeRequested?.Invoke(upgradeType);
            //GameManager.Instance.AudioManager.PlaySFX(SFXType.UpgradePurchased);
        }
        else
        {
            //GameManager.Instance.AudioManager.PlaySFX(SFXType.UpgradeFailed);
        }
    }

    public void HandlePrestigeUpgradeClick(PrestigeUpgradeButton prestigeUpgradeButton)
    {
        PrestigeUpgradeType upgradeType = prestigeUpgradeButton.GetUpgradeType();
        bool success = _upgradeManager.PurchasePrestigeUpgrade(upgradeType);

        if (success)
        {
            OnPrestigeUpgradeRequested?.Invoke(upgradeType);
            //GameManager.Instance.AudioManager.PlaySFX(SFXType.PrestigeUpgradePurchased);
        }
        else
        {
            //GameManager.Instance.AudioManager.PlaySFX(SFXType.UpgradeFailed);
        }
    }

    public void HandlePrestigeClick()
    {
        if (_prestigeManager.CanPrestige())
        {
            _prestigeManager.ExecutePrestige();
            OnPrestigeRequested?.Invoke();
        }
    }

}
