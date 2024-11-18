using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyMenuManager : MonoBehaviour
{
    public InputField lobbyNameInput;
    public Toggle publicToggle, privateToggle;
    public InputField passwordInput;

    public void CreateLobby()
    {
        string lobbyName = lobbyNameInput.text;
        bool isPrivate = privateToggle.isOn;
        string password = isPrivate ? passwordInput.text : null;

        if (string.IsNullOrEmpty(lobbyName) || (isPrivate && string.IsNullOrEmpty(password)))
        {
           // FindObjectOfType<GameManager>().ShowErrorPanel("Lobby name and password (if private) are required.");
            return;
        }

        FindObjectOfType<MatchmakingManager>().CreateLobby(lobbyName, isPrivate, password);
    }
}
