using UnityEngine;

public class SuspicionUIBinder : MonoBehaviour
{
    public SuspicionMeterUI suspicionUI;

    void Start()
    {
        ProfessorAI prof = FindAnyObjectByType<ProfessorAI>();
        if (prof != null)
            prof.suspicionUI = suspicionUI;
    }
}
