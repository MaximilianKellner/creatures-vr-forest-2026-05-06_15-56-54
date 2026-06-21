using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI; // WICHTIG: Ermöglicht die Steuerung des UI-Bildes

public class SonarSense : MonoBehaviour
{
    [Header("Upgrade")]
    [Tooltip("Auf false stellen, um den Sonar-Sinn direkt ohne Beute-Upgrade zu testen!")]
    [SerializeField] private bool needsUpgrade = true;

    [Header("Input")]
    [SerializeField] private Key key = Key.B;

    [SerializeField] private Transform playerPosition;

    [Header("Sonar Settings")]
    [SerializeField] private float sonarRadius = 25f;
    [SerializeField] private float sonarVisibleTime = 2f;
    [SerializeField] private float cooldown = 3f;
    [SerializeField] private LayerMask sonarMask;

    [Header("Wave Visual")]
    [SerializeField] private GameObject wavePrefab;
    [SerializeField] private float waveDuration = 1.2f;
    [SerializeField] private float waveHeightOffset = 0.05f;

    [Header("Post Processing Optional")]
    [SerializeField] private Volume sonarVolume;
    [SerializeField] private float volumeFadeSpeed = 5f;

    [Header("Sound Optional")]
    [SerializeField] private AudioSource sonarSound;

    [Header("UI Feedback")]
    [Tooltip("Ziehe hier das Cooldown_overlay für den Sonar-Button hinein")]
    [SerializeField] private Image cooldownOverlay;

    private UpgradeSystem upgradeSystem;
    private bool isOnCooldown;
    private Coroutine volumeRoutine;

    private void Awake()
    {
        upgradeSystem =
            GetComponentInParent<UpgradeSystem>() ??
            GetComponentInChildren<UpgradeSystem>();

        if (playerPosition == null)
            playerPosition = transform;

        if (sonarVolume != null)
            sonarVolume.weight = 0f;

        // Sicherstellen, dass das Cooldown-Bild am Anfang unsichtbar ist
        if (cooldownOverlay != null) 
            cooldownOverlay.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[key].wasPressedThisFrame)
            TryUseSonar();
    }

    // AUF PUBLIC GEÄNDERT: Damit der UI-Button diese Funktion direkt auslösen kann
    public void TryUseSonar()
    {
        if (isOnCooldown)
        {
            Debug.Log("Ultraschall ist noch im Cooldown.");
            return;
        }

        if (needsUpgrade &&
            (upgradeSystem == null || !upgradeSystem.HasUpgrade(PreyGivesUpgrade.SonarSense)))
        {
            Debug.Log("Ultraschall noch nicht freigeschaltet.");
            return;
        }

        StartCoroutine(SonarRoutine());
    }

    private IEnumerator SonarRoutine()
    {
        isOnCooldown = true;

        if (sonarSound != null)
            sonarSound.Play();

        SpawnWave();
        RevealTargets();
        StartVolumeEffect();

        // --- AB HIER STARTET DER VISUELLE COOLDOWN ---
        // Overlay sichtbar machen und voll ausfüllen
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = 1f;
        }

        // Der Kreis läuft flüssig jede Frame als Uhr ab (über die gesamte Cooldown-Dauer)
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

        // Cooldown vorbei: Kreis wieder unsichtbar machen
        if (cooldownOverlay != null) 
            cooldownOverlay.gameObject.SetActive(false);

        isOnCooldown = false;
        Debug.Log("Ultraschall wieder bereit.");
    }

    private void SpawnWave()
    {
        if (wavePrefab == null || playerPosition == null)
            return;

        Vector3 spawnPosition = playerPosition.position;
        spawnPosition.y += waveHeightOffset;

        GameObject wave = Instantiate(
            wavePrefab,
            spawnPosition,
            Quaternion.identity,
            playerPosition
        );

        SonarWave waveScript = wave.GetComponent<SonarWave>();

        if (waveScript != null)
            waveScript.Play(sonarRadius, waveDuration);
    }

    private void RevealTargets()
    {
        Collider[] hits = Physics.OverlapSphere(
            playerPosition.position,
            sonarRadius,
            sonarMask
        );

        foreach (Collider hit in hits)
        {
            SonarTarget target =
                hit.GetComponentInParent<SonarTarget>() ??
                hit.GetComponentInChildren<SonarTarget>();

            if (target != null)
                target.ShowSonar(sonarVisibleTime);
        }

        Debug.Log("Ultraschall hat " + hits.Length + " Collider erkannt.");
    }

    private void StartVolumeEffect()
    {
        if (sonarVolume == null)
            return;

        if (volumeRoutine != null)
            StopCoroutine(volumeRoutine);

        volumeRoutine = StartCoroutine(VolumePulse());
    }

    private IEnumerator VolumePulse()
    {
        while (sonarVolume.weight < 1f)
        {
            sonarVolume.weight += Time.deltaTime * volumeFadeSpeed;
            yield return null;
        }

        sonarVolume.weight = 1f;

        yield return new WaitForSeconds(sonarVisibleTime);

        while (sonarVolume.weight > 0f)
        {
            sonarVolume.weight -= Time.deltaTime * volumeFadeSpeed;
            yield return null;
        }

        sonarVolume.weight = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = playerPosition != null ? playerPosition : transform;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin.position, sonarRadius);
    }
}