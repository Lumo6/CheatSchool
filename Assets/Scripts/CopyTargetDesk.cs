using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CopyTargetDesk : MonoBehaviour
{
    private void Reset()
    {
        // Ensure collider is trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player entered copy target desk: {gameObject.name}");
            GameManager.Instance.copying();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player exited copy target desk: {gameObject.name}");
            GameManager.Instance.stopcopying();
        }
    }
}
