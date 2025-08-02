using System.Collections;
using UnityEngine;
using TMPro;

// Damage number UI component
public class DamageNumberUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _text;
    
    public void Initialize(string text, Color color, float scale)
    {
        if (_text != null)
        {
            _text.text = text;
            _text.color = color;
            _text.transform.localScale = Vector3.one * scale;
        }
        
        StartCoroutine(AnimateDamageNumber());
    }
    
    private IEnumerator AnimateDamageNumber()
    {
        // Move up and fade out
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * 2f;
        
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        float duration = 1.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;
            
            transform.position = Vector3.Lerp(startPos, endPos, progress);
            canvasGroup.alpha = 1f - progress;
            
            yield return null;
        }
    }
}