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
        winText1.gameObject.SetActive(false);
        winText2.gameObject.SetActive(false);
        defeatText1.gameObject.SetActive(false);
        defeatText2.gameObject.SetActive(false);
    }

    void Update()
    {
        if (photonView.IsMine) // Only run the check on the master client
        {
            CheckWinAndDefeatConditions();
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
        winText2.gameObject.SetActive(true);
    }

    [PunRPC]
    void HandleDefeat(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            targetView.gameObject.SetActive(false);
        }

        GameObject[] remainingYou1Objects = GameObject.FindGameObjectsWithTag("You1");
        GameObject[] remainingYou2Objects = GameObject.FindGameObjectsWithTag("You2");

        if (remainingYou1Objects.Length == 0 || remainingYou2Objects.Length == 0)
        {
            defeatText1.gameObject.SetActive(true);
            defeatText2.gameObject.SetActive(true);
        }
    }

    void ReloadScene()
    {
        if (photonView.IsMine) // Only reload on master client
        {
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().name);
        }
    }
}
