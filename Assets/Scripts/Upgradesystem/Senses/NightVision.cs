using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.UI; // WICHTIG: Erlaubt die Steuerung des UI-Overlays

public class CreatureSenses : MonoBehaviour
{
    [Header("Upgrade")]
    [Tooltip("Auf false stellen, um die Nachtsicht direkt ohne Beute-Upgrade zu testen!")]
    [SerializeField] private bool needsUpgrade = true;

    [Header("Night Vision Settings")]
    [Tooltip("Ziehe hier das NightVision_Volume Objekt hinein")]
    public Volume nightVisionVolume;

    [Header("Timing")]
    [SerializeField] private float visionDuration = 5f; // Wie lange bleibt die Nachtsicht an?
    [SerializeField] private float cooldown = 10f;       // Wie lange lädt sie auf?

    [Header("UI Feedback")]
    [Tooltip("Ziehe hier das Cooldown_overlay GameObject oder Image hinein")]
    [SerializeField] private Image cooldownOverlay;

    private UpgradeSystem upgradeSystem;
    private bool isOnCooldown = false;

    private void Awake()
    {
        upgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();

        if (nightVisionVolume != null)
            nightVisionVolume.weight = 0f;

        // Sicherstellen, dass das Cooldown-Bild am Anfang unsichtbar ist
        if (cooldownOverlay != null) 
            cooldownOverlay.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Reagiert auf die Taste 'N'
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            TryUseNightVision();
        }
    }

    // PUBLIC, damit auch ein Klick auf den UI-Button diese Funktion auslösen kann!
    public void TryUseNightVision()
    {
        if (isOnCooldown)
        {
            Debug.Log("Nachtsicht ist noch im Cooldown.");
            return;
        }

        // Überprüfung des Upgrade-Systems (kann via Inspector deaktiviert werden)
        if (needsUpgrade &&
            (upgradeSystem == null || !upgradeSystem.HasUpgrade(PreyGivesUpgrade.NightVision)))
        {
            Debug.Log("Nachtsicht noch nicht freigeschaltet.");
            return;
        }

        StartCoroutine(NightVisionRoutine());
    }

    private IEnumerator NightVisionRoutine()
    {
        isOnCooldown = true;

        // 1. Nachtsicht einschalten
        if (nightVisionVolume != null)
            nightVisionVolume.weight = 1f;
        
        Debug.Log("Nachtsicht: AKTIVIERT");

        // Warte die aktive Dauer ab
        yield return new WaitForSeconds(visionDuration);

        // 2. Nachtsicht wieder ausschalten & Cooldown starten
        if (nightVisionVolume != null)
            nightVisionVolume.weight = 0f;

        Debug.Log("Nachtsicht: DEAKTIVIERT. Cooldown startet.");

        // Cooldown-Overlay aktivieren und voll anzeigen
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = 1f;
        }

        // Cooldown flüssig jede Frame runterzählen (Uhren-Effekt)
        float cooldownTimer = cooldown;
        while (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = cooldownTimer / cooldown;
            }
            
            yield return null; // Wartet bis zum nächsten Frame
        }

        // Cooldown beendet: Overlay wieder verstecken
        if (cooldownOverlay != null) 
            cooldownOverlay.gameObject.SetActive(false);

        isOnCooldown = false;
        Debug.Log("Nachtsicht wieder bereit.");
    }
}