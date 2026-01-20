using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    [Header("Singleton Instance")]
    [Tooltip("Singleton instance of the SoundFXManager")]
    public static SoundFXManager Instance;

    [Header("Sound FX Settings")]
    [Tooltip("AudioSource prefab for playing sound effects")]
    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        // Ensure only one instance of SoundFXManager exists
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void PlaySound(AudioClip audioClip, Transform spawnTransform, bool loop = false)
    {
        // Instantiate a new AudioSource at the specified position
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

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
            return;
        }

        // Get the length of the audio clip
        float clipLength = audioSource.clip.length;

        // Destroy the AudioSource game object after the clip has finished playing
        Destroy(audioSource.gameObject, audioClip.length);
    }
}
