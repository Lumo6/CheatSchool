using UnityEngine;
using System.Collections;

// Ce composant exige qu'un Collider soit présent sur l'objet
[RequireComponent(typeof(Collider))]
public class CopyTargetDesk : MonoBehaviour
{
    // Référence à la coroutine en cours (pour éviter les copies multiples)
    private Coroutine copyCoroutine;
    // Référence au GameManager (singleton)
    private GameManager gm;
    // Référence au UIManager (singleton)
    private UIManager ui;

    // Appelé lors de l'initialisation de l'objet
    private void Awake()
    {
        gm = GameManager.Instance; // Récupère l'instance du GameManager
        ui = UIManager.Instance;   // Récupère l'instance du UIManager
    }

    // Appelé lors de la réinitialisation dans l'éditeur Unity
    private void Reset()
    {
        // S'assure que le collider est en mode "isTrigger" pour détecter les entrées/sorties
        GetComponent<Collider>().isTrigger = true;
    }

    /// <summary>
    /// Démarre l'interaction de copie si aucune n'est en cours.
    /// </summary>
    public void StartInteraction(PlayerControls player)
    {
        // Si une copie est déjà en cours, on ne fait rien
        if (copyCoroutine != null) return;

        gm.StartCopying(); // Informe le GameManager que la copie commence
        copyCoroutine = StartCoroutine(CopyRoutine(player)); // Lance la coroutine de copie
    }

    /// <summary>
    /// Arrête l'interaction de copie en cours.
    /// </summary>
    public void StopInteraction()
    {
        // Si une copie est en cours, on l'arrête
        if (copyCoroutine != null)
        {
            StopCoroutine(copyCoroutine); // Arrête la coroutine
            copyCoroutine = null;
            UIManager.Instance.updateCurrentCopyProgressUI(0f); // Réinitialise l'UI de progression
            gm.StopCopying(); // Informe le GameManager que la copie s'arrête
        }
    }

    /// <summary>
    /// Coroutine qui gère la progression de la copie.
    /// </summary>
    private IEnumerator CopyRoutine(PlayerControls player)
    {
        float timer = 0f; // Temps écoulé depuis le début de la copie

        // Boucle tant que la durée de copie n'est pas atteinte
        while (timer < gm.copyDuration)
        {
            // Si le joueur bouge, la copie échoue
            if (player.IsMoving())
            {
                Debug.Log("Player moved, copy failed");
                StopInteraction();
                yield break; // Arrête la coroutine
            }
            // Met à jour la barre de progression de la copie
            UIManager.Instance.updateCurrentCopyProgressUI(timer / gm.copyDuration);
            timer += Time.deltaTime; // Incrémente le temps
            yield return null; // Attend la prochaine frame
        }
        ui.ShowInteractUI(false); // Cache l'UI d'interaction
        gm.CopyCompleted();       // Informe le GameManager que la copie est terminée
        copyCoroutine = null;     // Réinitialise la référence à la coroutine
        MakeDeskNotGlow();        // Désactive l'effet lumineux du bureau
        Destroy(this);            // Détruit ce composant (le bureau ne peut plus être copié)
    }

    /// <summary>
    /// Détecte l'entrée d'un joueur dans la zone de déclenchement.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Vérifie que c'est bien le joueur
        if (!other.CompareTag("Player"))
            return;

        ui.ShowInteractUI(true); // Affiche l'UI d'interaction
        gm.currentdesk = this;   // Définit ce bureau comme bureau courant dans le GameManager
    }

    /// <summary>
    /// Détecte la sortie du joueur de la zone de déclenchement.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        // Vérifie que c'est bien le joueur
        if (!other.CompareTag("Player"))
            return;

        ui.ShowInteractUI(false); // Cache l'UI d'interaction
        StopInteraction();        // Arrête la copie si elle était en cours
        gm.currentdesk = null;    // Réinitialise le bureau courant dans le GameManager
    }

    /// <summary>
    /// Désactive l'effet lumineux (émission) sur tous les matériaux du bureau.
    /// </summary>
    void MakeDeskNotGlow()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(); // Récupère tous les renderers enfants

        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                mat.DisableKeyword("_EMISSION"); // Désactive l'émission lumineuse
            }
        }
    }
}
