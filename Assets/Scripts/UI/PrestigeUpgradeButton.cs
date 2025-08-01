using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PrestigeUpgradeButton : MonoBehaviour
{
    [SerializeField] private PrestigeUpgradeType _prestigeUpgradeType;
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
        inputManager.HandlePrestigeUpgradeClick(this);
    }

    public PrestigeUpgradeType GetUpgradeType() => _prestigeUpgradeType;
}
