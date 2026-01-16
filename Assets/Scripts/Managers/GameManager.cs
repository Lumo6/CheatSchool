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
    public float DeltaTime => deltaTime;

    private float gameTime = 0f;
    private float deltaTime = 0f;
    private float copyTimer = 0f;

    private float copyprogress = 5f;
    public float GetCopyProgress() => copyprogress;
    public float SetCopyProgress(float value)
    {
        copyprogress = value;
        return copyprogress;
    }
    public int nbCopyNeeded;

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

        deltaTime = Time.deltaTime;
        gameTime += deltaTime;

        if (CurrentState == GameState.Copying)
        {
            copyTimer += deltaTime;

            if (copyTimer >= copyDuration)
                CopyCompleted();
        }
    }

    public void StartCopying()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Copying;
        copyTimer = 0f;
        Debug.Log("Copy started");
    }

    public void StopCopying()
    {
        if (CurrentState != GameState.Copying) return;

        CurrentState = GameState.Playing;
        copyTimer = 0f;
        Debug.Log("Copy interrupted");
    }

    public void CopyCompleted()
    {
        CurrentState = GameState.Playing;
        Debug.Log("Copy completed");
    }

    public void PlayerSpotted()
    {
        if (CurrentState == GameState.Spotted_GameOver)
            return;

        CurrentState = GameState.Spotted_GameOver;
        Debug.Log("Game Over: Player spotted");
    }

    public void CheckWin()
    {
        if (copyprogress >= 1.0f)
        {
            CurrentState = GameState.Win;
            Debug.Log("You win!");
        }
    }

    public bool IsCopying() => CurrentState == GameState.Copying;
}
