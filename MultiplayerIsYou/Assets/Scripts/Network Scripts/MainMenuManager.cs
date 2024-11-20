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
using dotenv.net;
using Amazon.Runtime;
using System.Net; // For ServicePointManager

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    [Header("Main Menu Elements")]
    public GameObject menuPanel;
    public GameObject matchmakingPanel;
    public GameObject profilePanel;

    [Header("Profile Menu Elements")]
    public Button[] profileIconButtons; // Assign the preset icon buttons here
    public Button uploadIconButton;    // Assign the upload button here
    public TMP_InputField usernameInput; // Assign the username input field here
    public Button saveButton;         // Assign the save button here
    public Image selectedIconDisplay; // Display for the selected icon
    private Sprite selectedIcon;

    [Header("Player Info")]
    public TMP_Text usernameText;
    public Image profileIconImage;

    private const string bucketName = "multiplayerisyou";
    private AmazonS3Client s3Client;

    private Dictionary<string, Texture2D> iconCache = new Dictionary<string, Texture2D>();

    private void Awake()
    {
        // Ensure TLS 1.2 is used
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        // Load environment variables
        string envFilePath = "";

#if UNITY_EDITOR
        // In the Unity Editor, the project root is one level up from Application.dataPath
        envFilePath = Path.Combine(Application.dataPath, "..", ".env");
#else
        // In builds, include the .env file in StreamingAssets
        envFilePath = Path.Combine(Application.streamingAssetsPath, ".env");
#endif

        // Load environment variables from the specified path
        DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { envFilePath }));

        // Fetch AWS credentials from environment variables
        string accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
        string secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
        string region = Environment.GetEnvironmentVariable("AWS_REGION");

        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(region))
        {
            Debug.LogError("AWS credentials or region are not set in the environment variables.");
            return;
        }

        // Initialize AmazonS3Client with credentials from the environment
        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var regionEndpoint = RegionEndpoint.GetBySystemName(region);

        s3Client = new AmazonS3Client(credentials, regionEndpoint);
    }

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
        Debug.Log("UpdateMenuUI called.");

        // Check and update the username
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Username"))
        {
            usernameText.text = PhotonNetwork.LocalPlayer.CustomProperties["Username"].ToString();
            Debug.Log($"Username updated to: {usernameText.text}");
        }
        else
        {
            usernameText.text = "Guest";
            Debug.Log("Username set to Guest.");
        }

        // Check and update the profile icon
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Icon"))
        {
            string url = PhotonNetwork.LocalPlayer.CustomProperties["Icon"].ToString();
            Debug.Log($"Icon URL: {url}");

            if (!string.IsNullOrEmpty(url))
            {
                if (iconCache.ContainsKey(url))
                {
                    Texture2D cachedTexture = iconCache[url];
                    profileIconImage.sprite = Sprite.Create(cachedTexture, new Rect(0, 0, cachedTexture.width, cachedTexture.height), Vector2.zero);
                    profileIconImage.sprite.name = url;
                    Debug.Log("Profile icon updated from cache.");
                }
                else
                {
                    Texture2D texture = await DownloadImageFromS3(url);
                    if (texture != null)
                    {
                        iconCache[url] = texture;
                        profileIconImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                        profileIconImage.sprite.name = url;
                        Debug.Log("Profile icon updated after download.");
                    }
                    else
                    {
                        Debug.LogWarning("Failed to download texture.");
                    }
                }
            }
        }
        else
        {
            profileIconImage.sprite = null; // Optionally, set a default icon here
            Debug.LogWarning("Icon property not found.");
        }
    }

    public void OpenMatchmakingPanel()
    {
        menuPanel.SetActive(false);
        matchmakingPanel.SetActive(true);

        // Update the username and profile icon in the matchmaking panel
        FindObjectOfType<MatchmakingManager>()?.UpdateUsernameText();
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
            Debug.Log($"Selected file path: {path}");

            Texture2D texture = LoadTexture(path);
            if (texture == null)
            {
                Debug.LogError("Failed to load texture from selected file.");
                // Show error message to user (implement UI feedback as needed)
                return;
            }

            Sprite newIcon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            selectedIcon = newIcon;
            selectedIconDisplay.sprite = newIcon;
        }
    }

    public async void OnSaveProfileClicked()
    {
        string iconUrl = null;

        // Update icon
        if (selectedIcon != null)
        {
            string filePath = await SaveSpriteToLocal(selectedIcon);
            iconUrl = await UploadIconToS3(filePath);
            UpdateIconInPhotonAndPlayFab(iconUrl);
        }

        // Update username if changed
        if (!string.IsNullOrEmpty(usernameInput.text))
        {
            PhotonNetwork.NickName = usernameInput.text;

            // Update in PlayFab
            var updateRequest = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { "Username", usernameInput.text }
                }
            };

            PlayFabClientAPI.UpdateUserData(updateRequest, result =>
            {
                Debug.Log("Username updated in PlayFab.");
            }, error =>
            {
                Debug.LogError($"Failed to update username in PlayFab: {error.ErrorMessage}");
            });

            // Update in Photon
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "Username", usernameInput.text }
            });

            // Update the matchmaking UI
            FindObjectOfType<MatchmakingManager>()?.UpdateUsernameText();
        }

        // Ensure UI reflects updated data
        UpdateMenuUI();

        profilePanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    private Texture2D LoadTexture(string filePath)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                return texture;
            }
            else
            {
                Debug.LogError("Failed to load image data into texture.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading texture from file: {ex.Message}");
            return null;
        }
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
            if (s3Client == null)
            {
                Debug.LogError("s3Client is null. AWS credentials may not be initialized.");
                throw new Exception("AWS S3 client is not initialized.");
            }

            // Read the file into a byte array
            byte[] fileData = File.ReadAllBytes(filePath);

            // Optionally, check file size (e.g., limit to 5 MB)
            if (fileData.Length > 5 * 1024 * 1024) // 5 MB limit
            {
                Debug.LogError("File is too large to upload. Please select an image less than 5 MB.");
                // Show error message to user (implement UI feedback as needed)
                return null;
            }

            // Create a memory stream from the byte array
            using (var ms = new MemoryStream(fileData))
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = ms,
                    ContentType = "image/png"
                };

                var response = await s3Client.PutObjectAsync(putRequest);
                Debug.Log("Icon uploaded to S3 successfully.");
                return $"https://{bucketName}.s3.{s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{key}";
            }
        }
        catch (AmazonS3Exception ex)
        {
            Debug.LogError($"AmazonS3Exception: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception during icon upload: {ex.Message}");
            return null;
        }
    }

    private void UpdateIconInPhotonAndPlayFab(string iconUrl)
    {
        if (!string.IsNullOrEmpty(iconUrl))
        {
            // Update in Photon
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "Icon", iconUrl }
            });
            Debug.Log("Icon updated in Photon.");

            // Manually trigger the property update (if necessary)
            OnPlayerPropertiesUpdate(PhotonNetwork.LocalPlayer, new ExitGames.Client.Photon.Hashtable { { "Icon", iconUrl } });

            // Update in PlayFab
            var updateRequest = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    { "ProfilePictureUrl", iconUrl }
                }
            };

            PlayFabClientAPI.UpdateUserData(updateRequest, result =>
            {
                Debug.Log("Profile picture URL updated in PlayFab.");
            }, error =>
            {
                Debug.LogError($"Failed to update profile picture URL in PlayFab: {error.ErrorMessage}");
            });
        }
    }

    public async Task<Texture2D> DownloadImageFromS3(string url)
    {
        try
        {
            // Extract bucket and key from the URL
            Uri uri = new Uri(url);
            string key = uri.LocalPath.TrimStart('/');

            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
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
