using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class CopyTargetDesk : MonoBehaviour
{
    [SerializeField] private GameObject interactUI;

    private Coroutine copyCoroutine;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public void StartInteraction(PlayerControls player)
    {
        if (copyCoroutine != null) return;

        GameManager.Instance.StartCopying();
        copyCoroutine = StartCoroutine(CopyRoutine(player));
    }

    public void StopInteraction()
    {
        if (copyCoroutine != null)
        {
            StopCoroutine(copyCoroutine);
            copyCoroutine = null;
            GameManager.Instance.StopCopying();
        }
    }

    private IEnumerator CopyRoutine(PlayerControls player)
    {
        float timer = 0f;

        while (timer < GameManager.Instance.copyDuration)
        {
            if (player.IsMoving())
            {
                Debug.Log("Player moved, copy failed");
                StopInteraction();
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        GameManager.Instance.CopyCompleted();
        copyCoroutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) 
            return;

        if(interactUI == null)
            return;

        interactUI.SetActive(true);
        other.GetComponent<PlayerControls>().SetCurrentDesk(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) 
            return;

        if (interactUI == null)
            return;

        interactUI?.SetActive(false);
        StopInteraction();
        other.GetComponent<PlayerControls>()?.ClearCurrentDesk(this);
    }
}
