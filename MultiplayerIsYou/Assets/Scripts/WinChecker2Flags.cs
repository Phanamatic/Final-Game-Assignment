using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Import SceneManagement
public class WinChecker2Flags : MonoBehaviour
{
    public TextMeshProUGUI winText1;

    public TextMeshProUGUI defeatText1; // For defeat condition

    // Radius for OverlapCircle to detect proximity
    public float checkRadius = 0.2f;

    void Start()
    {
        // Initially hide the win and defeat texts
        winText1.gameObject.SetActive(false);
        defeatText1.gameObject.SetActive(false);
    }

    void Update()
    {
        // Check if any object with tag "You1" or "You2" is near an object with the tag "Win"
        GameObject[] you1Objects = GameObject.FindGameObjectsWithTag("You1");
        GameObject[] you2Objects = GameObject.FindGameObjectsWithTag("You2");
        GameObject[] winObjects = GameObject.FindGameObjectsWithTag("Win");
        GameObject[] defeatObjects = GameObject.FindGameObjectsWithTag("Defeat");

        // Both You1 and You2 must be touching Win for the win text to appear
        bool isYou1TouchingWin = CheckOverlap(you1Objects, winObjects);
        bool isYou2TouchingWin = CheckOverlap(you2Objects, winObjects);

        // Check for defeat conditions
        GameObject touchingDefeatObj = GetTouchingDefeat(you1Objects, defeatObjects) ?? GetTouchingDefeat(you2Objects, defeatObjects);

        if (isYou1TouchingWin && isYou2TouchingWin)
        {
            DisplayWinTexts();
        }

        if (touchingDefeatObj != null)
        {
            HandleDefeat(touchingDefeatObj);
        }

        // Check if player presses "R" to reload the scene
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }
    }

    bool CheckOverlap(GameObject[] youObjects, GameObject[] targetObjects)
    {
        foreach (GameObject youObj in youObjects)
        {
            foreach (GameObject targetObj in targetObjects)
            {
                // Use OverlapCircle to detect proximity
                Collider2D[] colliders = Physics2D.OverlapCircleAll(youObj.transform.position, checkRadius);

                foreach (Collider2D collider in colliders)
                {
                    if (collider.gameObject == targetObj) // Check if the target object (Win or Defeat) is within range
                    {
                        return true; // Detected proximity between You and target objects
                    }
                }
            }
        }
        return false;
    }

    // New method to return the object that is touching the defeat object
    GameObject GetTouchingDefeat(GameObject[] youObjects, GameObject[] defeatObjects)
    {
        foreach (GameObject youObj in youObjects)
        {
            foreach (GameObject defeatObj in defeatObjects)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(youObj.transform.position, checkRadius);

                foreach (Collider2D collider in colliders)
                {
                    if (collider.gameObject == defeatObj)
                    {
                        return youObj; // Return the You object that touched the Defeat object
                    }
                }
            }
        }
        return null; // No object found touching defeat
    }

    void DisplayWinTexts()
    {
        winText1.gameObject.SetActive(true); // Show the first win text
    }

    void HandleDefeat(GameObject touchingObject)
    {
        // Deactivate the object that touched the defeat object
        touchingObject.SetActive(false);

        // Check if there are no more You1 or You2 objects left in the scene
        GameObject[] remainingYou1Objects = GameObject.FindGameObjectsWithTag("You1");
        GameObject[] remainingYou2Objects = GameObject.FindGameObjectsWithTag("You2");

        if (remainingYou1Objects.Length == 0 || remainingYou2Objects.Length == 0)
        {
            // Display defeat texts if no more You1 or You2 objects remain
            defeatText1.gameObject.SetActive(true);
        }
    }

    // Method to reload the current scene when the player presses "R"
    void ReloadScene()
    {
        // Reload the currently active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
