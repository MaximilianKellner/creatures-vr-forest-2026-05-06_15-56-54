using UnityEngine;

public class GasMagicHoleYellow : MonoBehaviour
{
    [Header("Referenzen")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Gesamtes Missionsobjekt")]
    [Tooltip("Ziehe hier das Parent-Objekt Mission-2_Gas hinein.")]
    [SerializeField] private GameObject missionObject;

    [Tooltip("Verzögerung, bevor das gesamte Missionsobjekt verschwindet.")]
    [SerializeField] private float missionObjectDestroyDelay = 0.5f;

    [Header("Gas-Partikel")]
    [Tooltip("Das Gas-Partikelsystem oder das Parent-Objekt mit allen Gaspartikeln.")]
    [SerializeField] private GameObject gasParticleObject;

    [Header("Death Zone")]
    [Tooltip("Die DeathZoneGas2, die nach dem Schließen deaktiviert wird.")]
    [SerializeField] private GameObject deathZoneGas2;

    [Header("Gas-Schaden")]
    [SerializeField] private bool gasCausesDamage = true;

    [SerializeField] private float schadenProTick = 0.2f;
    [SerializeField] private float schadenIntervall = 1f;

    private float schadenTimer;
    private float schadenAkkumulator;

    [Header("Magische Flasche")]
    [Tooltip("Muss mit der Target Id der gelben Flasche übereinstimmen.")]
    [SerializeField] private string requiredMissionTargetId =
        "YellowMagicDrink";

    [Tooltip("Wie viele gelbe Flaschen werden benötigt?")]
    [SerializeField] private int benoetigteFlaschen = 1;

    [Header("Explosion")]
    [Tooltip("Explosion, wenn das Rätsel abgeschlossen wird.")]
    [SerializeField] private GameObject explosionsEffekt;

    [Tooltip("Optionales Loch-Objekt, das nach der Explosion entfernt wird.")]
    [SerializeField] private GameObject holeObject;

    [Tooltip("Soll das Loch nach Abschluss entfernt werden?")]
    [SerializeField] private bool disableHoleAfterCompletion = true;

    [Tooltip("Verzögerung, bevor das Loch entfernt wird.")]
    [SerializeField] private float disableDelay = 0.25f;

    [Header("Mission")]
    [Tooltip("Soll die gelbe Flasche an den MissionManager gemeldet werden?")]
    [SerializeField] private bool reportMissionProgress = true;

    private int flaschenVersenkt;
    private bool gasIstAktiv = true;
    private bool raetselGeloest;

    private void Start()
    {
        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            Debug.LogWarning(
                $"{name}: Kein PlayerHealth-Skript gefunden. " +
                "Der Gasschaden wird nicht ausgeführt."
            );
        }

        if (benoetigteFlaschen < 1)
        {
            benoetigteFlaschen = 1;
        }
    }

    private void Update()
    {
        if (!gasIstAktiv ||
            !gasCausesDamage ||
            playerHealth == null ||
            playerHealth.CurrentHealth <= 0)
        {
            return;
        }

        schadenTimer += Time.deltaTime;

        if (schadenTimer < schadenIntervall)
            return;

        schadenTimer = 0f;
        schadenAkkumulator += schadenProTick;

        if (schadenAkkumulator < 1f)
            return;

        int ganzerSchaden =
            Mathf.FloorToInt(schadenAkkumulator);

        playerHealth.TakeDamage(ganzerSchaden);
        schadenAkkumulator -= ganzerSchaden;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!gasIstAktiv || raetselGeloest)
            return;

        MissionTarget missionTarget =
            other.GetComponent<MissionTarget>() ??
            other.GetComponentInParent<MissionTarget>();

        if (missionTarget == null)
            return;

        if (missionTarget.TargetId != requiredMissionTargetId)
            return;

        GameObject potion = missionTarget.gameObject;

        // Die Flasche muss geworfen worden sein.
        if (potion.transform.parent != null)
        {
            Debug.Log(
                $"{potion.name}: Die gelbe Flasche muss in das Loch geworfen werden."
            );

            return;
        }

        // Mehrfachzählung verhindern.
        Collider[] potionColliders =
            potion.GetComponentsInChildren<Collider>();

        foreach (Collider potionCollider in potionColliders)
        {
            potionCollider.enabled = false;
        }

        flaschenVersenkt++;

        Debug.Log(
            $"Gelbe magische Flasche versenkt! " +
            $"({flaschenVersenkt}/{benoetigteFlaschen})"
        );

        if (reportMissionProgress)
        {
            MissionManager.ReportTargetCollected(
                missionTarget.TargetId
            );
        }

        Destroy(potion);

        if (flaschenVersenkt >= benoetigteFlaschen)
        {
            LoeseRaetsel();
        }
    }

    private void LoeseRaetsel()
    {
        if (raetselGeloest)
            return;

        raetselGeloest = true;
        gasIstAktiv = false;

        Debug.Log(
            "Das zweite Gasloch wurde mit der gelben magischen Flasche zerstört!"
        );

        StoppeGasPartikel();
        DeaktiviereDeathZone();

        if (explosionsEffekt != null)
        {
            Instantiate(
                explosionsEffekt,
                transform.position,
                Quaternion.identity
            );
        }

        // Optional nur das Loch entfernen.
        if (disableHoleAfterCompletion &&
            missionObject == null)
        {
            GameObject objectToDisable =
                holeObject != null
                    ? holeObject
                    : gameObject;

            Destroy(objectToDisable, disableDelay);
        }

        // Gesamtes Mission-2_Gas-Objekt entfernen.
        if (missionObject != null)
        {
            Destroy(
                missionObject,
                missionObjectDestroyDelay
            );
        }
        else
        {
            Debug.LogWarning(
                "GasMagicHoleYellow: Kein Mission Object zugewiesen."
            );
        }
    }

    private void StoppeGasPartikel()
    {
        if (gasParticleObject == null)
            return;

        ParticleSystem[] particleSystems =
            gasParticleObject.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        gasParticleObject.SetActive(false);
    }

    private void DeaktiviereDeathZone()
    {
        if (deathZoneGas2 == null)
            return;

        deathZoneGas2.SetActive(false);
    }
}