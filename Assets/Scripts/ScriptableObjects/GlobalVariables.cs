using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScoreHistory
{
    public int scores;
    public string difficultyname;

    public ScoreHistory(int scores, string difficultyname)
    {
        this.scores = scores;
        this.difficultyname = difficultyname;
    }
}

[CreateAssetMenu(fileName = "GlobalVariables", menuName = "Scriptable Objects/GlobalVariables")]
public class GlobalVariables : ScriptableObject
{
    public string difficultyname;
    public List<ScoreHistory> scoreHisto;
    private void OnEnable()
    {
        if (scoreHisto == null)
            scoreHisto = new List<ScoreHistory>();
    }
}

