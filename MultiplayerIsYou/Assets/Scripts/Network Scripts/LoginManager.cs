using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SFB; 
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
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
using System.Net; 

// Add this alias to resolve ambiguity
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

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
    public GameObject profileIconsPanel; 
    public Image selectedIconDisplay; 
    public TextMeshProUGUI fileNameText;
    public TextMeshProUGUI usernamePreviewText;
    private Sprite selectedIcon;

    [Header("Buttons")]
    public Button loginButton;
    public Button signupButton;

    // Removed Force Logout Panel fields  (Allowed for multi device loggins)

    // AWS S3 Configuration
    private const string bucketName = "multiplayerisyou";
    private AmazonS3Client s3Client;

    private bool needToSetPhotonProperties = false;
    private string pendingUsername;
    private string pendingPassword;
    private string pendingIconUrl;

    private void Awake()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        // Load environment variables
        string envFilePath = "";

#if UNITY_EDITOR
        // In the Unity Editor, the project root 
        envFilePath = Path.Combine(Application.dataPath, "..", ".env");
#else
        // In builds, include the .env file in StreamingAssets
        envFilePath = Path.Combine(Application.streamingAssetsPath, ".env");
#endif

        // Load environment variables from the specified path
        DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { envFilePath }));

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

        // Removed listeners for force logout buttons
    }

    public void Login()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (!IsValidUsername(username))
        {
            errorText.text = "Username must be 3-20 characters, letters and numbers only.";
            errorText.color = Color.red;
            errorPanel.SetActive(true);
            return;
        }

        if (!IsValidPassword(password))
        {
            errorText.text = "Password must be at least 6 characters.";
            errorText.color = Color.red;
            errorPanel.SetActive(true);
            return;
        }

        pendingUsername = username;
        pendingPassword = password;

        loginButton.interactable = false;
        errorText.color = Color.white;
        StartCoroutine(AnimateLoadingText(errorText, "Logging in"));

        AttemptLogin();
    }

    private void AttemptLogin()
    {
        var loginRequest = new LoginWithPlayFabRequest
        {
            Username = pendingUsername,
            Password = pendingPassword,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserData = true
            }
        };

        PlayFabClientAPI.LoginWithPlayFab(loginRequest, result =>
        {
            PhotonNetwork.NickName = pendingUsername;

            LoadUserDataAndSyncToPhoton(() =>
            {
                errorPanel.SetActive(false);
                AccountPanel.SetActive(false);
                menuPanel.SetActive(true);
                loginButton.interactable = true;
                StopAllCoroutines();
                Debug.Log("Menu panel activated successfully.");
            });
        }, error =>
        {
            Debug.LogError($"Login failed: {error.ErrorMessage}");
            errorText.color = Color.red;
            errorText.text = error.ErrorMessage;
            errorPanel.SetActive(true);
            loginButton.interactable = true;
            StopAllCoroutines();
        });
    }

    // Removed OnForceLogoutButtonClicked and OnCancelForceLogoutButtonClicked methods

    public void CreateAccount()
    {
        string username = createUsernameInput.text;
        string password = createPasswordInput.text;

        if (!IsValidUsername(username))
        {
            createErrorText.text = "Username must be 3-20 characters, letters and numbers only.";
            createErrorText.color = Color.red;
            createErrorPanel.SetActive(true);
            return;
        }

        if (!IsValidPassword(password))
        {
            createErrorText.text = "Password must be at least 6 characters.";
            createErrorText.color = Color.red;
            createErrorPanel.SetActive(true);
            return;
        }

        if (selectedIcon == null)
        {
            createErrorText.text = "Please select an icon.";
            createErrorText.color = Color.red;
            createErrorPanel.SetActive(true);
            return;
        }

        signupButton.interactable = false;
        createErrorText.color = Color.white;
        StartCoroutine(AnimateLoadingText(createErrorText, "Creating account"));

        var registerRequest = new RegisterPlayFabUserRequest
        {
            Username = username,
            Password = password,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, async result =>
        {
            try
            {
                StopAllCoroutines();
                createErrorText.text = "Account created successfully! Signing you in...";
                createErrorText.color = Color.white;
                StartCoroutine(AnimateLoadingText(createErrorText, "Account created successfully! Signing you in"));

                Debug.Log("Account Created Successfully");

                // Proceed only if the client is logged in
                if (PlayFabClientAPI.IsClientLoggedIn())
                {
                    string filePath = await SaveSpriteToLocal(selectedIcon);
                    string s3Url = await UploadImageToS3(filePath, $"{username}/profile_picture.png");

                    if (string.IsNullOrEmpty(s3Url))
                    {
                        throw new Exception("Failed to upload image to S3.");
                    }

                    SaveUserData(username, username, s3Url); // Save both Username and NewUsername (Removed this system cause playfab doesnt allow)
                    PhotonNetwork.NickName = username;

                    pendingUsername = username;
                    pendingIconUrl = s3Url;
                    needToSetPhotonProperties = true;

                    if (!PhotonNetwork.IsConnected)
                    {
                        PhotonNetwork.ConnectUsingSettings();
                    }
                    else
                    {
                        SetPhotonProperties();
                    }
                }
                else
                {
                    Debug.LogError("Account created, but PlayFabClientAPI reports not logged in.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception during account creation: {ex.Message}");
                createErrorText.color = Color.red;
                createErrorText.text = "An error occurred during account creation.";
                createErrorPanel.SetActive(true);
                signupButton.interactable = true;
                StopAllCoroutines();
            }
        }, error =>
        {
            Debug.LogError($"Account creation failed: {error.ErrorMessage}");
            createErrorText.color = Color.red;
            createErrorText.text = error.ErrorMessage;
            createErrorPanel.SetActive(true);
            signupButton.interactable = true;
            StopAllCoroutines();
        });
    }

    private bool IsValidUsername(string username)
    {
        // Username must be between 3 and 20 characters, letters and numbers only
        if (username.Length < 3 || username.Length > 20)
            return false;

        foreach (char c in username)
        {
            if (!char.IsLetterOrDigit(c))
                return false;
        }
        return true;
    }

    private bool IsValidPassword(string password)
    {
        // Password must be at least 6 characters
        return password.Length >= 6;
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
            Debug.Log($"Selected file path: {path}");

            fileNameText.text = Path.GetFileName(path);

            Texture2D texture = LoadTexture(path);
            if (texture == null)
            {
                Debug.LogError("Failed to load texture from selected file.");
                // Show error message to user
                createErrorText.text = "Failed to load image. Please select a valid image file.";
                createErrorText.color = Color.red;
                createErrorPanel.SetActive(true);
                return;
            }

            Sprite newIcon = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            selectedIcon = newIcon;
            selectedIconDisplay.sprite = newIcon;
        }
    }

    private async Task<string> UploadImageToS3(string filePath, string key)
    {
        try
        {
            if (s3Client == null)
            {
                Debug.LogError("s3Client is null. AWS credentials may not be initialized.");
                throw new Exception("AWS S3 client is not initialized.");
            }

            // Read the file into a byte array
            byte[] fileData = File.ReadAllBytes(filePath);

            if (fileData.Length > 5 * 1024 * 1024) // 5 MB limit
            {
                Debug.LogError("File is too large to upload. Please select an image less than 5 MB.");
                // Show error message to user
                createErrorText.text = "Image is too large. Please select an image less than 5 MB.";
                createErrorText.color = Color.red;
                createErrorPanel.SetActive(true);
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
                Debug.Log("Image uploaded successfully to S3.");
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
            Debug.LogError($"Exception during image upload: {ex.Message}");
            return null;
        }
    }

    private async Task<string> SaveSpriteToLocal(Sprite sprite)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "temp_profile_picture.png");

        Texture2D texture = MakeTextureReadable(sprite.texture);
        if (texture == null)
        {
            Debug.LogError("Failed to make the sprite texture readable.");
            return null;
        }

        byte[] imageData = texture.EncodeToPNG();
        if (imageData == null || imageData.Length == 0)
        {
            Debug.LogError("Failed to encode texture to PNG.");
            return null;
        }

        try
        {
            await File.WriteAllBytesAsync(filePath, imageData);
            Debug.Log($"Sprite saved to local file: {filePath}");
            return filePath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving sprite to local file: {ex.Message}");
            return null;
        }
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
                // Removed "IsLoggedIn" from user data
            }
        };

        PlayFabClientAPI.UpdateUserData(updateRequest, result =>
        {
            Debug.Log("User data saved successfully!");

            PhotonHashtable photonProperties = new PhotonHashtable
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
                PhotonHashtable photonProperties = new PhotonHashtable();

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

                    // Optionally, you can load the image here
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

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server.");

        if (needToSetPhotonProperties)
        {
            SetPhotonProperties();
            needToSetPhotonProperties = false;
        }
    }

    private void SetPhotonProperties()
    {
        PhotonHashtable photonProperties = new PhotonHashtable
        {
            { "Username", pendingUsername },
            { "Icon", pendingIconUrl }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(photonProperties);

        FindObjectOfType<MainMenuManager>()?.UpdateMenuUI();

        AccountPanel.SetActive(false);
        menuPanel.SetActive(true);
        signupButton.interactable = true;
        StopAllCoroutines();
        Debug.Log("Menu panel activated successfully after account creation.");
    }

    private async Task<Texture2D> DownloadImageFromS3(string url)
    {
        try
        {
            var uri = new Uri(url);
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
                if (texture.LoadImage(stream.ToArray()))
                {
                    return texture;
                }
                else
                {
                    Debug.LogError("Failed to load image data into texture.");
                    return null;
                }
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

    private Texture2D MakeTextureReadable(Texture2D originalTexture)
    {
        if (originalTexture.isReadable)
        {
            return originalTexture; 
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

    private void UpdateUsernamePreview(string newUsername)
    {
        usernamePreviewText.text = $"{newUsername}";
    }

    private IEnumerator AnimateLoadingText(TextMeshProUGUI textComponent, string baseText)
    {
        string[] dots = { ".", "..", "..." };
        int index = 0;
        while (true)
        {
            textComponent.text = $"{baseText}{dots[index % dots.Length]}";
            index++;
            yield return new WaitForSeconds(0.5f);
        }
    }

    // Removed OnApplicationQuit method
}
