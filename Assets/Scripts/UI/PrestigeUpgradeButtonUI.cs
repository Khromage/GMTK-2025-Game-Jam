using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Prestige upgrade button UI component
public class PrestigeUpgradeButtonUI : MonoBehaviour
{
    [SerializeField] private UpgradeDataSO _upgradeDataSO;
    [Header("UI References")]
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _maxLevelIndicator;
    
    private PrestigeUpgradeType _upgradeType;
    private UIManager _uiManager;
    
    public void Initialize(PrestigeUpgradeType upgradeType, UIManager uiManager)
    {
        _upgradeType = upgradeType;
        _uiManager = uiManager;
        
        if (_nameText != null) _nameText.text = _upgradeDataSO.GetPrestigeUpgrade(upgradeType).Name;
        if (_descriptionText != null) _descriptionText.text = _upgradeDataSO.GetPrestigeUpgrade(upgradeType).Description;
        if (_icon != null) _icon.sprite = _upgradeDataSO.GetPrestigeUpgrade(upgradeType).Icon;

        if (_button != null) _button.onClick.AddListener(OnButtonClick);
    }
    
    public void UpdateLevel(int level)
    {
        if (_levelText != null)
        {
            _levelText.text = $"Level {level}";
            PrestigeUpgradeData data = _upgradeDataSO.GetPrestigeUpgrade(_upgradeType);
            if (level >= data.MaxLevel)
                _maxLevelIndicator.SetActive(true);
        }
    }
    
    public void UpdateCost(int cost)
    {
        if (_costText != null) _costText.text = $"Cost: {cost}";
    }
    
    public void SetAffordable(bool affordable)
    {
        //if (_button != null) _button.interactable = affordable;
        
        // Visual feedback for affordability
        Color buttonColor = affordable ? Color.white : Color.gray;
        if (_button != null && _button.targetGraphic != null)
        {
            _button.targetGraphic.color = buttonColor;
        }
    }
    
    public void PlayPurchaseAnimation()
    {
        //StartCoroutine(PulseAnimation());
    }
    
    private void OnButtonClick()
    {
        if (_uiManager != null)
        {
            bool success = _uiManager.TryPurchasePrestigeUpgrade(_upgradeType);
            if (!success)
            {
                StartCoroutine(ShakeAnimation());
            }
        }
    }
    
    private IEnumerator PulseAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        
        // Scale up
        float duration = 0.1f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        // Scale down
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }

    private IEnumerator ShakeAnimation()
    {
        Vector3 originalPosition = transform.localPosition;
        float shakeAmount = 5f;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = originalPosition.x + Random.Range(-shakeAmount, shakeAmount);
            transform.localPosition = new Vector3(x, originalPosition.y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}