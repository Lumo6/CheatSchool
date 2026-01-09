using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CopyTargetDesk : MonoBehaviour
{
    [SerializeField] private GameObject interactUI;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public void StartInteraction()
    {
        Debug.Log($"Start copying at desk: {gameObject.name}");
        GameManager.Instance.copying();
    }

    public void StopInteraction()
    {
        Debug.Log($"Stop copying at desk: {gameObject.name}");
        GameManager.Instance.stopcopying();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"Player entered desk trigger: {gameObject.name}");
        interactUI?.SetActive(true);

        other.GetComponent<PlayerControls>()?.SetCurrentDesk(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"Player exited desk trigger: {gameObject.name}");
        interactUI?.SetActive(false);

        other.GetComponent<PlayerControls>()?.ClearCurrentDesk(this);
    }
}
