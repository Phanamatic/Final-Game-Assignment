using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance;

    [Header("Sound Effects")]
    public AudioClip winSound;
    public AudioClip defeatSound;
    public AudioClip clickSound;  
    public AudioClip hoverSound;   

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
    }

    public void PlayWinSound()
    {
        if (winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }
        else
        {
            Debug.LogError("WinSound is not assigned in UISoundManager.");
        }
    }

    public void PlayDefeatSound()
    {
        if (defeatSound != null)
        {
            audioSource.PlayOneShot(defeatSound);
        }
        else
        {
            Debug.LogError("DefeatSound is not assigned in UISoundManager.");
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        else
        {
            Debug.LogError("ClickSound is not assigned in UISoundManager.");
        }
    }

    public void PlayHoverSound()
    {
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
        else
        {
            Debug.LogError("HoverSound is not assigned in UISoundManager.");
        }
    }
}
