using UnityEngine;
public class PlayerDesk : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.CheckWin();
    }
}

