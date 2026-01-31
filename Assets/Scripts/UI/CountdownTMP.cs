using UnityEngine;
using TMPro;
using LitMotion;

/// <summary>
/// Gère un compte à rebours affiché avec TextMeshPro.
/// </summary>
public class CountdownTMP : MonoBehaviour
{
    // Référence au composant TMP_Text qui affichera le compte à rebours
    [SerializeField] private TMP_Text countdownText;
    // Temps de départ du compte à rebours (en secondes)
    [SerializeField] private int startTime = 3;

    // Appelé automatiquement quand l'objet devient actif
    private void OnEnable()
    {
        // Démarre le compte à rebours à partir de startTime
        StartCountdown(startTime);
    }

    /// <summary>
    /// Lance le compte à rebours de 'from' à 0.
    /// </summary>
    /// <param name="from">Valeur de départ du compte à rebours</param>
    private void StartCountdown(int from)
    {
        // Crée une animation de 'from' à 0 sur 'from' secondes
        LMotion.Create(from, 0, from)
            // Utilise une interpolation linéaire (vitesse constante)
            .WithEase(Ease.Linear)
            // Utilise le scheduler temps réel (pas lié au Time.timeScale)
            .WithScheduler(MotionScheduler.UpdateRealtime)
            // À chaque mise à jour, met à jour le texte affiché
            .Bind(value =>
            {
                // Arrondit la valeur à l'entier supérieur
                int v = Mathf.CeilToInt(value);
                // Affiche le nombre restant ou "GO!" quand le temps est écoulé
                countdownText.text = v > 0 ? v.ToString() : "GO!";
            });
    }
}
