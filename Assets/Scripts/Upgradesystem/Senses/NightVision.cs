using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;

public class CreatureSenses : MonoBehaviour
{
    [Header("Night Vision Settings")]
    [Tooltip("Ziehe hier das NightVision_Volume Objekt hinein")]
    public Volume nightVisionVolume;

    private bool isNightVisionActive = false;
    private UpgradeSystem upgradeSystem;

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
                Debug.Log("Nachtsicht noch nicht freigeschaltet.");
                return;
            }

            ToggleNightVisionVR();
        }
    }

    public void ToggleNightVisionVR()
    {
        isNightVisionActive = !isNightVisionActive;

        if (nightVisionVolume == null)
            return;

        nightVisionVolume.weight = isNightVisionActive ? 1f : 0f;

        Debug.Log(isNightVisionActive
            ? "Nachtsicht: AKTIVIERT"
            : "Nachtsicht: DEAKTIVIERT");
    }
}
