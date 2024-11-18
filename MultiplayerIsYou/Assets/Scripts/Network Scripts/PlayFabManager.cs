using PlayFab;
using UnityEngine;

public class PlayFabManager : MonoBehaviour
{
    void Start()
    {
        // Ensure PlayFab is initialized with the shared settings
        Debug.Log("PlayFab initialized with Title ID: " + PlayFabSettings.staticSettings.TitleId);
    }
}
