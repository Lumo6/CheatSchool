using UnityEngine;
using UnityEngine.UI;

public class SuspicionMeterUI : MonoBehaviour
{
    public Image fillImage;

    public Color lowColor = Color.yellow;
    public Color highColor = Color.red;

    public void SetValue(float current, float max)
    {
        float normalized = Mathf.Clamp01(current / max);
        fillImage.fillAmount = normalized;
        fillImage.color = Color.Lerp(lowColor, highColor, normalized);
    }

    public void Hide()
    {
        fillImage.fillAmount = 0f;
    }
}
