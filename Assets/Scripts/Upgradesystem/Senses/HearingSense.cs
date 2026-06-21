using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI; // 1. WICHTIG: Erlaubt uns die Steuerung von UI-Bildern

public class HearingSense : MonoBehaviour
{
    [Header("Upgrade")]
    [SerializeField] private bool needsUpgrade = true;

    [Header("Input")]
    [SerializeField] private Key key = Key.H;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string volumeParameter = "WorldVolume";

    [Header("Volume Settings")]
    [SerializeField] private float normalVolume = -25f;
    [SerializeField] private float hearingVolume = 0f;

    [Header("Timing")]
    [SerializeField] private float hearingDuration = 5f;
    [SerializeField] private float cooldown = 10f;

    [Header("UI Feedback")] // 2. Hier fügen wir deinen weißen Kreis ein
    [SerializeField] private Image cooldownOverlay;

    private UpgradeSystem upgradeSystem;
    private bool isOnCooldown;

    private void Awake()
    {
        upgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();

        SetVolume(normalVolume);

        // Sicherstellen, dass der weiße Kreis am Anfang unsichtbar ist
        if (cooldownOverlay != null) 
            cooldownOverlay.gameObject.SetActive(false);
    }

    private void Start()
    {
        SetVolume(normalVolume);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[key].wasPressedThisFrame)
            TryUseHearing();
    }

    // 3. Auf PUBLIC geändert, damit der UI-Button diese Funktion auslösen kann!
    public void TryUseHearing()
    {
        if (isOnCooldown)
        {
            Debug.Log("Hörsinn ist noch im Cooldown.");
            return;
        }

        if (needsUpgrade &&
            (upgradeSystem == null ||
             !upgradeSystem.HasUpgrade(PreyGivesUpgrade.HearingSense)))
        {
            Debug.Log("Hörsinn noch nicht freigeschaltet.");
            return;
        }

        StartCoroutine(HearingRoutine());
    }

    // 4. Die Coroutine steuert jetzt den weißen Kreis
    private IEnumerator HearingRoutine()
    {
        isOnCooldown = true;

        SetVolume(hearingVolume);
        Debug.Log("Hörsinn aktiviert.");

        // Warte, solange der Hörsinn aktiv ist (z.B. 5 Sekunden)
        yield return new WaitForSeconds(hearingDuration);

        SetVolume(normalVolume);
        Debug.Log("Hörsinn deaktiviert. Cooldown startet.");

        // --- AB HIER STARTET DER COOLDOWN ---
        // Weißen Kreis sichtbar machen und voll ausfüllen
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = 1f;
        }

        // Der Kreis läuft jetzt flüssig jede Frame als Uhr ab (z.B. 10 Sekunden lang)
        float cooldownTimer = cooldown;
        while (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime; // Zieht die vergangene Zeit ab
            
            if (cooldownOverlay != null)
            {
                // Berechnet den Kreis-Fortschritt (zwischen 1.0 und 0.0)
                cooldownOverlay.fillAmount = cooldownTimer / cooldown;
            }
            
            yield return null; // Wartet bis zum nächsten Frame
        }

        // Cooldown vorbei: Kreis wieder unsichtbar machen
        if (cooldownOverlay != null) 
            cooldownOverlay.gameObject.SetActive(false);

        isOnCooldown = false;
        Debug.Log("Hörsinn wieder bereit.");
    }

    private void SetVolume(float volume)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("Kein AudioMixer im HearingSense zugewiesen.");
            return;
        }

        audioMixer.SetFloat(volumeParameter, volume);
    }
}