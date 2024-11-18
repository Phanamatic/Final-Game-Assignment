using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SFB; // For file browser
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using System.Threading.Tasks;
using System;
using dotenv.net;
using Amazon.Runtime;

public class LoginManager : MonoBehaviourPunCallbacks
{
    [Header("Sign-In Menu")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;
    public GameObject errorPanel;
    public GameObject AccountPanel;
    public GameObject menuPanel;

    [Header("Create Account Menu")]
    public TMP_InputField createUsernameInput;
    public TMP_InputField createPasswordInput;
    public GameObject createErrorPanel;
    public TextMeshProUGUI createErrorText;
    public GameObject profileIconsPanel; // Parent of preset icon buttons
    public Image selectedIconDisplay;   // Image for showing selected icon
    public TextMeshProUGUI fileNameText;
    public TextMeshProUGUI usernamePreviewText;
    private Sprite selectedIcon;

    // AWS S3 Configuration
    private const string bucketName = "multiplayerisyou";
    private AmazonS3Client s3Client;

    private void Awake()
    {
        // Load environment variables
        DotEnv.Load();

        // Fetch AWS credentials and region from environment variables
        string accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
        string secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
        string region = Environment.GetEnvironmentVariable("AWS_REGION");

        if (string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(region))
        {
            Debug.LogError("AWS credentials or region are not set in the environment variables.");
            return;
        }

        // Initialize AmazonS3Client with credentials
        var credentials = new BasicAWSCredentials(accessKey, secretKey);
        var regionEndpoint = RegionEndpoint.GetBySystemName(region);

        s3Client = new AmazonS3Client(credentials, regionEndpoint);
    }

    private void Start()
    {
        createUsernameInput.onValueChanged.AddListener(UpdateUsernamePreview);
    }

    public void Login()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            errorText.text = "Please enter both username and password.";
            errorPanel.SetActive(true);
            return;
        }

        var loginRequest = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };

        PlayFabClientAPI.LoginWithPlayFab(loginRequest, result =>
        {
            Debug.Log("Login Successful");
            PhotonNetwork.NickName = username;

            // Proceed only after confirming the login is successful
            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                LoadUserDataAndSyncToPhoton(() =>
                {
                    errorPanel.SetActive(false);
                    AccountPanel.SetActive(false);
                    menuPanel.SetActive(true);
                    Debug.Log("Menu panel activated successfully.");
                });
            }
            else
            {
                Debug.LogError("Login successful, but PlayFabClientAPI reports not logged in.");
            }
        }, error =>
        {
            Debug.LogError($"Login failed: {error.ErrorMessage}");
            errorText.text = error.ErrorMessage;
            errorPanel.SetActive(true);
        });
    }

    public void CreateAccount()
    {
        string username = createUsernameInput.text;
        string password = createPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || selectedIcon == null)
        {
            createErrorText.text = "Please fill in all fields and select an icon.";
            createErrorPanel.SetActive(true);
            return;
        }

        var registerRequest = new RegisterPlayFabUserRequest
        {
            Username = username,
            Password = password,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, async result =>
        {
            Debug.Log("Account Created Successfully");

            string filePath = await SaveSpriteToLocal(selectedIcon);
            string s3Url = await UploadImageToS3(filePath, $"{username}/profile_picture.png");

            SaveUserData(username, username, s3Url); // Save both Username and NewUsername
            PhotonNetwork.NickName = username;

            Hashtable photonProperties = new Hashtable
            {
                { "Username", username },
                { "Icon", s3Url }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(photonProperties);

            FindObjectOfType<MainMenuManager>()?.UpdateMenuUI();

            AccountPanel.SetActive(false);
            menuPanel.SetActive(true);
            Debug.Log("Menu panel activated successfully after account creation.");
        }, error =>
        {
            Debug.LogError($"Account creation failed: {error.ErrorMessage}");
            createErrorText.text = error.ErrorMessage;
            createErrorPanel.SetActive(true);
        });
    }

    public void SelectPreSetIcon(Button iconButton)
    {
        selectedIcon = iconButton.GetComponent<Image>().sprite;
        selectedIconDisplay.sprite = selectedIcon;
    }

    public void UploadIcon()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Profile Icon", "", "png", false);
        if (paths.Length > 0)
        {
            string path = paths[0];

            // Display the file name in the UI
            fileNameText.text = Path.GetFileName(path);

            Texture2D texture = LoadTexture(path);
            Sprite newIcon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            selectedIcon = newIcon;
            selectedIconDisplay.sprite = newIcon;
        }
    }

    private async Task<string> UploadImageToS3(string filePath, string key)
    {
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
            Debug.Log("Image uploaded successfully to S3.");
            return $"https://{bucketName}.s3.{s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{key}";
        }
        catch (AmazonS3Exception ex)
        {
            Debug.LogError($"Error uploading image to S3: {ex.Message}");
            return null;
        }
    }

    private async Task<string> SaveSpriteToLocal(Sprite sprite)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "temp_profile_picture.png");

        Texture2D texture = MakeTextureReadable(sprite.texture);
        byte[] imageData = texture.EncodeToPNG();
        await File.WriteAllBytesAsync(filePath, imageData);

        return filePath;
    }

    private void SaveUserData(string username, string newUsername, string s3Url)
    {
        var updateRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "Username", username },
                { "NewUsername", newUsername },
                { "ProfilePictureUrl", s3Url }
            }
        };

        PlayFabClientAPI.UpdateUserData(updateRequest, result =>
        {
            Debug.Log("User data saved successfully!");

            Hashtable photonProperties = new Hashtable
            {
                { "Username", newUsername },
                { "Icon", s3Url }
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(photonProperties);
        }, error =>
        {
            Debug.LogError($"Failed to save user data: {error.ErrorMessage}");
        });
    }

    private void LoadUserDataAndSyncToPhoton(System.Action onComplete = null)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), async result =>
        {
            if (result.Data != null)
            {
                Hashtable photonProperties = new Hashtable();

                if (result.Data.ContainsKey("Username"))
                {
                    string username = result.Data["Username"].Value;
                    PhotonNetwork.NickName = username;
                    photonProperties["Username"] = username;
                }

                if (result.Data.ContainsKey("ProfilePictureUrl"))
                {
                    string url = result.Data["ProfilePictureUrl"].Value;
                    photonProperties["Icon"] = url;

                    Texture2D texture = await DownloadImageFromS3(url);
                    if (texture != null)
                    {
                        Sprite profileSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                        selectedIconDisplay.sprite = profileSprite;
                    }
                }

                PhotonNetwork.LocalPlayer.SetCustomProperties(photonProperties);
                Debug.Log("User data synced to Photon.");

                FindObjectOfType<MainMenuManager>()?.UpdateMenuUI();
            }

            onComplete?.Invoke();
        }, error =>
        {
            Debug.LogError($"Failed to load user data: {error.ErrorMessage}");
            onComplete?.Invoke();
        });
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

    private Texture2D LoadTexture(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    private Texture2D MakeTextureReadable(Texture2D originalTexture)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
            originalTexture.width, originalTexture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

        Graphics.Blit(originalTexture, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;

        Texture2D readableTexture = new Texture2D(originalTexture.width, originalTexture.height);
        readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        return readableTexture;
    }

    private void UpdateUsernamePreview(string newUsername)
    {
        usernamePreviewText.text = $"{newUsername}";
    }
}
