using UnityEngine;
using LitMotion;
using LitMotion.Extensions;

public class WaveAnimation : MonoBehaviour
{
    private GameObject iconObject;
    [SerializeField] private float minScale = 0.9f;// The minimum scale of the icon
    [SerializeField] private float maxScale = 1.1f;// The maximum scale of the icon
    [SerializeField] private float duration = 0.8f;// The duration of one scaling cycle

    private MotionHandle scaleMotion;// The motion handle for the scaling animation

    private void Awake()
    {
        iconObject = gameObject;
    }

    private void OnEnable()
    {
        // Initialize the icon scale
        iconObject.transform.localScale = Vector3.one * minScale;
        // Create a looping scaling motion
        scaleMotion = LMotion.Create(minScale, maxScale, duration)
            .WithEase(Ease.InOutSine)
            .WithLoops(-1, LoopType.Yoyo)
            .Bind(value =>
            {
                iconObject.transform.localScale = Vector3.one * value;
            });
    }

    private void OnDisable()
    {
        scaleMotion.Cancel();
    }
}
