using UnityEngine;
using System.Linq;
using System.IO;
using System.Collections.Generic;

public class ShowHighScore : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text Score;
    [SerializeField] private GlobalVariables globals;

    private string filePath;

    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "scores.txt");
        LoadScoresFromFile();
    }

    private void LoadScoresFromFile()
    {
        globals.scoreHisto = new List<ScoreHistory>();

        if (!File.Exists(filePath))
            return;

        var lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            var data = line.Split(';');
            if (data.Length != 2) continue;

            if (int.TryParse(data[0], out int score))
            {
                globals.scoreHisto.Add(new ScoreHistory(score, data[1]));
            }
        }
    }

    private void OnEnable()
    {
        Score.text = "Temps restant :\n";
        Debug.Log("Affichage des scores pour la difficulté : " + globals.difficultyname);
        Score.text += string.Join("\n",
            globals.scoreHisto
                .Where(s => s.difficultyname == globals.difficultyname)
                .OrderBy(s => s.scores)
                .Take(3)
                .Select(s => $"{s.scores} secondes ({s.difficultyname})")
        );
    }

}
