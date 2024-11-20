using TMPro;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class WinChecker : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI winText1;
    public TextMeshProUGUI winText2;
    public TextMeshProUGUI defeatText1;
    public TextMeshProUGUI defeatText2;
    public float checkRadius = 0.2f;

    void Start()
    {
        // Initially hide all texts
        winText1.gameObject.SetActive(false);
        winText2.gameObject.SetActive(false);
        defeatText1.gameObject.SetActive(false);
        defeatText2.gameObject.SetActive(false);
    }

    void Update()
    {
        if (photonView.IsMine) // Only run the checks on the master client
        {
            CheckWinAndDefeatConditions();
        }

        // Allow any player to reload the scene on pressing "R"
        if (Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }
    }

    void CheckWinAndDefeatConditions()
    {
        GameObject[] you1Objects = GameObject.FindGameObjectsWithTag("You1");
        GameObject[] you2Objects = GameObject.FindGameObjectsWithTag("You2");
        GameObject[] winObjects = GameObject.FindGameObjectsWithTag("Win");
        GameObject[] defeatObjects = GameObject.FindGameObjectsWithTag("Defeat");

        bool isTouchingWin = CheckOverlap(you1Objects, winObjects) || CheckOverlap(you2Objects, winObjects);
        GameObject touchingDefeatObj = GetTouchingDefeat(you1Objects, defeatObjects) ?? GetTouchingDefeat(you2Objects, defeatObjects);

        if (isTouchingWin)
        {
            photonView.RPC("DisplayWinTexts", RpcTarget.All);
        }

        if (touchingDefeatObj != null)
        {
            photonView.RPC("HandleDefeat", RpcTarget.All, touchingDefeatObj.GetComponent<PhotonView>().ViewID);
        }
    }

    // Check if any "You" object is overlapping with a target object (e.g., "Win")
    bool CheckOverlap(GameObject[] youObjects, GameObject[] targetObjects)
    {
        foreach (GameObject youObj in youObjects)
        {
            foreach (GameObject targetObj in targetObjects)
            {
                Collider2D[] colliders = Physics2D.OverlapCircleAll(youObj.transform.position, checkRadius);
                foreach (Collider2D collider in colliders)
                {
                    if (collider.gameObject == targetObj)
                    {
                        return true; // Proximity detected
                    }
                }
            }
        }
        return false;
    }

    // Get the "You" object that is touching a "Defeat" object
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
                        return youObj; // Return the touching "You" object
                    }
                }
            }
        }
        return null; // No overlap found
    }

    [PunRPC]
    void DisplayWinTexts()
    {
        // Show win texts for both players
        winText1.gameObject.SetActive(true);
        winText2.gameObject.SetActive(true);
    }

    [PunRPC]
    void HandleDefeat(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            targetView.gameObject.SetActive(false); // Deactivate the object that touched a "Defeat" object
        }

        // Check if any "You1" or "You2" objects remain in the scene
        GameObject[] remainingYou1Objects = GameObject.FindGameObjectsWithTag("You1");
        GameObject[] remainingYou2Objects = GameObject.FindGameObjectsWithTag("You2");

        if (remainingYou1Objects.Length == 0 || remainingYou2Objects.Length == 0)
        {
            // Show defeat texts if no "You1" or "You2" objects remain
            defeatText1.gameObject.SetActive(true);
            defeatText2.gameObject.SetActive(true);
        }
    }

    void ReloadScene()
    {
        if (photonView.IsMine) // Ensure only the master client reloads the scene
        {
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        }
    }
}
