using UnityEngine;
using UnityEngine.UI;

public class UIBarsController : MonoBehaviour
{
    [Header("Suspicion UI")]
    public Image suspicionBar;

    [Header("Copy UI")]
    public Image copyBar;

    ProfessorAI prof;
    private void Awake()
    {
        prof = FindAnyObjectByType<ProfessorAI>();
    }

    void Update()
    {
        UpdateSuspicionUI();
        UpdateCopyUI();
    }

    void UpdateSuspicionUI()
    {
        if (GameManager.Instance == null)
            return;

        float suspicionNormalized =
            ProfessorSuspicion() / MaxSuspicion();

        suspicionBar.fillAmount = Mathf.Clamp01(suspicionNormalized);
    }

    void UpdateCopyUI()
    {
        if (GameManager.Instance == null)
            return;

        float copyNormalized =
            CopyTimer() / GameManager.Instance.copyDuration;

        copyBar.fillAmount = Mathf.Clamp01(copyNormalized);
    }

    // ---------- HELPERS ----------
    float ProfessorSuspicion()
    {
        return prof != null ? prof.GetSuspicion() : 0f;
    }

    float MaxSuspicion()
    {
        return prof != null ? prof.GetMaxSuspicion() : 1f;
    }

    float CopyTimer()
    {
        return GameManager.Instance.IsCopying()
            ? GameManager.Instance.GetCopyProgress()
            : 0f;
    }
}
