using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System.Collections;

public class WinChecker : MonoBehaviourPun
{
    public TextMeshProUGUI winText1;
    public TextMeshProUGUI defeatText1;
    public TextMeshProUGUI countdownText;

    public float checkRadius = 0.2f;
    private bool coroutineStarted = false;

    void Start()
    {
        winText1.gameObject.SetActive(false);
        defeatText1.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

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
            int touchingObjectViewID = touchingDefeatObj.GetComponent<PhotonView>().ViewID;
            photonView.RPC("HandleDefeat", RpcTarget.All, touchingObjectViewID);
        }

        // Check if player presses "R" to reload the scene
        if (Input.GetKeyDown(KeyCode.R) && IsRestartEnabled())
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
                Collider2D[] colliders = Physics2D.OverlapCircleAll(youObj.transform.position, checkRadius);

                foreach (Collider2D collider in colliders)
                {
                    if (collider.gameObject == targetObj)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

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
                        return youObj;
                    }
                }
            }
        }
        return null;
    }

    [PunRPC]
    void DisplayWinTexts()
    {
        winText1.gameObject.SetActive(true);

        if (!coroutineStarted)
        {
            coroutineStarted = true;
            StartCoroutine(ReturnToLevelSelectorAfterDelay(5f));
        }
    }

    [PunRPC]
    void HandleDefeat(int touchingObjectViewID)
    {
        PhotonView touchingObjectPV = PhotonView.Find(touchingObjectViewID);
        if (touchingObjectPV != null)
        {
            touchingObjectPV.gameObject.SetActive(false);
        }

        GameObject[] remainingYou1Objects = GameObject.FindGameObjectsWithTag("You1");
        GameObject[] remainingYou2Objects = GameObject.FindGameObjectsWithTag("You2");

        if (remainingYou1Objects.Length == 0 || remainingYou2Objects.Length == 0)
        {
            defeatText1.gameObject.SetActive(true);

            if (!coroutineStarted)
            {
                coroutineStarted = true;
                StartCoroutine(RestartLevelCountdown());
            }
        }
    }

    IEnumerator RestartLevelCountdown()
    {
        int countdown = 5;
        while (countdown > 0)
        {
            photonView.RPC("UpdateCountdownText", RpcTarget.All, countdown);
            yield return new WaitForSeconds(1f);
            countdown--;
        }
        photonView.RPC("RestartLevel", RpcTarget.All);
    }

    [PunRPC]
    void UpdateCountdownText(int countdown)
    {
        countdownText.gameObject.SetActive(true);
        string dots = new string('.', (5 - countdown) % 4);
        countdownText.text = $"Restarting level in {countdown}{dots}";
    }

    [PunRPC]
    void RestartLevel()
    {
        Time.timeScale = 1f; // Ensure time scale is reset
        PhotonNetwork.LoadLevel(PhotonNetwork.CurrentRoom.CustomProperties["CurrentLevel"].ToString());
    }

    IEnumerator ReturnToLevelSelectorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("LevelSelector");
        }
    }

    void ReloadScene()
    {
        PhotonNetwork.LoadLevel(PhotonNetwork.CurrentRoom.CustomProperties["CurrentLevel"].ToString());
    }

    bool IsRestartEnabled()
    {
        return PlayerPrefs.GetInt("RestartEnabled", 1) == 1;
    }
}
