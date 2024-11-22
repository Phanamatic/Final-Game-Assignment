using UnityEngine;

public class UIBackgroundManager : MonoBehaviour
{
    public GameObject movingImagePrefab; 
    public int numberOfImages = 10;    

    void Start()
    {
        for (int i = 0; i < numberOfImages; i++)
        {
            GameObject img = Instantiate(movingImagePrefab, transform);
            RectTransform rt = img.GetComponent<RectTransform>();
            rt.anchoredPosition = RandomPosition();
        }
    }

    Vector2 RandomPosition()
    {
        RectTransform canvasRect = GetComponent<RectTransform>();
        float x = Random.Range(-canvasRect.sizeDelta.x / 2, canvasRect.sizeDelta.x / 2);
        float y = Random.Range(-canvasRect.sizeDelta.y / 2, canvasRect.sizeDelta.y / 2);
        return new Vector2(x, y);
    }
}
