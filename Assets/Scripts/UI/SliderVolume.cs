using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Classe SliderVolume
/// <para>
/// Classe qui gère le contrôle du volume via un slider.
/// Elle permet de changer le niveau sonore global en ajustant la valeur du slider.
/// <para>
/// </summary>
public class SliderVolume : MonoBehaviour
{
    [Header("Références du SliderVolume")]
    [Tooltip("Slider pour ajuster le volume")]
    [SerializeField] private Slider volumeSlider; // Le slider utilisé pour ajuster le volume

    void Start()
    {
        if (volumeSlider != null)
        {
            // Ajoute un écouteur d'événements pour changer le volume lorsque le slider est modifié
            volumeSlider.onValueChanged.AddListener(ChangeVolume);
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 0.5f); // Charge le volume sauvegardé ou utilise 0.5 par défaut
        }
    }

    /// <summary>
    /// Change le volume global en fonction de la valeur du slider.
    /// </summary>
    /// <param name="volume">Le nouveau volume à appliquer.</param>
    public void ChangeVolume(float volume)
    {
        PlayerPrefs.SetFloat("Volume", volume); // Met à jour le niveau sonore global
        SoundFXManager.Instance.updatePlayingSoundVolume(); // Met à jour le volume des sons en cours de lecture
    }
}
