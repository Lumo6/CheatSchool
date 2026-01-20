using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class CopyTargetDesk : MonoBehaviour
{

    private Coroutine copyCoroutine;
    private GameManager gm;
    private UIManager ui;

    private void Awake()
    {
        gm = GameManager.Instance;
        ui = UIManager.Instance;
    }

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public void StartInteraction(PlayerControls player)
    {
        if (copyCoroutine != null) return;

        gm.StartCopying();
        copyCoroutine = StartCoroutine(CopyRoutine(player));
    }

    public void StopInteraction()
    {
        if (copyCoroutine != null)
        {
            StopCoroutine(copyCoroutine);
            copyCoroutine = null;
            gm.StopCopying();
        }
    }

    private IEnumerator CopyRoutine(PlayerControls player)
    {
        float timer = 0f;

        while (timer < gm.copyDuration)
        {
            if (player.IsMoving())
            {
                Debug.Log("Player moved, copy failed");
                StopInteraction();
                yield break;
            }
            UIManager.Instance.updateCurrentCopyProgressUI(timer / gm.copyDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        ui.ShowInteractUI(false);
        gm.CopyCompleted();
        copyCoroutine = null;
        MakeDeskNotGlow();
        Destroy(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) 
            return;

        if(ui.interactUI == null)
            return;

        ui.ShowInteractUI(true);
        gm.currentdesk = this;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) 
            return;

        if (ui.interactUI == null)
            return;

        ui.ShowInteractUI(false);
        StopInteraction();
        gm.currentdesk = null;
    }

    void MakeDeskNotGlow()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            foreach (Material mat in rend.materials)
            {
                mat.DisableKeyword("_EMISSION");
            }
        }
    }
}
