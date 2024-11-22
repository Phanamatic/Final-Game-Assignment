using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using Photon.Voice.Unity;
using Photon.Voice.PUN;
using UnityEngine.Audio; // Added for AudioMixer

public class PauseManager : MonoBehaviourPunCallbacks
{
    public static PauseManager Instance;

    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject confirmMenuPanel; 

    [Header("Pause Menu Elements")]
    public TMP_Text pauseNotificationText;
    public Button resumeButton;
    public Button settingsButton;
    public Button restartLevelButton;
    public Button loadLevelSelectorButton;
    public Button menuButton;

    [Header("Settings Panel Elements")]
    public Slider masterVolumeSlider;      
    public Slider voiceChatVolumeSlider;   
    public Slider myVoiceVolumeSlider;     
    public Toggle muteToggle;              
    public Toggle restartToggle;
    public Button closeSettingsButton;

    [Header("New Settings Panel Elements")]
    public Slider voiceOutputVolumeSlider;  
    public Toggle micMuteToggle;           

    [Header("Confirmation Panel Elements")]
    public TMP_Text disconnectingText; 
    public Button confirmButton;
    public Button cancelButton;

    [Header("Additional UI Elements")]
    public TMP_Text changeLevelHoverText;

    [Header("Audio Mixer")]
    public AudioMixer voiceAudioMixer;

    public bool isGamePaused = false; 

    private Coroutine disconnectingCoroutine;

    private Recorder recorder;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        if (photonView == null)
        {
            Debug.LogError("photonView is null in PauseManager. Please attach a PhotonView component to the GameObject.");
        }

        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        confirmMenuPanel.SetActive(false);
        changeLevelHoverText.gameObject.SetActive(false);
        disconnectingText.gameObject.SetActive(false);

        resumeButton.onClick.AddListener(OnResumeButtonClicked);
        settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        restartLevelButton.onClick.AddListener(OnRestartLevelButtonClicked);
        loadLevelSelectorButton.onClick.AddListener(OnLoadLevelSelectorButtonClicked);
        menuButton.onClick.AddListener(OnMenuButtonClicked);

        closeSettingsButton.onClick.AddListener(OnCloseSettingsButtonClicked);

        confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);

        EventTrigger trigger = loadLevelSelectorButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entryEnter = new EventTrigger.Entry();
        entryEnter.eventID = EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((eventData) => { OnLoadLevelButtonHoverEnter(); });
        trigger.triggers.Add(entryEnter);

        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((eventData) => { OnLoadLevelButtonHoverExit(); });
        trigger.triggers.Add(entryExit);

        InitializeVolumeSliders();

        InitializeNewSettingsElements();

        InitializeRecorder();
    }

    void InitializeRecorder()
    {
        GameObject localPlayer = PhotonNetwork.LocalPlayer.TagObject as GameObject;

        if (localPlayer != null)
        {
            PhotonVoiceView voiceView = localPlayer.GetComponent<PhotonVoiceView>();
            if (voiceView != null)
            {
                recorder = voiceView.RecorderInUse;
            }
            else
            {
                Debug.LogError("PhotonVoiceView is not attached to the local player.");
            }
        }
        else
        {
            Debug.LogError("Local player GameObject is not set in PhotonNetwork.LocalPlayer.TagObject.");
        }

        if (recorder == null)
        {
            recorder = PunVoiceClient.Instance.PrimaryRecorder;
            if (recorder == null)
            {
                Debug.LogError("Recorder not found. Please ensure a Recorder component is attached to the local player or set as the Primary Recorder in PunVoiceClient.");
            }
        }
    }

    void InitializeVolumeSliders()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = AudioListener.volume;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        else
        {
            Debug.LogError("MasterVolumeSlider is not assigned in the Inspector.");
        }

        if (voiceChatVolumeSlider != null)
        {
            voiceChatVolumeSlider.value = 1f; 
            voiceChatVolumeSlider.onValueChanged.AddListener(OnVoiceChatVolumeChanged);
        }
        else
        {
            Debug.LogError("VoiceChatVolumeSlider is not assigned in the Inspector.");
        }

        if (myVoiceVolumeSlider != null)
        {
            myVoiceVolumeSlider.minValue = 0f;
            myVoiceVolumeSlider.maxValue = 1f;
            myVoiceVolumeSlider.value = 1f;
            myVoiceVolumeSlider.onValueChanged.AddListener(OnMyVoiceVolumeChanged);
        }
        else
        {
            Debug.LogError("MyVoiceVolumeSlider is not assigned in the Inspector.");
        }

        if (muteToggle != null)
        {
            muteToggle.isOn = false;
            muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
        }
        else
        {
            Debug.LogError("MuteToggle is not assigned in the Inspector.");
        }
    }

    void InitializeNewSettingsElements()
    {
        if (voiceOutputVolumeSlider != null)
        {
            voiceOutputVolumeSlider.value = 1f;
            voiceOutputVolumeSlider.onValueChanged.AddListener(OnVoiceOutputVolumeChanged);
        }
        else
        {
            Debug.LogError("VoiceOutputVolumeSlider is not assigned in the Inspector.");
        }

        if (micMuteToggle != null)
        {
            micMuteToggle.isOn = false;
            micMuteToggle.onValueChanged.AddListener(OnMicMuteToggleChanged);
        }
        else
        {
            Debug.LogError("MicMuteToggle is not assigned in the Inspector.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isGamePaused)
            {
                photonView.RPC("PauseGame", RpcTarget.All, PhotonNetwork.NickName);
            }
            else
            {
                OnResumeButtonClicked();
            }
        }
    }

    [PunRPC]
    public void PauseGame(string pausingPlayerName)
    {
        isGamePaused = true;
        pauseMenuPanel.SetActive(true);
        pauseNotificationText.text = $"{pausingPlayerName} paused the game!";
    }

    public void OnResumeButtonClicked()
    {
        photonView.RPC("ResumeGame", RpcTarget.All);
    }

    [PunRPC]
    public void ResumeGame()
    {
        isGamePaused = false;
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
        photonView.RPC("RequestRestartLevel", RpcTarget.MasterClient);
    }

    [PunRPC]
    void RequestRestartLevel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("MasterClient is requesting to restart the level.");

            photonView.RPC("ResetGameStates", RpcTarget.All);

            string currentLevel = SceneManager.GetActiveScene().name;
            Debug.Log($"Loading level: {currentLevel}");
            PhotonNetwork.LoadLevel(currentLevel);
        }
    }

    [PunRPC]
    public void ResetGameStates()
    {
        Debug.Log("Resetting game states for all clients.");
        isGamePaused = false;
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public void OnLoadLevelSelectorButtonClicked()
    {
        photonView.RPC("RequestLoadLevelSelector", RpcTarget.MasterClient);
    }

    [PunRPC]
    void RequestLoadLevelSelector()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ResetGameStates", RpcTarget.All);

            PhotonNetwork.LoadLevel("LevelSelector");
        }
    }

    public void OnMenuButtonClicked()
    {
        confirmMenuPanel.SetActive(true);
    }

    public void OnConfirmButtonClicked()
    {
        disconnectingText.gameObject.SetActive(true);

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

        GameState.IsReturningFromGame = true;

        SceneManager.LoadScene("Menu_Scene");
    }

    public void OnCancelButtonClicked()
    {
        confirmMenuPanel.SetActive(false);
    }

    public void OnLoadLevelButtonHoverEnter()
    {
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

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning($"Disconnected from Photon: {cause}");

        if (PhotonNetwork.InRoom)
        {
            GameState.WasInGameRoom = true;
            GameState.LastRoomName = PhotonNetwork.CurrentRoom.Name;
        }
    }


    public void OnMasterVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }

    public void OnVoiceChatVolumeChanged(float value)
    {
        var speakers = FindObjectsOfType<Speaker>();
        foreach (var speaker in speakers)
        {
            if (speaker != null && speaker.IsLinked)
            {
                AudioSource audioSource = speaker.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.volume = value;
                }
                else
                {
                    Debug.LogError("AudioSource component not found on Speaker.");
                }
            }
        }

        Debug.Log($"Voice chat playback volume set to: {value}");
    }

    public void OnMyVoiceVolumeChanged(float value)
    {
        if (voiceAudioMixer != null)
        {
            float mixerVolume = Mathf.Log10(value) * 20f;
            voiceAudioMixer.SetFloat("VoiceGroupVolume", mixerVolume);
            Debug.Log($"My voice volume set to: {value}");
        }
        else
        {
            Debug.LogError("VoiceAudioMixer is not assigned in the Inspector.");
        }
    }

    public void OnMuteToggleChanged(bool isMuted)
    {
        if (recorder != null)
        {
            recorder.TransmitEnabled = !isMuted;
            Debug.Log($"Mute Myself set to: {isMuted}");
        }
        else
        {
            Debug.LogError("Recorder is not initialized.");
        }
    }

    public void OnVoiceOutputVolumeChanged(float value)
    {
        var speakers = FindObjectsOfType<Speaker>();
        foreach (var speaker in speakers)
        {
            if (speaker != null && speaker.IsLinked)
            {
                AudioSource audioSource = speaker.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.volume = value;
                }
                else
                {
                    Debug.LogError("AudioSource component not found on Speaker.");
                }
            }
        }

        Debug.Log($"Voice output volume set to: {value}");
    }

    public void OnMicMuteToggleChanged(bool isMuted)
    {
        if (recorder != null)
        {
            recorder.TransmitEnabled = !isMuted;
            Debug.Log($"Microphone Muted: {isMuted}");
        }
        else
        {
            Debug.LogError("Recorder is not initialized.");
        }
    }
}
