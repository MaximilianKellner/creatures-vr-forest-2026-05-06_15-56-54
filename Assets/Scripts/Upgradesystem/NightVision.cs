using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CreatureSenses : MonoBehaviour
{
    [Header("Night Vision Settings")]
    [Tooltip("Ziehe hier das NightVision_Volume Objekt hinein")]
    public Volume nightVisionVolume;

    private bool isNightVisionActive = false;
    private UpgradeSystem upgradeSystem;
    
    // Speichert die originalen Materialfarben zum Zurücksetzen
    private Dictionary<Renderer, Color> originalEmissionColors = new Dictionary<Renderer, Color>();

    private void Awake()
    {
        upgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();

        if (nightVisionVolume != null)
            nightVisionVolume.weight = 0f;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.nKey.wasPressedThisFrame)
        {
            if (upgradeSystem == null || !upgradeSystem.HasUpgrade(PreyGivesUpgrade.NightVision))
            {
                Debug.Log("Thermal-Sicht noch nicht freigeschaltet.");
                return;
            }

            ToggleNightVision();
        }
    }

    private void ToggleNightVision()
    {
        isNightVisionActive = !isNightVisionActive;

        if (nightVisionVolume != null)
        {
            nightVisionVolume.weight = isNightVisionActive ? 1f : 0f;
        }

        ApplyThermalSignatures(isNightVisionActive);
    }

    private void ApplyThermalSignatures(bool activate)
    {
        if (activate)
        {
            originalEmissionColors.Clear();

            // 1. Alle Beutetiere finden und individuelle Helligkeit verpassen
            Prey[] allPreys = FindObjectsByType<Prey>(FindObjectsSortMode.None);
            foreach (Prey prey in allPreys)
            {
                float intensitaet = 1.0f; // Standard für normale Fliegen
                string name = prey.gameObject.name.ToLower();

                if (name.Contains("crab"))
                {
                    intensitaet = 1.4f; // Krabben leuchten etwas massiver am Boden
                }
                else if (name.Contains("poised") || name.Contains("special"))
                {
                    intensitaet = 1.7f; // Gefährliche Fliegen strahlen intensiver
                }

                ApplyBloodRedLook(prey.gameObject, intensitaet);
            }

            // 2. Den Hauptfeind finden (Er ist die absolut massivste, bedrohlichste Hitzequelle)
            PlayerEnemy[] allEnemies = FindObjectsByType<PlayerEnemy>(FindObjectsSortMode.None);
            foreach (PlayerEnemy enemy in allEnemies)
            {
                float gegnerIntensitaet = 2.8f; // Extrem helles, furchteinflößendes Blutrot!
                ApplyBloodRedLook(enemy.gameObject, gegnerIntensitaet);
            }
        }
        else
        {
            // Reset-Logik: Bringt alle Materialien in den Normalzustand
            foreach (KeyValuePair<Renderer, Color> kvp in originalEmissionColors)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.material.SetColor("_EmissionColor", kvp.Value);
                    if (kvp.Value == Color.black)
                    {
                        kvp.Key.material.DisableKeyword("_EMISSION");
                    }
                }
            }
        }
    }

    private void ApplyBloodRedLook(GameObject target, float targetMultiplier)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];
            if (rend.material == null) continue;

            if (!originalEmissionColors.ContainsKey(rend))
            {
                Color currentEmission = rend.material.HasProperty("_EmissionColor") 
                    ? rend.material.GetColor("_EmissionColor") 
                    : Color.black;
                
                originalEmissionColors.Add(rend, currentEmission);
            }

            // --- INTENSIVE BLOOD RED PALETTE (PULSIERENDE ROTSTUFEN) ---
            Color bloodRedColor;
            int zoneIndex = i % 4; 

            switch (zoneIndex)
            {
                case 0:
                    // Gleißendes, frisches Crimson / Heißer Kern (HDR Intensität: 12)
                    // Ein ganz kleiner Hauch Blau/Grün sorgt dafür, dass das Rot im Kern ins Weißliche übersteuert
                    bloodRedColor = new Color(12f, 0.4f, 0.2f); 
                    break;
                case 1:
                    // Sattes, reines Signal-Blutrot (HDR Intensität: 8)
                    bloodRedColor = new Color(8f, 0f, 0f); 
                    break;
                case 2:
                    // Mittleres, pulsierendes Dunkelrot (HDR Intensität: 5)
                    bloodRedColor = new Color(5f, 0f, 0f); 
                    break;
                default:
                    // Tiefes, geronnenes Dunkelrot an den Gliedmaßen/Rändern (HDR Intensität: 2.5)
                    bloodRedColor = new Color(2.5f, 0f, 0f); 
                    break;
            }

            // Multiplizieren mit der individuellen Intensität des jeweiligen Prefabs
            bloodRedColor *= targetMultiplier;

            // Zuweisen an das Material mit aktiviertem Leuchte-Effekt
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", bloodRedColor);
        }
    }
}