using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class SonarSense : MonoBehaviour
{
    [Header("Upgrade")]
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
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current[key].wasPressedThisFrame)
            TryUseSonar();
    }

    private void TryUseSonar()
    {
        if (isOnCooldown)
            return;

        if (needsUpgrade &&
            (upgradeSystem == null ||
             !upgradeSystem.HasUpgrade(PreyGivesUpgrade.SonarSense)))
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

        yield return new WaitForSeconds(cooldown);

        isOnCooldown = false;
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