using System.Collections;
using UnityEngine;

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
    public float GameTime => gameTime;

    private float gameTime = 0f;

    [SerializeField] private float CopyProgress = 0f;
    
    public int nbCopyNeeded = 5;
    public CopyTargetDesk currentdesk;

    public AudioClip mainMusicClip;
    public AudioClip loopMusicClip;
    public AudioClip victorySoundClip;
    public AudioClip loseSoundClip;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        ShowRules();
        UIManager.Instance.ShowInGameScreen();
        SoundFXManager.Instance.PlaySound(mainMusicClip, this.transform);
        StartCoroutine(PlayNextMusicAfter(mainMusicClip.length));
    }

    private void ShowRules()
    {
        Time.timeScale = 0f;
        StartCoroutine(CoroutineRules());
        Time.timeScale = 1f;
    }

    private IEnumerator CoroutineRules()
    {
        UIManager.Instance.beforeGameUI.SetActive(true);
        yield return new WaitForSeconds(10f);
        UIManager.Instance.beforeGameUI.SetActive(false);
    }

    private IEnumerator PlayNextMusicAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Play the looping music clip
        SoundFXManager.Instance.PlaySound(loopMusicClip, this.transform, true);
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
        UIManager.Instance.updateCurrentCopyProgressUI(0f);
        
        Debug.Log("Copy completed");
    }

    public void PlayerSpotted()
    {
        if (CurrentState == GameState.Spotted_GameOver)
            return;

        CurrentState = GameState.Spotted_GameOver;
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
            Time.timeScale = 0f;
            UIManager.Instance.ShowEndScreen("You Win!");
            SoundFXManager.Instance.PlaySound(victorySoundClip, this.transform);
            Debug.Log("You win!");
        }
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
