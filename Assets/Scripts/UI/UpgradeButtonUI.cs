using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UI component for individual upgrade buttons
public class UpgradeButtonUI : MonoBehaviour
{
    [SerializeField] private UpgradeDataSO _upgradeDataSO;
    [Header("UI References")]
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _levelText;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _lockedOverlay;
    [SerializeField] private GameObject _maxLevelIndicator;
    private UpgradeType _upgradeType;
    private UIManager _uiManager;
    private bool _isUnlocked = false;

    public void Initialize(UpgradeType upgradeType, UIManager uiManager)
    {
        _upgradeType = upgradeType;
        _uiManager = uiManager;

        if (_nameText != null) _nameText.text = _upgradeDataSO.GetNormalUpgrade(upgradeType).Name;
        if (_descriptionText != null) _descriptionText.text = _upgradeDataSO.GetNormalUpgrade(upgradeType).Description;
        if (_icon != null) _icon.sprite = _upgradeDataSO.GetNormalUpgrade(upgradeType).Icon;
        
        if (_button != null) _button.onClick.AddListener(OnButtonClick);

    }

    public void UpdateLevel(int level)
    {
        if (_levelText != null) _levelText.text = $"Level {level}";
    }

    public void UpdateCost(int cost)
    {
        if (_costText != null) _costText.text = $"Cost: {cost}";
    }

    public void SetAffordable(bool affordable)
    {
        if (_button != null) _button.interactable = affordable && _isUnlocked;

        // Visual feedback for affordability
        Color buttonColor = affordable ? Color.white : Color.gray;
        if (_button != null && _button.targetGraphic != null)
        {
            _button.targetGraphic.color = buttonColor;
        }
    }

    public void SetUnlocked(bool unlocked)
    {
        _isUnlocked = unlocked;
        if (_lockedOverlay != null) _lockedOverlay.SetActive(!unlocked);
        if (_button != null) _button.interactable = unlocked;
    }

    public void PlayPurchaseAnimation()
    {
        StartCoroutine(PulseAnimation());
    }

    public void PlayUnlockAnimation()
    {
        StartCoroutine(UnlockAnimation());
    }

    public void PlayResetAnimation()
    {
        StartCoroutine(ResetAnimation());
    }

    private void OnButtonClick()
    {
        if (_uiManager != null)
        {
            bool success = _uiManager.TryPurchaseNormalUpgrade(_upgradeType);
            if (!success)
            {
                // Could play error sound or animation here
                StartCoroutine(ShakeAnimation());
            }
        }
    }

    private IEnumerator PulseAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        // Scale up
        yield return ScaleToTarget(targetScale, 0.1f);

        // Scale down
        yield return ScaleToTarget(originalScale, 0.1f);
    }

    private IEnumerator UnlockAnimation()
    {
        // Flash effect
        for (int i = 0; i < 3; i++)
        {
            if (_icon != null) _icon.color = Color.yellow;
            yield return new WaitForSeconds(0.1f);
            if (_icon != null) _icon.color = Color.white;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ResetAnimation()
    {
        // Fade out and in
        yield return FadeToAlpha(0f, 0.2f);
        yield return FadeToAlpha(1f, 0.2f);
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

    private IEnumerator ScaleToTarget(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private IEnumerator FadeToAlpha(float targetAlpha, float duration)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}