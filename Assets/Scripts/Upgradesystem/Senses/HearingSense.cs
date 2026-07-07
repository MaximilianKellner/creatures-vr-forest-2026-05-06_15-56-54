using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

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

    private UpgradeSystem upgradeSystem;
    private bool isOnCooldown;

    private void Awake()
    {
        upgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();

        SetVolume(normalVolume);
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

    private IEnumerator HearingRoutine()
    {
        isOnCooldown = true;

        SetVolume(hearingVolume);
        Debug.Log("Hörsinn aktiviert.");

        yield return new WaitForSeconds(hearingDuration);

        SetVolume(normalVolume);
        Debug.Log("Hörsinn deaktiviert.");

        yield return new WaitForSeconds(cooldown);

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