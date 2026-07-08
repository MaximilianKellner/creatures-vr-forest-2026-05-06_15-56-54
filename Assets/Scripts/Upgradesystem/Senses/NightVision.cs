using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

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

    [Header("UI Feedback")]
    [SerializeField] private Image cooldownOverlay;

    private UpgradeSystem upgradeSystem;

    private bool isActive;
    private bool isOnCooldown;
    
    // NEU: Merkt sich, ob die Fähigkeit im laufenden Spiel freigeschaltet wurde
    private bool isUnlocked = false; 

    private void Awake()
    {
        upgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();

        if (nightVisionVolume != null)
            nightVisionVolume.weight = 0f;
    }

    private void Start()
    {
        // NEU: Beim Start direkt prüfen, ob das Icon ausgegraut sein muss oder nicht
        CheckUnlockStatus();
    }

    private void Update()
    {
        // NEU: Solange es gesperrt ist, checken wir, ob der Spieler das Upgrade gerade eingesammelt hat
        if (!isUnlocked)
        {
            CheckUnlockStatus();
            return; // Verhindert jegliche Eingabe, solange es gesperrt ist
        }

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            if (isActive || isOnCooldown)
                return;

            StartCoroutine(NightVisionRoutine());
        }
    }

    // NEU: Diese Methode steuert das Ausgrauen des Buttons
    private void CheckUnlockStatus()
    {
        // Prüfen, ob ein Upgrade nötig ist UND ob wir es schon haben
        if (!needsUpgrade || (upgradeSystem != null && upgradeSystem.HasUpgrade(PreyGivesUpgrade.NightVision)))
        {
            // Fähigkeit IST freigeschaltet!
            if (!isUnlocked) 
            {
                isUnlocked = true;
                
                // Grauen Schleier entfernen (nur wenn wir nicht zufällig gerade im Cooldown sind)
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
                cooldownOverlay.fillAmount = 1f; // Overlay zu 100% füllen (ausgrauen)
            }
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

        Debug.Log("Nachtsicht wieder bereit.");
    }

    public bool IsActive => isActive;
    public bool IsOnCooldown => isOnCooldown;
}