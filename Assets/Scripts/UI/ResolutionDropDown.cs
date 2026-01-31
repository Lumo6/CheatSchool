using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class ResolutionDropDown : MonoBehaviour
{
    public TMPro.TMP_Dropdown resolutionDropdown;// Reference to the dropdown UI element

    Resolution[] resolutions;/// Array to hold available screen resolutions

    /// <summary>
    /// Initialisation de la liste déroulante des résolutions disponibles.
    /// </summary>
    public void Start()
    {
        // Récupère les résolutions d'écran disponibles et les filtre pour éviter les doublons
        resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().ToArray();
        resolutionDropdown.ClearOptions();
        // Crée une liste pour stocker les options de résolution
        List<string> options = new List<string>();
        // Indice de la résolution actuelle
        int currentResolutionIndex = 0;
        // Parcourt les résolutions disponibles pour les ajouter à la liste des options
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            // Vérifie si cette résolution correspond à la résolution actuelle de l'écran
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }
        // Ajoute les options à la liste déroulante et sélectionne la résolution actuelle
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        Screen.fullScreen = true;
    }

    /// <summary>
    /// Définit la résolution d'écran en fonction de la sélection dans la liste déroulante.
    /// </summary>
    public void SetResolution()
    {
        // Définit la résolution d'écran en fonction de la sélection dans la liste déroulante
        Resolution resolution = resolutions[resolutionDropdown.value];
        Debug.Log("Setting resolution to: " + resolution.width + " x " + resolution.height);
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

}
