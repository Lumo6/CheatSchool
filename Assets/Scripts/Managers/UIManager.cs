using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject interactUI;
    [SerializeField] private Canvas mainCanvas;

    [Header("Suspicion UI")]
    [SerializeField] private Image suspicionBar;

    [Header("Copy UI")]
    [SerializeField] private Image copyBar;

    [Header("Copy UI")]
    [SerializeField] private Image currentCopyBar;

    [Header("Copy UI")]
    [SerializeField] private TMPro.TMP_Text resultMessage;

    [Header("Groups UI")]
    public GameObject beforeGameUI;
    public GameObject endGameUI;
    public GameObject inGameUI;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowInteractUI(bool state)
    {
        interactUI.SetActive(state);
    }

    public void updateCopyProgressUI(float nb)
    {
        copyBar.fillAmount = nb;
    }

    public void updateSuspicionProgressUI(float nb)
    {
        suspicionBar.fillAmount = nb;
    }

    public void updateCurrentCopyProgressUI(float nb)
    {
        currentCopyBar.fillAmount = nb;
    }

    public void ShowMenuScreen()
    {
        endGameUI.SetActive(true);
    }

    public void HideMenuScreen()
    {
        endGameUI.SetActive(false);
    }

    public void ShowEndScreen(string message)
    {
        endGameUI.SetActive(true);
        resultMessage.gameObject.SetActive(true);
        resultMessage.text = message;
    }

    public void ShowBeforeScreen()
    {
        beforeGameUI.SetActive(true);
    }

    public void HideBeforeScreen()
    {
        beforeGameUI.SetActive(false);
    }

    public void ShowInGameScreen()
    {
        inGameUI.SetActive(true);
    }

    public void HideInGameScreen()
    {
        inGameUI.SetActive(false);
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
