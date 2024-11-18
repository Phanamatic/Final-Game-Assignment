using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using SFB;
using Photon.Pun;
using Photon.Realtime;

public class ProfileManager : MonoBehaviourPunCallbacks
{
    // UI References
    public GameObject profilePanel;
    public GameObject menuPanel;
    public GameObject matchmakingPanel;

    public TMP_InputField usernameInput;
    public Image[] profileIconButtons;
    public Button uploadButton;
    public Image selectedProfileIcon;
    public TMP_Text menuUsername;
    public Image menuProfileIcon;
    public TMP_Text matchmakingUsername;
    public Image matchmakingProfileIcon;
    public TMP_Text fileNameText;

    private Sprite selectedIcon;
    private string username;
    private string customImagePath;

    private const string customImageFileKey = "CustomProfileImagePath";

    private void Start()
    {
        // Check if profile is already set
        if (PlayerPrefs.HasKey("PlayerUsername"))
        {
            LoadProfile();
            ShowMenuPanel();
        }
        else
        {
            OpenProfilePanel();
        }
    }

    private void LoadProfile()
    {
        username = PlayerPrefs.GetString("PlayerUsername", "");
        selectedIcon = LoadSavedIcon();

        // Update UI elements with loaded data
        menuUsername.text = username;
        menuProfileIcon.sprite = selectedIcon;
        matchmakingUsername.text = username;
        matchmakingProfileIcon.sprite = selectedIcon;
        PhotonNetwork.LocalPlayer.NickName = username;

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "PlayerIcon", customImagePath ?? "Preset" },
            { "PlayerUsername", username }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
    }

    public void SelectProfileIcon(int index)
    {
        customImagePath = null;
        PlayerPrefs.DeleteKey(customImageFileKey);
        fileNameText.text = "";

        selectedIcon = profileIconButtons[index].sprite;
        selectedProfileIcon.sprite = selectedIcon;
    }

    public void UploadProfileIcon()
    {
        var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Profile Image", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            string filePath = paths[0];

            if (File.Exists(filePath))
            {
                byte[] imageBytes = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);

                selectedIcon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                selectedProfileIcon.sprite = selectedIcon;
                customImagePath = filePath;

                fileNameText.text = Path.GetFileName(filePath);
                PlayerPrefs.SetString(customImageFileKey, customImagePath);
            }
        }
    }

    public void SaveProfile()
    {
        username = usernameInput.text;
        PlayerPrefs.SetString("PlayerUsername", username);

        if (customImagePath != null)
        {
            SaveCustomImage(selectedIcon);
            PlayerPrefs.SetString(customImageFileKey, customImagePath);
        }
        else
        {
            int selectedIndex = System.Array.IndexOf(profileIconButtons, selectedProfileIcon);
            PlayerPrefs.SetString("SelectedProfileIcon", selectedIndex.ToString());
        }

        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "PlayerIcon", customImagePath ?? "Preset" },
            { "PlayerUsername", username }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

        UpdateProfileDisplays();
        ShowMenuPanel();
    }

    private void UpdateProfileDisplays()
    {
        menuUsername.text = username;
        menuProfileIcon.sprite = selectedIcon;
        matchmakingUsername.text = username;
        matchmakingProfileIcon.sprite = selectedIcon;
    }

    public void OpenProfilePanel()
    {
        profilePanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void ShowMenuPanel()
    {
        profilePanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void OpenMatchmakingPanel()
    {
        menuPanel.SetActive(false);
        matchmakingPanel.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SaveCustomImage(Sprite icon)
    {
        string profileImageSavePath = "ProfileImages";
        if (!Directory.Exists(profileImageSavePath))
        {
            Directory.CreateDirectory(profileImageSavePath);
        }

        Texture2D texture = icon.texture;
        byte[] bytes = texture.EncodeToPNG();
        string filePath = Path.Combine(profileImageSavePath, "custom_profile_image.png");

        File.WriteAllBytes(filePath, bytes);
        PlayerPrefs.SetString(customImageFileKey, filePath);
    }

    private Sprite LoadSavedIcon()
    {
        string savedCustomImagePath = PlayerPrefs.GetString(customImageFileKey, null);

        if (!string.IsNullOrEmpty(savedCustomImagePath) && File.Exists(savedCustomImagePath))
        {
            byte[] imageBytes = File.ReadAllBytes(savedCustomImagePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);
            fileNameText.text = Path.GetFileName(savedCustomImagePath);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        return profileIconButtons[0].sprite;
    }
}
