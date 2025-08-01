using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private UpgradeType _upgradeType;
    private Button button;
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        // Get reference to InputManager and call the method directly
        InputManager inputManager = GameManager.Instance.InputManager;
        inputManager.HandleUpgradeClick(this);
    }

    public UpgradeType GetUpgradeType() => _upgradeType;
}
