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

    private bool copied = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        copied = true;
        CurrentState = GameState.Playing;
        Debug.Log("Copy completed");
    }

    public void PlayerSpotted()
    {
        CurrentState = GameState.Spotted_GameOver;
        Debug.Log("Game Over: Player spotted");
    }

    public void CheckWin()
    {
        if (copied)
        {
            CurrentState = GameState.Win;
            Debug.Log("You win!");
        }
    }

    public bool IsCopying() => CurrentState == GameState.Copying;

}
