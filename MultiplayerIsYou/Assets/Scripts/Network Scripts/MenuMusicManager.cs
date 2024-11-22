using UnityEngine;

public class MenuMusicManager : MonoBehaviour
{
    public AudioClip[] musicTracks; 
    private AudioSource audioSource;

    void Start()
    {
        if (musicTracks.Length == 0)
        {
            Debug.LogError("No music tracks assigned to MenuMusicManager.");
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
        AudioClip clip = musicTracks[Random.Range(0, musicTracks.Length)];
        audioSource.clip = clip;
        audioSource.Play();
        Invoke(nameof(PlayRandomTrack), clip.length);
    }
}
