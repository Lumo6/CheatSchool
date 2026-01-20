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

    [Header("EndGame UI")]
    [SerializeField] private GameObject endGameUI;

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

    public void ShowEndScreen(string message)
    {
        endGameUI.SetActive(true);
        resultMessage.text = message;
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
