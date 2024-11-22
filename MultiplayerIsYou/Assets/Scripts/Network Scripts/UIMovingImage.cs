using UnityEngine;
using UnityEngine.UI;

public class UIMovingImage : MonoBehaviour
{
    public RectTransform rectTransform;
    public Vector2 moveDirection;
    public float speed = 50f;

    private RectTransform canvasRect;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        moveDirection = Random.insideUnitCircle.normalized;
    }

    void Update()
    {
        rectTransform.anchoredPosition += moveDirection * speed * Time.deltaTime;
        CheckBounds();
    }

    void CheckBounds()
    {
        Vector2 pos = rectTransform.anchoredPosition;
        Vector2 size = rectTransform.sizeDelta;
        Vector2 canvasSize = canvasRect.sizeDelta;

        if (pos.x - size.x / 2 < -canvasSize.x / 2 || pos.x + size.x / 2 > canvasSize.x / 2)
        {
            moveDirection.x = -moveDirection.x;
        }

        if (pos.y - size.y / 2 < -canvasSize.y / 2 || pos.y + size.y / 2 > canvasSize.y / 2)
        {
            moveDirection.y = -moveDirection.y;
        }
    }
}
