using UnityEngine;
using UnityEngine.Rendering; // Nötig für das Volume-System
using UnityEngine.InputSystem; // NEU: Nötig für das neue Input System

public class CreatureSenses : MonoBehaviour
{
    [Header("Night Vision Settings")]
    [Tooltip("Ziehe hier das NightVision_Volume Objekt hinein")]
    public Volume nightVisionVolume;
    
    private bool isNightVisionActive = false;

    void Update()
    {
        // NEU: Das neue Input System nutzt 'Keyboard.current' statt 'Input.GetKeyDown'
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
        {
            ToggleNightVision();
        }
    }

    void ToggleNightVision()
    {
        isNightVisionActive = !isNightVisionActive;

        if (isNightVisionActive)
        {
            nightVisionVolume.weight = 1f; // Schaltet den Effekt ein
            Debug.Log("Evrans Nachtsicht: AKTIVIERT (Neues Input System)");
        }
        else
        {
            nightVisionVolume.weight = 0f; // Schaltet den Effekt aus
            Debug.Log("Evrans Nachtsicht: DEAKTIVIERT (Neues Input System)");
        }
    }
}