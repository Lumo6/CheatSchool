using UnityEngine;
using System.Linq;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Affiche les meilleurs scores à l'écran.
/// </summary>
public class ShowHighScore : MonoBehaviour
{
    // Référence au composant texte TMP pour afficher les scores
    [SerializeField] private TMPro.TMP_Text Score;
    // Référence à l'objet contenant les variables globales
    [SerializeField] private GlobalVariables globals;

    // Chemin du fichier où sont stockés les scores
    private string filePath;

    /// <summary>
    /// Charge les scores depuis le fichier et les stocke dans la liste globale.
    /// </summary>
    private void LoadScoresFromFile()
    {
        // Réinitialise la liste des scores
        globals.scoreHisto = new List<ScoreHistory>();

        // Si le fichier n'existe pas, on quitte la méthode
        if (!File.Exists(filePath))
            return;

        // Lit toutes les lignes du fichier
        var lines = File.ReadAllLines(filePath);

        // Parcourt chaque ligne pour extraire les données
        foreach (var line in lines)
        {
            var data = line.Split(';');
            // Vérifie que la ligne contient bien deux éléments
            if (data.Length != 2) continue;

            // Tente de convertir le premier élément en entier (score)
            if (int.TryParse(data[0], out int score))
            {
                // Ajoute le score à la liste globale
                globals.scoreHisto.Add(new ScoreHistory(score, data[1]));
            }
        }
    }

    /// <summary>
    /// Appelé lorsque l'objet devient actif. Charge les scores et les affiche.
    /// </summary>
    private void OnEnable()
    {
        // Définit le chemin du fichier de scores
        filePath = Path.Combine(Application.persistentDataPath, "scores.txt");
        // Charge les scores depuis le fichier
        LoadScoresFromFile();

        // Initialise le texte affiché
        Score.text = "Temps restant :\n";
        // Affiche dans la console la difficulté sélectionnée
        Debug.Log("Affichage des scores pour la difficulté : " + globals.difficultyname);
        // Filtre, trie et affiche les 3 meilleurs scores pour la difficulté courante
        Score.text += string.Join("\n",
            globals.scoreHisto
                .Where(s => s.difficultyname == globals.difficultyname)
                .OrderBy(s => s.scores)
                .Take(3)
                .Select(s => $"{s.scores} secondes ({s.difficultyname})")
        );
    }
}
