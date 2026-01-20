using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(Slider))]

/// <summary>
/// Class MenuManager
/// <para>
/// Manage the main menu and pause menu functionalities such as starting the game
/// </para
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Menu Manager References")]
    [Tooltip("Slider to adjust sound volume")]
    public Slider sliderTransform;
    [Tooltip("Name of the scene to load for gameplay")]
    public string PlayScene;
    [Tooltip("Menu background music clip")]
    public AudioClip menuMusicClip;


    public void PlayGame()
    {
        SceneManager.LoadScene(PlayScene);
    }

    // Back to main menu scene
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // Quit the game
    public void QuitGame()
    {
        StartCoroutine(QuitAfterDelay());
    }

    // Coroutine to quit the application after a short delay
    private IEnumerator QuitAfterDelay()
    {
        Debug.Log("Fermeture du jeu dans 1 seconde...");
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    // Initialize the slider value on start
    public void Start()
    {
        // Initialize slider value to current sound level
        if (sliderTransform != null)
        {
            sliderTransform.value = PlayerPrefs.GetFloat("Volume", 0.5f);
        }
        SoundFXManager.Instance.PlaySound(menuMusicClip, this.transform, true);
    }
}
