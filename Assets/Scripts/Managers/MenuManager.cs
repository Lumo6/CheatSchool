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
    [Tooltip("Name of the scene to load for gameplay")]
    [SerializeField] private string PlayScene;
    [Tooltip("Menu background music clip")]
    [SerializeField] private AudioClip menuMusicClip;


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

    public void Start()
    {
        SoundFXManager.Instance.PlaySound(menuMusicClip, this.transform, true);
    }
}
