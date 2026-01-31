using UnityEngine;
public class PlayerDesk : MonoBehaviour
{
    // Lorsqu'un objet entre en collision avec le trigger
    private void OnTriggerEnter(Collider other)
    {
        // Vérifie si l'objet entrant est le joueur
        if (!other.CompareTag("Player")) return;
        // Appelle la méthode CheckWin du GameManager
        GameManager.Instance.CheckWin();
    }
}

