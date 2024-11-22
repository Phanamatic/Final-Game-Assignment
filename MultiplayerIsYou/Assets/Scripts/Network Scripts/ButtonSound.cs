using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Color originalColor;
    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage == null)
        {
            buttonImage = GetComponentInChildren<Image>();
        }

        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
        }
        else
        {
            Debug.LogError("ButtonSound: No Image component found on this GameObject or its children.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayHoverSound();
        }
        else
        {
            Debug.LogError("UISoundManager.Instance is null. Ensure UISoundManager is in the scene.");
        }

        if (buttonImage != null)
        {
            buttonImage.color = new Color(0.8f, 0.8f, 1f, originalColor.a); 
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonImage != null)
        {
            buttonImage.color = originalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance.PlayClickSound();
        }
        else
        {
            Debug.LogError("UISoundManager.Instance is null. Ensure UISoundManager is in the scene.");
        }
    }
}
