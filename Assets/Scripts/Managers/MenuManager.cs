using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Ce composant exige la présence d'un Slider sur le GameObject pour d'éventuelles interactions UI.
[RequireComponent(typeof(Slider))]

/// <summary>
/// Classe MenuManager
/// <para>
/// Gère les fonctionnalités du menu principal et du menu pause, telles que démarrer le jeu, choisir la difficulté, revenir au menu principal et quitter.
/// </para>
/// </summary>
public class MenuManager : MonoBehaviour
{
    [Header("Références du Menu Manager")]
    [Tooltip("Nom de la scène à charger pour le gameplay")]
    [SerializeField] private string PlayScene; // Nom de la scène de jeu à charger lors du démarrage

    [Tooltip("Clip de musique de fond du menu")]
    [SerializeField] private AudioClip menuMusicClip; // Musique de fond à jouer dans le menu

    [SerializeField] private GlobalVariables globals; // Référence au ScriptableObject contenant les variables globales (dont la difficulté)

    /// <summary>
    /// Démarre le jeu avec la difficulté sélectionnée.
    /// </summary>
    /// <param name="difficultyname">Nom de la difficulté choisie</param>
    public void PlayGame(string difficultyname)
    {
        // Définit la difficulté choisie dans les variables globales
        SetDifficulty(difficultyname);
        // Charge la scène de jeu principale
        SceneManager.LoadScene(PlayScene);
    }

    /// <summary>
    /// Définit la difficulté dans les variables globales.
    /// </summary>
    /// <param name="difficultyname">Nom de la difficulté</param>
    public void SetDifficulty(string difficultyname)
    {
        // Affiche la difficulté sélectionnée dans la console pour le debug
        Debug.Log("Difficulté sélectionnée : " + difficultyname);
        // Met à jour la variable globale de difficulté
        globals.difficultyname = difficultyname;
    }

    /// <summary>
    /// Retourne au menu principal.
    /// </summary>
    public void BackToMainMenu()
    {
        // Charge la scène du menu principal
        SceneManager.LoadScene("Menu");
    }

    /// <summary>
    /// Quitte le jeu.
    /// </summary>
    public void QuitGame()
    {
        // Ferme l'application (fonctionne uniquement en build, pas dans l'éditeur)
        Application.Quit();
    }

    /// <summary>
    /// Méthode appelée au démarrage du script.
    /// Joue la musique de fond du menu.
    /// </summary>
    public void Start()
    {
        // Joue le clip audio du menu en boucle via le SoundFXManager
        SoundFXManager.Instance.PlaySound(menuMusicClip, this.transform, true);
    }
}
