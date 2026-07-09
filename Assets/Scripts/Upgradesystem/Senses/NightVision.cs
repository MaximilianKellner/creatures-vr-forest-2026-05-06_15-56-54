using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

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

    [Header("UI")]
    [SerializeField] private PlayerAbilityUI abilityUI;

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
                abilityUI?.SetLocked(PreyGivesUpgrade.NightVision);
                return;
            }

            StartCoroutine(NightVisionRoutine());
        }
    }

    private IEnumerator NightVisionRoutine()
    {
        isActive = true;

        abilityUI?.SetAbilityState(
            PreyGivesUpgrade.NightVision,
            true,
            0f
        );

        if (nightVisionVolume != null)
            nightVisionVolume.weight = 1f;

        if (nightVisionSound != null)
        {
            nightVisionSound.Stop();
            nightVisionSound.Play();
        }

        Debug.Log("Nachtsicht aktiviert.");

        yield return new WaitForSeconds(activeTime);

        if (nightVisionVolume != null)
            nightVisionVolume.weight = 0f;

        Debug.Log("Nachtsicht deaktiviert.");

        isActive = false;
        isOnCooldown = true;

        abilityUI?.SetAbilityState(
            PreyGivesUpgrade.NightVision,
            false,
            1f
        );

        float cooldownTimer = cooldown;

        while (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;

            abilityUI?.SetAbilityState(
                PreyGivesUpgrade.NightVision,
                false,
                cooldownTimer / cooldown
            );

            yield return null;
        }

        isOnCooldown = false;

        abilityUI?.SetReady(PreyGivesUpgrade.NightVision);

        Debug.Log("Nachtsicht wieder bereit.");
    }

    public bool IsActive => isActive;
    public bool IsOnCooldown => isOnCooldown;
}