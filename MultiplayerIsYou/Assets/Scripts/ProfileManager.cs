using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO; // For file operations
using SFB;
using Photon.Pun;
using Photon.Realtime;


public class ProfileManager : MonoBehaviour
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
    public TMP_Text fileNameText;       

    private Sprite selectedIcon;       
    private Sprite originalIcon;      
    private string username;          
    private string originalUsername;    

    private string customImagePath;   
    private string savedImagePath;      

    private const string customImageFileKey = "CustomProfileImagePath";
    private const string profileImageSavePath = "ProfileImages"; 

    private void Start()
    {
        // Load saved profile data if there is 
        username = PlayerPrefs.GetString("PlayerUsername", "Username");
        originalUsername = username;

        selectedIcon = LoadSavedIcon();
        originalIcon = selectedIcon;

        menuUsername.text = username;
        menuProfileIcon.sprite = selectedIcon;
    }

    public void SelectProfileIcon(int index)
    {
        // Clear any custom image path when selecting one of the preset icon
        customImagePath = null;
        PlayerPrefs.DeleteKey(customImageFileKey);
        fileNameText.text = "";

        selectedIcon = profileIconButtons[index].sprite;
        selectedProfileIcon.sprite = selectedIcon;
    }

    public void UploadProfileIcon()   // To upload custom icon using StandaloneFileBrowser package
{
    var extensions = new[] {
        new ExtensionFilter("Image Files", "png", "jpg", "jpeg"),
    };
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

            Debug.Log("Custom icon uploaded successfully.");

            customImagePath = filePath;
            PlayerPrefs.SetString(customImageFileKey, customImagePath);

            fileNameText.text = Path.GetFileName(filePath);
        }
        else
        {
            Debug.LogError("File does not exist at the selected path.");
        }
    }
    else
    {
        Debug.LogWarning("No file selected.");
    }
}


    public void SaveProfile()
{
    username = usernameInput.text;      // Need to implement saving player properties to proton, currently just saves to local playerprefs
    PlayerPrefs.SetString("PlayerUsername", username);

    if (customImagePath != null)
    {
        SaveCustomImage(selectedIcon);
        PlayerPrefs.SetString("CustomProfileImagePath", customImagePath);
    }
    else
    {
        int selectedIndex = System.Array.IndexOf(profileIconButtons, selectedProfileIcon);
        PlayerPrefs.SetString("SelectedProfileIcon", selectedIndex.ToString());
        PlayerPrefs.DeleteKey("CustomProfileImagePath");
    }

    // Set the player’s profile info in Photon as custom properties  ----   Need to refine this, currently not working 100%
    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable
    {
        { "PlayerIcon", customImagePath ?? "Preset" },
        { "PlayerUsername", username }
    };

    PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);

    menuUsername.text = username;
    menuProfileIcon.sprite = selectedIcon;

    profilePanel.SetActive(false);
    menuPanel.SetActive(true);
}





    public void CancelProfileChanges()
    {
        // Revert to the original profile data (before edits)
        selectedProfileIcon.sprite = originalIcon;
        usernameInput.text = originalUsername;

        fileNameText.text = customImagePath == null ? "" : Path.GetFileName(customImagePath);

        profilePanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void OpenProfilePanel()
    {
        // load the current data (before any edits)
        usernameInput.text = originalUsername;
        selectedProfileIcon.sprite = originalIcon;

        menuPanel.SetActive(false);
        profilePanel.SetActive(true);
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
        Debug.Log("Custom profile icon loaded from: " + savedCustomImagePath);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
    }

    Debug.Log("No custom icon found, loading default icon.");
    return profileIconButtons[0].sprite;
}

}
