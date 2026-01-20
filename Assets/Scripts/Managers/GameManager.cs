using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum Difficulty { Easy, Medium, Hard }

    public enum GameState
    {
        Playing,
        Copying,
        Spotted_GameOver,
        Win
    }

    [Header("Enums")]
    public GameState CurrentState { get; private set; } = GameState.Playing;
    public Difficulty difficulty = Difficulty.Easy;

    [Header("Copy Settings")]
    public float copyDuration = 5f;

    [Header("Chrono")]
    public float GameTime => gameTime;

    private float gameTime = 0f;

    [SerializeField] private float CopyProgress = 0f;
    
    public int nbCopyNeeded = 5;
    public CopyTargetDesk currentdesk;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        if (CurrentState == GameState.Spotted_GameOver ||
            CurrentState == GameState.Win)
            return;

        gameTime += Time.deltaTime;
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
        UIManager.Instance.updateCopyProgressUI(CopyProgress);
        Debug.Log("Copy completed");
    }

    public void PlayerSpotted()
    {
        if (CurrentState == GameState.Spotted_GameOver)
            return;

        CurrentState = GameState.Spotted_GameOver;
        Time.timeScale = 0f;
        UIManager.Instance.ShowEndScreen("Game Over: Player Spotted");
        Debug.Log("Game Over: Player spotted");
    }

    public void CheckWin()
    {
        if (CopyProgress == 1.0f)
        {
            CurrentState = GameState.Win;
            Time.timeScale = 0f;
            UIManager.Instance.ShowEndScreen("You Win!");
            Debug.Log("You win!");
        }
    }

    public bool IsCopying() => CurrentState == GameState.Copying;
}
