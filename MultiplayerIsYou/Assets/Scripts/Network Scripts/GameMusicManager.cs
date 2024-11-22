using UnityEngine;

public class GameMusicManager : MonoBehaviour
{
    public static GameMusicManager Instance;

    [Header("Music Tracks")]
    public AudioClip[] musicTracks; 

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

        audioSource.loop = false;
        audioSource.playOnAwake = false;

        PlayRandomTrack();
    }

    void PlayRandomTrack()
    {
        if (musicTracks.Length == 0)
        {
            Debug.LogError("No music tracks assigned to GameMusicManager.");
            return;
        }

        AudioClip clip = musicTracks[Random.Range(0, musicTracks.Length)];
        audioSource.clip = clip;
        audioSource.Play();
        Invoke(nameof(PlayRandomTrack), clip.length);
    }
}
