using UnityEngine;

[CreateAssetMenu(fileName = "DifficultySettings", menuName = "Scriptable Objects/DifficultySettings")]
public class DifficultySettings : ScriptableObject
{
    [Header("Difficulty Name")]
    public string difficultyname;

    [Header("Copy Settings")]
    public float copyDuration = 5f;

    [Header("Chrono")]
    public float gameTimeLimit = 300f;

    [Header("Copies Needed")]
    public int nbCopyNeeded = 5;

    [Header("AI Vision")]
    public float viewDistance = 10f;
    public float viewAngle = 120f;

    [Header("Suspicion")]
    public float suspicionIncreaseRate = 20f;
    public float suspicionDecreaseRate = 10f;
    public float stopDuration = 0.5f;

    [Header("Room Size")]
    public int rows = 5;
    public int columns = 6;

    [Header("Room Dimensions")]
    public int length = 3;
    public int width = 3;
}
