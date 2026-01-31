using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Gère l'affichage et la mise à jour des différents éléments d'interface utilisateur.
/// Utilise le pattern Singleton.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("General UI")]
    [SerializeField] private GameObject interactUI; // UI d'interaction
    [SerializeField] private Canvas mainCanvas; // Canvas principal
    [SerializeField] private TMPro.TMP_Text TimeUI; // Affichage de l'heure

    [Header("Suspicion UI")]
    [SerializeField] private Image suspicionBar; // Barre de suspicion

    [Header("Copy UI")]
    [SerializeField] private Image copyBar; // Barre de progression de copie
    [SerializeField] private Image currentCopyBar; // Barre de progression de la copie en cours

    [Header("Result Message")]
    [SerializeField] private TMPro.TMP_Text resultMessage; // Message de résultat

    [Header("Groups UI")]
    public GameObject beforeGameUI; // UI avant le jeu
    public GameObject endGameUI; // UI de fin de jeu
    public GameObject inGameUI; // UI pendant le jeu

    /// <summary>
    /// Initialise le Singleton.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Affiche ou masque l'UI d'interaction.
    /// </summary>
    public void ShowInteractUI(bool state)
    {
        interactUI.SetActive(state);
    }

    /// <summary>
    /// Met à jour la progression de la barre de copie.
    /// </summary>
    public void updateCopyProgressUI(float nb)
    {
        copyBar.fillAmount = nb;
    }

    /// <summary>
    /// Met à jour la progression de la barre de suspicion.
    /// </summary>
    public void updateSuspicionProgressUI(float nb)
    {
        suspicionBar.fillAmount = nb;
    }

    /// <summary>
    /// Met à jour la progression de la barre de copie en cours.
    /// </summary>
    public void updateCurrentCopyProgressUI(float nb)
    {
        currentCopyBar.fillAmount = nb;
    }

    /// <summary>
    /// Affiche l'écran de menu (fin de partie).
    /// </summary>
    public void ShowMenuScreen()
    {
        endGameUI.SetActive(true);
    }

    /// <summary>
    /// Masque l'écran de menu (fin de partie).
    /// </summary>
    public void HideMenuScreen()
    {
        endGameUI.SetActive(false);
    }

    /// <summary>
    /// Affiche l'écran de fin avec un message.
    /// </summary>
    public void ShowEndScreen(string message)
    {
        endGameUI.SetActive(true);
        resultMessage.gameObject.SetActive(true);
        resultMessage.text = message;
    }

    /// <summary>
    /// Affiche l'écran avant le début du jeu.
    /// </summary>
    public void ShowBeforeScreen()
    {
        beforeGameUI.SetActive(true);
    }

    /// <summary>
    /// Masque l'écran avant le début du jeu.
    /// </summary>
    public void HideBeforeScreen()
    {
        beforeGameUI.SetActive(false);
    }

    /// <summary>
    /// Affiche l'UI de jeu.
    /// </summary>
    public void ShowInGameScreen()
    {
        inGameUI.SetActive(true);
    }

    /// <summary>
    /// Masque l'UI de jeu.
    /// </summary>
    public void HideInGameScreen()
    {
        inGameUI.SetActive(false);
    }

    /// <summary>
    /// Change de scène.
    /// </summary>
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Met à jour l'affichage de l'heure.
    /// </summary>
    public void UpdateTime(string timeString)
    {
        TimeUI.text = timeString;
    }
}
