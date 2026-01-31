using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Gère l'état global du jeu, la progression, la gestion des scores et la logique principale.
/// Utilise le pattern Singleton.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Instance unique du GameManager

    // Enumération des différents états possibles du jeu
    public enum GameState
    {
        Playing,            // Le joueur joue normalement
        Copying,            // Le joueur est en train de copier
        Spotted_GameOver,   // Le joueur a été repéré (fin de partie)
        Win                 // Le joueur a gagné
    }

    [Header("Enums")]
    public GameState CurrentState { get; private set; } = GameState.Playing; // État courant du jeu

    [Header("Copy Settings")]
    public float copyDuration = 5f; // Durée nécessaire pour copier un bureau

    [Header("Chrono")]
    public float gameTime = 0f;         // Temps écoulé depuis le début de la partie
    public float gameTimeLimit = 300f;  // Limite de temps pour la partie

    [SerializeField] private float CopyProgress = 0f; // Progression globale de la copie (0 à 1)

    public int nbCopyNeeded = 5; // Nombre de copies nécessaires pour gagner
    public CopyTargetDesk currentdesk; // Bureau actuellement ciblé pour la copie

    [SerializeField] private AudioClip mainMusicClip;    // Musique principale
    [SerializeField] private AudioClip victorySoundClip;  // Son de victoire
    [SerializeField] private AudioClip loseSoundClip;     // Son de défaite

    private AudioSource mainMusicAudioSource; // Source audio pour la musique principale

    [Header("All Difficulty Settings")]
    [SerializeField] private List<DifficultySettings> allDifficultySettings; // Liste de toutes les difficultés

    [Header("Current Difficulty Settings")]
    public DifficultySettings currentDifficultySettings; // Difficulté courante

    [Header("Global Variables")]
    public GlobalVariables globals; // Référence aux variables globales

    /// <summary>
    /// Initialisation du Singleton et des paramètres de difficulté.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Sélectionne la difficulté selon le nom choisi dans les variables globales
        currentDifficultySettings = GetDifficultyByName(globals.difficultyname) ? GetDifficultyByName(globals.difficultyname) : allDifficultySettings[0];
        copyDuration = currentDifficultySettings.copyDuration;
        gameTimeLimit = currentDifficultySettings.gameTimeLimit;
        nbCopyNeeded = currentDifficultySettings.nbCopyNeeded;
    }

    /// <summary>
    /// Démarre la partie (affiche l'écran d'attente puis lance le jeu).
    /// </summary>
    void Start()
    {
        gameTime = 0f;
        Time.timeScale = 0f; // Met le jeu en pause au début
        StartCoroutine(Begin());
    }

    /// <summary>
    /// Coroutine d'introduction avant le début du jeu.
    /// </summary>
    private IEnumerator Begin()
    {
        UIManager.Instance.beforeGameUI.SetActive(true); // Affiche l'écran d'attente
        mainMusicAudioSource = SoundFXManager.Instance.PlaySound(mainMusicClip, this.transform, true); // Lance la musique

        yield return new WaitForSecondsRealtime(10f); // Attend 10 secondes (temps réel)

        UIManager.Instance.beforeGameUI.SetActive(false); // Cache l'écran d'attente
        Time.timeScale = 1f; // Démarre le jeu
        UIManager.Instance.ShowInGameScreen(); // Affiche l'UI de jeu
    }

    /// <summary>
    /// Récupère les paramètres de difficulté selon le nom.
    /// </summary>
    private DifficultySettings GetDifficultyByName(string name)
    {
        foreach (var diff in allDifficultySettings)
        {
            Debug.Log("Checking difficulty: " + diff.difficultyname);
            if (diff.difficultyname == name)
                return diff;
        }
        Debug.LogWarning("DifficultySettings not found for name: " + name);
        return null;
    }

    /// <summary>
    /// Mise à jour principale du jeu (chrono, état, etc).
    /// </summary>
    void Update()
    {
        // Si la partie est terminée, on ne fait rien
        if (CurrentState == GameState.Spotted_GameOver ||
            CurrentState == GameState.Win)
            return;

        // Si le temps est écoulé, le joueur est repéré
        if (gameTime >= gameTimeLimit)
        {
            PlayerSpotted();
            return;
        }

        gameTime += Time.deltaTime; // Incrémente le temps
        UIManager.Instance.UpdateTime(Mathf.FloorToInt(gameTime).ToString()); // Met à jour l'affichage du temps
    }

    /// <summary>
    /// Lance la phase de copie si possible.
    /// </summary>
    public void StartCopying()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Copying;
        Debug.Log("Copy started");
    }

    /// <summary>
    /// Arrête la phase de copie.
    /// </summary>
    public void StopCopying()
    {
        if (CurrentState != GameState.Copying) return;

        CurrentState = GameState.Playing;
        Debug.Log("Copy interrupted");
    }

    /// <summary>
    /// Appelée quand une copie est terminée.
    /// </summary>
    public void CopyCompleted()
    {
        CopyProgress = Mathf.Clamp01(CopyProgress + (1.0f / nbCopyNeeded)); // Incrémente la progression
        CurrentState = GameState.Playing;
        UIManager.Instance.updateCopyProgressUI(CopyProgress); // Met à jour la barre de progression globale
        UIManager.Instance.updateCurrentCopyProgressUI(0f);    // Réinitialise la barre de progression courante

        Debug.Log("Copy completed");
    }

    /// <summary>
    /// Appelée quand le joueur est repéré (fin de partie).
    /// </summary>
    public void PlayerSpotted()
    {
        if (CurrentState == GameState.Spotted_GameOver)
            return;

        CurrentState = GameState.Spotted_GameOver;
        mainMusicAudioSource.Stop();
        Time.timeScale = 0f;
        UIManager.Instance.ShowEndScreen("Game Over: Player Spotted");
        SoundFXManager.Instance.PlaySound(loseSoundClip, this.transform);
        Debug.Log("Game Over: Player spotted");
    }

    /// <summary>
    /// Vérifie si le joueur a gagné (toutes les copies faites).
    /// </summary>
    public void CheckWin()
    {
        if (CopyProgress == 1.0f)
        {
            CurrentState = GameState.Win;

            int finalScore = Mathf.FloorToInt(gameTime);
            SaveScoreToFile(finalScore, currentDifficultySettings.difficultyname);

            mainMusicAudioSource.Stop();
            Time.timeScale = 0f;

            UIManager.Instance.ShowEndScreen("You Win!");
            SoundFXManager.Instance.PlaySound(victorySoundClip, this.transform);
            Debug.Log("You win!");
        }
    }

    /// <summary>
    /// Sauvegarde le score dans un fichier texte.
    /// </summary>
    private void SaveScoreToFile(int score, string difficulty)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "scores.txt");
        Debug.Log("Saving score to file: " + filePath);
        string line = $"{score};{difficulty}";

        File.AppendAllText(filePath, line + "\n");
    }

    /// <summary>
    /// Indique si le joueur est en train de copier.
    /// </summary>
    public bool IsCopying() => CurrentState == GameState.Copying;

    /// <summary>
    /// Met le jeu en pause et affiche le menu.
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
        UIManager.Instance.ShowMenuScreen();
    }

    /// <summary>
    /// Reprend le jeu après une pause.
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        UIManager.Instance.HideMenuScreen();
    }
}
