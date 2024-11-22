using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;

public class VoiceChatManager : MonoBehaviourPunCallbacks
{
    private PhotonVoiceView voiceView;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        // Check if PunVoiceClient is connected
        if (!PunVoiceClient.Instance.Client.InRoom)
        {
            Debug.LogError("PunVoiceClient is not connected to a room.");
            return;
        }

        voiceView = GetComponent<PhotonVoiceView>();
        if (voiceView == null)
        {
            voiceView = gameObject.AddComponent<PhotonVoiceView>();
        }

        // EMake sure the Primary Recorder is used
        if (PunVoiceClient.Instance.PrimaryRecorder != null)
        {
            PunVoiceClient.Instance.PrimaryRecorder.TransmitEnabled = true;
            Debug.Log("Primary Recorder is set and transmitting.");
        }
        else
        {
            Debug.LogError("Primary Recorder is not set in PunVoiceClient. Please assign a Recorder.");
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room. Voice chat is active.");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} joined the room. Updating voice chat.");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} left the room. Updating voice chat.");
    }
}
