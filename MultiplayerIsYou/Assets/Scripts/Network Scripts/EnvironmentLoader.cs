using dotenv.net;
using UnityEngine;
using System;

public class EnvironmentLoader : MonoBehaviour
{
    void Start()
    {
        // Load the .env file
        DotEnv.Load();

        // Access environment variables
        string accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
        string secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");
        string region = Environment.GetEnvironmentVariable("AWS_REGION");

        // Debug to confirm the values are loaded (Remove this in production for security)
        Debug.Log($"AWS Access Key: {accessKey}");
        Debug.Log($"AWS Secret Key: {secretKey}");
        Debug.Log($"AWS Region: {region}");
    }
}
