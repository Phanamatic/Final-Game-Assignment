using TMPro;
using UnityEngine;

public class WinChecker : MonoBehaviour
{
    public TextMeshProUGUI winText1;
    public TextMeshProUGUI winText2;

    // Radius for OverlapCircle to detect proximity
    public float checkRadius = 0.5f;

    void Start()
    {
        // Initially hide the texts
        winText1.gameObject.SetActive(false);
        winText2.gameObject.SetActive(false);
    }

    void Update()
    {
        // Check if any object with tag "You1" or "You2" is near an object with the tag "Win"
        GameObject[] you1Objects = GameObject.FindGameObjectsWithTag("You1");
        GameObject[] you2Objects = GameObject.FindGameObjectsWithTag("You2");
        GameObject[] winObjects = GameObject.FindGameObjectsWithTag("Win");

        bool isTouching = CheckOverlap(you1Objects, winObjects) || CheckOverlap(you2Objects, winObjects);

        if (isTouching)
        {
            DisplayWinTexts();
        }
    }

    bool CheckOverlap(GameObject[] youObjects, GameObject[] winObjects)
    {
        foreach (GameObject youObj in youObjects)
        {
            foreach (GameObject winObj in winObjects)
            {
                // Use OverlapCircle to detect proximity
                Collider2D[] colliders = Physics2D.OverlapCircleAll(youObj.transform.position, checkRadius);

                foreach (Collider2D collider in colliders)
                {
                    if (collider.gameObject == winObj) // Check if the win object is within range
                    {
                        return true; // Detected proximity between You and Win objects
                    }
                }
            }
        }
        return false;
    }

    void DisplayWinTexts()
    {
        winText1.gameObject.SetActive(true); // Show the first text
        winText2.gameObject.SetActive(true); // Show the second text
    }
}
