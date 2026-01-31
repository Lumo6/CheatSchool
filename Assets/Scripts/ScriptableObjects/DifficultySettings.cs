using UnityEngine;

/// <summary>
/// ScriptableObject pour définir les paramètres de difficulté du jeu.
/// Permet de créer facilement plusieurs niveaux de difficulté (Facile, Normal, Difficile, etc.)
/// via l’éditeur Unity et de les réutiliser dans le GameManager.
/// </summary>
[CreateAssetMenu(fileName = "DifficultySettings", menuName = "Scriptable Objects/DifficultySettings")]
public class DifficultySettings : ScriptableObject
{
    [Header("Difficulty Name")]
    // Nom de la difficulté (ex: Easy, Medium, Hard)
    public string difficultyname;

    [Header("Copy Settings")]
    // Durée nécessaire pour copier un bureau (en secondes)
    public float copyDuration = 5f;

    [Header("Chrono")]
    // Limite de temps pour la partie (en secondes)
    public float gameTimeLimit = 300f;

    [Header("Copies Needed")]
    // Nombre de copies nécessaires pour gagner la partie
    public int nbCopyNeeded = 5;

    [Header("AI Vision")]
    // Distance maximale de détection par l'IA (professeur)
    public float viewDistance = 10f;
    // Angle de vision de l'IA (en degrés)
    public float viewAngle = 120f;

    [Header("Suspicion")]
    // Vitesse à laquelle la suspicion augmente lorsqu’un joueur est repéré
    public float suspicionIncreaseRate = 20f;
    // Vitesse à laquelle la suspicion diminue lorsque le joueur n'est pas observé
    public float suspicionDecreaseRate = 10f;
    // Durée pendant laquelle le professeur s'arrête lorsqu'il a détecté quelque chose
    public float stopDuration = 0.5f;

    [Header("Room Size")]
    // Nombre de rangées et colonnes de bureaux étudiants dans la salle
    public int rows = 5;
    public int columns = 6;

    [Header("Room Dimensions")]
    // Dimensions physiques de la salle en unités de segments
    public int length = 3;
    public int width = 3;

    [Header("Obstacles")]
    // Nombre d'obstacles aléatoires à placer dans la salle
    public int obstacleCount = 5;
}
