using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI; 

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

    [Header("UI Feedback")] 
    [SerializeField] private Image cooldownOverlay;

    private UpgradeSystem upgradeSystem;
    private bool isOnCooldown;
    
    // NEU: Merkt sich den Freischalt-Status
    private bool isUnlocked = false;

    private void Awake()
    {
        upgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();

        SetVolume(normalVolume);

        if (cooldownOverlay != null) 
            cooldownOverlay.gameObject.SetActive(false);
    }

    private void Start()
    {
        SetVolume(normalVolume);
        
        // NEU: Beim Start direkt prüfen, ob das Icon ausgegraut sein muss
        CheckUnlockStatus();
    }

    private void Update()
    {
        // NEU: Solange es gesperrt ist, auf das Upgrade warten
        if (!isUnlocked)
        {
            CheckUnlockStatus();
            return; // Verhindert Tastatureingaben, solange gesperrt
        }

        if (Keyboard.current == null)
            return;

        if (Keyboard.current[key].wasPressedThisFrame)
            TryUseHearing();
    }

    // NEU: Steuert das Ausgrauen des Buttons
    private void CheckUnlockStatus()
    {
        if (!needsUpgrade || (upgradeSystem != null && upgradeSystem.HasUpgrade(PreyGivesUpgrade.HearingSense)))
        {
            // Fähigkeit IST freigeschaltet
            if (!isUnlocked) 
            {
                isUnlocked = true;
                
                if (cooldownOverlay != null && !isOnCooldown)
                {
                    cooldownOverlay.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // Fähigkeit ist noch GESPERRT
            isUnlocked = false;
            
            if (cooldownOverlay != null)
            {
                cooldownOverlay.gameObject.SetActive(true);
                cooldownOverlay.fillAmount = 1f; // Vollständig ausgrauen
            }
        }
    }

    public void TryUseHearing()
    {
        // NEU: Einfachere Abfrage durch die isUnlocked-Variable (wichtig für Klicks per UI-Button)
        if (!isUnlocked)
        {
            Debug.Log("Hörsinn noch nicht freigeschaltet.");
            return;
        }

        if (isOnCooldown)
        {
            Debug.Log("Hörsinn ist noch im Cooldown.");
            return;
        }

        StartCoroutine(HearingRoutine());
    }

    private IEnumerator HearingRoutine()
    {
        isOnCooldown = true;

        SetVolume(hearingVolume);
        Debug.Log("Hörsinn aktiviert.");

        yield return new WaitForSeconds(hearingDuration);

        SetVolume(normalVolume);
        Debug.Log("Hörsinn deaktiviert. Cooldown startet.");

        // --- VISUELLER COOLDOWN ---
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = 1f;
        }

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