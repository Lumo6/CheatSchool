using UnityEngine;

public class ShowHighScore : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text Score;
    [SerializeField] private GlobalVariables globals;

    private void OnEnable()
    {
        string scores;
        foreach (var pair in globals.scoreHisto)
        {
            scores = pair.scores + "\n";

        }
    }
}
