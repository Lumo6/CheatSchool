using UnityEngine;
using LitMotion;
using LitMotion.Extensions;

public class WaveAnimation : MonoBehaviour
{
    private GameObject iconObject;
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;
    [SerializeField] private float duration = 0.8f;

    private MotionHandle scaleMotion;

    private void Awake()
    {
        iconObject = gameObject;
    }

    private void OnEnable()
    {
        iconObject.transform.localScale = Vector3.one * minScale;

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
