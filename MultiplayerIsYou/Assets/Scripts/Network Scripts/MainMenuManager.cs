using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using SFB;
using PlayFab;
using PlayFab.ClientModels;
using ExitGames.Client.Photon;
using System.Collections.Generic;


public class MainMenuManager : MonoBehaviourPunCallbacks
{
    [Header("Main Menu Elements")]
    public GameObject menuPanel;
    public GameObject matchmakingPanel;
    public GameObject profilePanel;

    [Header("Profile Menu Elements")]
    public Button[] profileIconButtons; // Assign the 5 preset icon buttons here
    public Button uploadIconButton;    // Assign the upload button here
    public TMP_InputField usernameInput; // Assign the username input field here
    public Button saveButton;         // Assign the save button here
    public Image selectedIconDisplay; // Display for the selected icon
    private Sprite selectedIcon;

    [Header("Player Info")]
    public TMP_Text usernameText;
    public Image profileIconImage;

    private const string bucketName = "multiplayerisyou";
    private AmazonS3Client s3Client = new AmazonS3Client(
        "AKIARHQBNWJCXTWXEVQP",
        "W1uqH1G63xuqkq8CSXUFmJ9KaTD/TP6bL4A/Z8ut",
        RegionEndpoint.EUNorth1
    );

    private void Start()
    {
        // Set the OnClick listeners in the Unity Inspector
        UpdateMenuUI();
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (targetPlayer.IsLocal)
        {
            UpdateMenuUI();
        }
    }

    public async void UpdateMenuUI()
    {
        // Check and update the username
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Username"))
        {
            usernameText.text = PhotonNetwork.LocalPlayer.CustomProperties["Username"].ToString();
        }
        else
        {
            usernameText.text = "Guest";
        }

        // Check and update the profile icon
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Icon"))
        {
            string url = PhotonNetwork.LocalPlayer.CustomProperties["Icon"].ToString();
            Texture2D texture = await DownloadImageFromS3(url);
            if (texture != null)
            {
                profileIconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            }
        }
        else
        {
            profileIconImage.sprite = null; // Optionally, set a default icon here
        }
    }

    public void OpenMatchmakingPanel()
    {
        menuPanel.SetActive(false);
        matchmakingPanel.SetActive(true);
    }

    public void OpenProfilePanel()
    {
        menuPanel.SetActive(false);
        profilePanel.SetActive(true);
    }

    public void QuitGame()
{
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    Debug.Log("Game Quit");
}


    public void OnIconSelected(Button iconButton)
    {
        selectedIcon = iconButton.GetComponent<Image>().sprite;
        selectedIconDisplay.sprite = selectedIcon;
    }

    public void OnUploadIconClicked()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Profile Icon", "", "png", false);
        if (paths.Length > 0)
        {
            string path = paths[0];
            Texture2D texture = LoadTexture(path);
            Sprite newIcon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            selectedIcon = newIcon;
            selectedIconDisplay.sprite = newIcon;
        }
    }

    public async void OnSaveProfileClicked()
{
    // Update icon
    if (selectedIcon != null)
    {
        string filePath = await SaveSpriteToLocal(selectedIcon);
        string iconUrl = await UploadIconToS3(filePath);
        UpdateIconInPhoton(iconUrl);
    }

    // Ensure UI reflects updated data
    UpdateMenuUI();

    profilePanel.SetActive(false);
    menuPanel.SetActive(true);
}



    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    private async Task<string> SaveSpriteToLocal(Sprite sprite)
{
    string filePath = Path.Combine(Application.persistentDataPath, "temp_profile_picture.png");

    // Ensure the sprite's texture is readable
    Texture2D texture = MakeTextureReadable(sprite.texture);

    if (texture == null)
    {
        Debug.LogError("Failed to make the sprite texture readable.");
        return null;
    }

    // Encode the texture to PNG
    byte[] imageData = texture.EncodeToPNG();

    if (imageData == null || imageData.Length == 0)
    {
        Debug.LogError("Failed to encode texture to PNG.");
        return null;
    }

    // Save the PNG data to a file
    try
    {
        await File.WriteAllBytesAsync(filePath, imageData);
        Debug.Log($"Sprite saved to local file: {filePath}");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error saving sprite to local file: {ex.Message}");
        return null;
    }

    return filePath;
}



    private async Task<string> UploadIconToS3(string filePath)
    {
        string key = $"{PhotonNetwork.NickName}/profile_picture.png";

        try
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                FilePath = filePath,
                ContentType = "image/png"
            };

            await s3Client.PutObjectAsync(putRequest);
            Debug.Log("Icon uploaded to S3 successfully.");
            return $"https://{bucketName}.s3.{Amazon.RegionEndpoint.EUNorth1.SystemName}.amazonaws.com/{key}";
        }
        catch (AmazonS3Exception ex)
        {
            Debug.LogError($"Error uploading icon to S3: {ex.Message}");
            return null;
        }
    }

    private void UpdateIconInPhoton(string iconUrl)
    {
        if (!string.IsNullOrEmpty(iconUrl))
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "Icon", iconUrl }
            });
            Debug.Log("Icon updated in Photon.");
        }
    }

    private async Task UpdateUsernameInPlayFab(string username)
{
    if (string.IsNullOrEmpty(username) || username.Length < 3 || username.Length > 25)
    {
        Debug.LogError("Invalid username. It must be between 3 and 25 characters.");
        return;
    }

    var updateDataRequest = new UpdateUserDataRequest
    {
        Data = new Dictionary<string, string>
        {
            { "NewUsername", username } // Update NewUsername field
        }
    };

    var taskCompletionSource = new TaskCompletionSource<bool>();

    PlayFabClientAPI.UpdateUserData(updateDataRequest, result =>
    {
        PhotonNetwork.NickName = username;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
        {
            { "Username", username }
        });
        Debug.Log("New username updated in PlayFab and Photon.");
        taskCompletionSource.SetResult(true);
    }, error =>
    {
        Debug.LogError($"Error updating username in PlayFab: {error.ErrorMessage}");
        taskCompletionSource.SetResult(false);
    });

    await taskCompletionSource.Task;
}




    private async Task<Texture2D> DownloadImageFromS3(string url)
    {
        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = new Uri(url).LocalPath.TrimStart('/')
            };

            using (var response = await s3Client.GetObjectAsync(getRequest))
            using (var stream = new MemoryStream())
            {
                await response.ResponseStream.CopyToAsync(stream);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(stream.ToArray());
                return texture;
            }
        }
        catch (AmazonS3Exception ex)
        {
            Debug.LogError($"Error downloading image from S3: {ex.Message}");
            return null;
        }
    }

    private Texture2D MakeTextureReadable(Texture2D originalTexture)
{
    Debug.Log($"Texture Format: {originalTexture.format}, Width: {originalTexture.width}, Height: {originalTexture.height}, Readable: {originalTexture.isReadable}");

    if (originalTexture.isReadable)
    {
        return originalTexture; // Return if the texture is already readable
    }

    try
    {
        RenderTexture tempRenderTexture = RenderTexture.GetTemporary(
            originalTexture.width,
            originalTexture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear);

        Graphics.Blit(originalTexture, tempRenderTexture);

        RenderTexture previousRenderTexture = RenderTexture.active;
        RenderTexture.active = tempRenderTexture;

        Texture2D readableTexture = new Texture2D(originalTexture.width, originalTexture.height, TextureFormat.RGBA32, false);
        readableTexture.ReadPixels(new Rect(0, 0, tempRenderTexture.width, tempRenderTexture.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previousRenderTexture;
        RenderTexture.ReleaseTemporary(tempRenderTexture);

        return readableTexture;
    }
    catch (Exception ex)
    {
        Debug.LogError($"Error making texture readable: {ex.Message}");
        return null;
    }
}



}
