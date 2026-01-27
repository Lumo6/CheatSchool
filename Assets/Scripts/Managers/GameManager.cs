using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public enum GameState
    {
        Playing,
        Copying,
        Spotted_GameOver,
        Win
    }

    [Header("Enums")]
    public GameState CurrentState { get; private set; } = GameState.Playing;

    [Header("Copy Settings")]
    public float copyDuration = 5f;

    [Header("Chrono")]
    public float gameTime = 0f;
    public float gameTimeLimit = 300f;

    [SerializeField] private float CopyProgress = 0f;
    
    public int nbCopyNeeded = 5;
    public CopyTargetDesk currentdesk;

    [SerializeField] private AudioClip mainMusicClip;
    [SerializeField] private AudioClip victorySoundClip;
    [SerializeField] private AudioClip loseSoundClip;

    private AudioSource mainMusicAudioSource;

    [Header("All Difficulty Settings")]
    [SerializeField] private List<DifficultySettings> allDifficultySettings;


    [Header("Current Difficulty Settings")]
    public DifficultySettings currentDifficultySettings;

    [Header("Global Variables")]
    public GlobalVariables globals;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentDifficultySettings = GetDifficultyByName(globals.difficultyname) ? GetDifficultyByName(globals.difficultyname) : allDifficultySettings[0];
    }
    void Start()
    {
        if (currentDifficultySettings == null)
        {
            Debug.LogError("DifficultySettings not found! Using default values.");
        }
        else
        {
            // Pull values from ScriptableObject
            copyDuration = currentDifficultySettings.copyDuration;
            gameTimeLimit = currentDifficultySettings.gameTimeLimit;
            nbCopyNeeded = currentDifficultySettings.nbCopyNeeded;
        }

        gameTime = 0f;
        Time.timeScale = 0f;
        StartCoroutine(Begin());
    }



    private IEnumerator Begin()
    {
        UIManager.Instance.beforeGameUI.SetActive(true);
        mainMusicAudioSource = SoundFXManager.Instance.PlaySound(mainMusicClip, this.transform, true);

        yield return new WaitForSecondsRealtime(10f);

        UIManager.Instance.beforeGameUI.SetActive(false);
        Time.timeScale = 1f;
        UIManager.Instance.ShowInGameScreen();
    }
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
    void Update()
    {
        if (CurrentState == GameState.Spotted_GameOver ||
            CurrentState == GameState.Win)
            return;

        if (gameTime >= gameTimeLimit)
        {
            PlayerSpotted();
            return;
        }

        gameTime += Time.deltaTime;
        UIManager.Instance.UpdateTime(Mathf.FloorToInt(gameTime).ToString());
    }

    public void StartCopying()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Copying;
        Debug.Log("Copy started");
    }

    public void StopCopying()
    {
        if (CurrentState != GameState.Copying) return;

        CurrentState = GameState.Playing;
        Debug.Log("Copy interrupted");
    }

    public void CopyCompleted()
    {
        CopyProgress = Mathf.Clamp01(CopyProgress + (1.0f / nbCopyNeeded));
        CurrentState = GameState.Playing;
        UIManager.Instance.updateCurrentCopyProgressUI(0f);
        
        Debug.Log("Copy completed");
    }

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

    public void CheckWin()
    {
        if (CopyProgress == 1.0f)
        {
            CurrentState = GameState.Win;

            int finalScore = Mathf.FloorToInt(gameTime);
            globals.scoreHisto.Add(
                new ScoreHistory(finalScore, currentDifficultySettings.difficultyname)
            );

            SaveScoreToFile(finalScore, currentDifficultySettings.difficultyname);

            mainMusicAudioSource.Stop();
            Time.timeScale = 0f;

            UIManager.Instance.ShowEndScreen("You Win!");
            SoundFXManager.Instance.PlaySound(victorySoundClip, this.transform);
            Debug.Log("You win!");
        }
    }


    private void SaveScoreToFile(int score, string difficulty)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "scores.txt");
        string line = $"{score};{difficulty}";

        File.AppendAllText(filePath, line + "\n");
    }




    public bool IsCopying() => CurrentState == GameState.Copying;

    public void PauseGame()
    {
        Time.timeScale = 0f;
        UIManager.Instance.ShowMenuScreen();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        UIManager.Instance.HideMenuScreen();
    }
}
