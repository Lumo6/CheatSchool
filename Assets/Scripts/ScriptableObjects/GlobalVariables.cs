using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Score history data structure
/// </summary>
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

/// <summary>
/// Global variables ScriptableObject
/// </summary>
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

