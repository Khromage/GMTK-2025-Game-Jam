using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Notification UI component
public class NotificationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private Image _background;
    [SerializeField] private Image _icon;
    
    public void Initialize(string message, NotificationType type, float displayTime)
    {
        if (_messageText != null) _messageText.text = message;
        
        // Set colors based on notification type
        Color backgroundColor = GetNotificationColor(type);
        if (_background != null) _background.color = backgroundColor;
        
        // Start fade-in animation
        StartCoroutine(FadeInAndOut(displayTime));
    }
    
    private Color GetNotificationColor(NotificationType type)
    {
        switch (type)
        {
            case NotificationType.Upgrade: return new Color(0.2f, 0.8f, 0.2f, 0.8f);
            case NotificationType.PrestigeUpgrade: return new Color(0.8f, 0.2f, 0.8f, 0.8f);
            case NotificationType.Prestige: return new Color(0.8f, 0.8f, 0.2f, 0.8f);
            case NotificationType.Unlock: return new Color(0.2f, 0.2f, 0.8f, 0.8f);
            default: return new Color(0.5f, 0.5f, 0.5f, 0.8f);
        }
    }
    
    private IEnumerator FadeInAndOut(float displayTime)
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Fade in
        canvasGroup.alpha = 0f;
        float fadeTime = 0.3f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = elapsedTime / fadeTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        
        // Wait
        yield return new WaitForSeconds(displayTime - fadeTime * 2f);
        
        // Fade out
        elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsedTime / fadeTime);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }
}
