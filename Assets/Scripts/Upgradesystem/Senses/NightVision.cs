using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI; // 1. WICHTIG: Wurde hinzugefügt für das UI-Image

public class NightVision : MonoBehaviour
{
    [Header("Upgrade")]
    [SerializeField] private bool needsUpgrade = true;

    [Header("Night Vision")]
    [SerializeField] private Volume nightVisionVolume;

    [SerializeField] private float activeTime = 5f;
    [SerializeField] private float cooldown = 8f;

    [Header("Audio")]
    [SerializeField] private AudioSource nightVisionSound;

    [Header("UI Feedback")] // 2. Das Feld für den Cooldown-Kreis im Inspector
    [SerializeField] private Image cooldownOverlay;

    private UpgradeSystem upgradeSystem;

    private bool isActive;
    private bool isOnCooldown;

    private void Awake()
    {
        upgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();

        if (nightVisionVolume != null)
            nightVisionVolume.weight = 0f;

        // Sicherstellen, dass der weiße Kreis am Anfang unsichtbar ist
        if (cooldownOverlay != null)
            cooldownOverlay.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            if (isActive || isOnCooldown)
                return;

            if (needsUpgrade &&
                (upgradeSystem == null ||
                 !upgradeSystem.HasUpgrade(PreyGivesUpgrade.NightVision)))
            {
                Debug.Log("Nachtsicht noch nicht freigeschaltet.");
                return;
            }

            StartCoroutine(NightVisionRoutine());
        }
    }

    private IEnumerator NightVisionRoutine()
    {
        isActive = true;

        if (nightVisionVolume != null)
            nightVisionVolume.weight = 1f;
        
        if (nightVisionSound != null)
            nightVisionSound.Play();

        Debug.Log("Nachtsicht aktiviert.");

        yield return new WaitForSeconds(activeTime);

        if (nightVisionVolume != null)
            nightVisionVolume.weight = 0f;

        Debug.Log("Nachtsicht deaktiviert. Cooldown startet.");

        isActive = false;
        isOnCooldown = true;

        // --- AB HIER STARTET DER VISUELLE COOLDOWN ---
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = 1f;
        }

        // Der Kreis läuft jetzt flüssig ab
        float cooldownTimer = cooldown;
        while (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime; 
            
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = cooldownTimer / cooldown;
            }
            
            yield return null; 
        }

        // Kreis wieder unsichtbar machen
        if (cooldownOverlay != null) 
            cooldownOverlay.gameObject.SetActive(false);

        isOnCooldown = false;

        Debug.Log("Nachtsicht wieder bereit.");
    }

    public bool IsActive => isActive;
    public bool IsOnCooldown => isOnCooldown;
}