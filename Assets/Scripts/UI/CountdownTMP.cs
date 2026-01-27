using UnityEngine;
using TMPro;
using LitMotion;

public class CountdownTMP : MonoBehaviour
{
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private int startTime = 3;


    private void OnEnable()
    {
        StartCountdown(startTime);
    }

    private void StartCountdown(int from)
    {
        LMotion.Create(from, 0, from)
        .WithEase(Ease.Linear)
        .WithScheduler(MotionScheduler.UpdateRealtime)
        .Bind(value =>
        {
            int v = Mathf.CeilToInt(value);
            countdownText.text = v > 0 ? v.ToString() : "GO!";
        });
    }
}
