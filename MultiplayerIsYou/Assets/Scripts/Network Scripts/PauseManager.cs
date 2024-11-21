using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviourPunCallbacks
{
    public static PauseManager Instance;

    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject confirmMenuPanel; // For the confirmation dialog

    [Header("Pause Menu Elements")]
    public TMP_Text pauseNotificationText;
    public Button resumeButton;
    public Button settingsButton;
    public Button restartLevelButton;
    public Button loadLevelSelectorButton;
    public Button menuButton;

    [Header("Settings Panel Elements")]
    public Slider volumeSlider;
    public Toggle restartToggle;
    public Button closeSettingsButton;

    [Header("Confirmation Panel Elements")]
    public TMP_Text disconnectingText; // Assign in the inspector
    public Button confirmButton;
    public Button cancelButton;

    [Header("Additional UI Elements")]
    public TMP_Text changeLevelHoverText;

    public bool isGamePaused = false; // Made public for access in other scripts

    private Coroutine disconnectingCoroutine;

    void Awake()
{
    // Singleton pattern to ensure only one instance exists
    if (Instance == null)
        Instance = this;
    else
        Destroy(gameObject);

    // Enable automatic scene synchronization
    PhotonNetwork.AutomaticallySyncScene = true;
}

    void Start()
    {
        // Check if photonView is valid
        if (photonView == null)
        {
            Debug.LogError("photonView is null in PauseManager. Please attach a PhotonView component to the GameObject.");
        }

        // Initially hide panels and texts
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        confirmMenuPanel.SetActive(false);
        changeLevelHoverText.gameObject.SetActive(false);
        disconnectingText.gameObject.SetActive(false);

        // Add button listeners
        resumeButton.onClick.AddListener(OnResumeButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        restartLevelButton.onClick.AddListener(OnRestartLevelButtonClicked);
        loadLevelSelectorButton.onClick.AddListener(OnLoadLevelSelectorButtonClicked);
        menuButton.onClick.AddListener(OnMenuButtonClicked);

        closeSettingsButton.onClick.AddListener(OnCloseSettingsButtonClicked);

        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);

        // Add hover listeners to the "Change Level" button
        EventTrigger trigger = loadLevelSelectorButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((eventData) => { OnLoadLevelButtonHoverEnter(); });
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((eventData) => { OnLoadLevelButtonHoverExit(); });
        trigger.triggers.Add(entryExit);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isGamePaused)
            {
                // Player wants to pause the game
                photonView.RPC("PauseGame", RpcTarget.All, PhotonNetwork.NickName);
            }
            else
            {
                // Player wants to resume the game
                OnResumeButtonClicked();
            }
        }
    }

    [PunRPC]
    public void PauseGame(string pausingPlayerName)
    {
        isGamePaused = true;
        // Game pauses by showing the pause panel for all players
        pauseMenuPanel.SetActive(true);
        pauseNotificationText.text = $"{pausingPlayerName} paused the game!";
    }

    public void OnResumeButtonClicked()
    {
        // Any player can resume the game for all players
        photonView.RPC("ResumeGame", RpcTarget.All);
    }

    [PunRPC]
    public void ResumeGame()
    {
        isGamePaused = false;
        // Game unpauses by hiding the pause panel for all players
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void OnSettingsButtonClicked()
    {
        settingsPanel.SetActive(true);
    }

    public void OnCloseSettingsButtonClicked()
    {
        settingsPanel.SetActive(false);
    }

    public void OnRestartLevelButtonClicked()
    {
        // Any player can request to restart the level
        photonView.RPC("RequestRestartLevel", RpcTarget.MasterClient);
    }

    [PunRPC]
void RequestRestartLevel()
{
    if (PhotonNetwork.IsMasterClient)
    {
        Debug.Log("MasterClient is requesting to restart the level.");

        // Unpause the game and reset states for all clients
        photonView.RPC("ResetGameStates", RpcTarget.All);

        // Then load the current level
        string currentLevel = SceneManager.GetActiveScene().name;
        Debug.Log($"Loading level: {currentLevel}");
        PhotonNetwork.LoadLevel(currentLevel);
    }
}


    [PunRPC]
public void ResetGameStates()
{
    Debug.Log("Resetting game states for all clients.");
    // Reset game states
    isGamePaused = false;
    pauseMenuPanel.SetActive(false);
    settingsPanel.SetActive(false);
}

    public void OnLoadLevelSelectorButtonClicked()
    {
        // Any player can request to load the level selector
        photonView.RPC("RequestLoadLevelSelector", RpcTarget.MasterClient);
    }

    [PunRPC]
    void RequestLoadLevelSelector()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Unpause the game and reset states for all clients
            photonView.RPC("ResetGameStates", RpcTarget.All);

            // Then load the LevelSelector scene
            PhotonNetwork.LoadLevel("LevelSelector");
        }
    }

    public void OnMenuButtonClicked()
    {
        confirmMenuPanel.SetActive(true);
    }

    public void OnConfirmButtonClicked()
    {
        // Show the disconnecting text
        disconnectingText.gameObject.SetActive(true);

        // Start the coroutine to animate the dots
        if (disconnectingCoroutine != null)
            StopCoroutine(disconnectingCoroutine);

        disconnectingCoroutine = StartCoroutine(AnimateDisconnectingText());

        DisconnectAndReturnToMenu();
    }

    private IEnumerator AnimateDisconnectingText()
    {
        string baseText = "Disconnecting";
        int dotCount = 0;
        while (true)
        {
            dotCount = (dotCount % 3) + 1;
            string dots = new string('.', dotCount);
            disconnectingText.text = $"{baseText}{dots}";
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void DisconnectAndReturnToMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        if (disconnectingCoroutine != null)
        {
            StopCoroutine(disconnectingCoroutine);
            disconnectingCoroutine = null;
        }

        // Set the flag to indicate we are returning from the game
        GameState.IsReturningFromGame = true;

        SceneManager.LoadScene("Menu_Scene"); // Replace with your main menu scene name
    }

    public void OnCancelButtonClicked()
    {
        confirmMenuPanel.SetActive(false);
    }

    public void OnLoadLevelButtonHoverEnter()
    {
        // Since both players can change the level, we hide the hover text
        changeLevelHoverText.gameObject.SetActive(false);
    }

    public void OnLoadLevelButtonHoverExit()
    {
        changeLevelHoverText.gameObject.SetActive(false);
    }

    public void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }
}
