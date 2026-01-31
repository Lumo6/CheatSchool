using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [Header("Singleton Instance")]
    [Tooltip("Singleton instance of the SoundFXManager")]
    public static SoundFXManager Instance; // Singleton instance

    [Header("Sound FX Settings")]
    [Tooltip("AudioSource prefab for playing sound effects")]
    [SerializeField] private AudioSource soundFXObject;// AudioSource prefab for sound effects

    private List<AudioSource> audioSources;// List to keep track of active AudioSources

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        // Ensure only one instance of SoundFXManager exists
        if (Instance == null)
        {
            Instance = this;
        }

        audioSources = new List<AudioSource>();
    }

    /// <summary>
    /// Plays a sound effect at the specified transform position.
    /// </summary>
    /// <param name="audioClip"></param>// The audio clip to play
    /// <param name="spawnTransform"></param>// The transform where the sound should be played
    /// <param name="loop"></param>// Whether the sound should loop
    /// <returns></returns>
    public AudioSource PlaySound(AudioClip audioClip, Transform spawnTransform, bool loop = false)
    {
        // Instantiate a new AudioSource at the specified position
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        audioSources.Add(audioSource);

        // Assign the audio clip to the AudioSource
        audioSource.clip = audioClip;

        // Set volume based on player preferences
        audioSource.volume = PlayerPrefs.GetFloat("Volume", 0.5f);

        // Play the audio clip
        audioSource.Play();

        // Handle looping
        if (loop)
        {
            audioSource.loop = true;
            return audioSource;
        }

        // Get the length of the audio clip
        float clipLength = audioSource.clip.length;

        // Destroy the AudioSource game object after the clip has finished playing
        Destroy(audioSource.gameObject, audioClip.length);
        return audioSource;
    }

    /// <summary>
    /// Updates the volume of all currently playing sounds based on player preferences.
    /// </summary>
    public void updatePlayingSoundVolume()
    {
        foreach (AudioSource source in audioSources)
        {
            source.volume = PlayerPrefs.GetFloat("Volume", 0.5f);
        }
    }
}
