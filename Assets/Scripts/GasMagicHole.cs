using UnityEngine;

public class GasMagicHole : MonoBehaviour
{
    [Header("Referenzen")]
    private PlayerHealth playerHealth;

    [Header("Start-Verzögerung")]
    [Tooltip("Wie viele Sekunden soll das Gas warten? (z.B. 60 für 1 Minute)")]
    public float startVerzoegerung = 60f;
    private float startTimer = 0f;
    private bool istGasAusgebrochen = false;

    [Header("Gas-Container (alle Gaslemente)")]
    public Transform gasContainer;
    public float zeitProPhase = 4f;

    [Header("Gas-Schaden")]
    public float schadenProTick = 0.2f;
    public float schadenIntervall = 1.0f;
    private float schadenAkkumulator = 0f;

    [Header("Effekte")]
    public GameObject portionTrefferEffekt;
    public GameObject raetselGeloestEffekt;

    [Header("Mission")]
    [Tooltip("Muss mit Required Target Id im MissionManager übereinstimmen.")]
    [SerializeField] private string requiredMissionTargetId = "MagicDrink";

    private GameObject[] gasStages;
    private int portionenVersenkt = 0;

    [SerializeField] private int benoetigtePortionen = 3;

    private bool gasIstAktiv = true;

    private float schadenTimer;
    private float phasenTimer;
    private int aktuellePhase = 0;

    private void Start()
    {
        playerHealth = FindAnyObjectByType<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError(
                "GasMagicHole: Kein PlayerHealth-Skript auf dem Spieler gefunden!"
            );
        }

        if (gasContainer != null)
        {
            gasStages = new GameObject[gasContainer.childCount];

            for (int i = 0; i < gasContainer.childCount; i++)
            {
                gasStages[i] = gasContainer.GetChild(i).gameObject;
                gasStages[i].SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!gasIstAktiv ||
            playerHealth == null ||
            playerHealth.CurrentHealth <= 0)
        {
            return;
        }

        // Schonfrist-Countdown
        if (!istGasAusgebrochen)
        {
            startTimer += Time.deltaTime;

            if (startTimer >= startVerzoegerung)
            {
                istGasAusgebrochen = true;

                if (gasStages != null && gasStages.Length > 0)
                {
                    gasStages[0].SetActive(true);
                }
            }

            return;
        }

        // Laufender Giftschaden mit Float-Akkumulator
        schadenTimer += Time.deltaTime;

        if (schadenTimer >= schadenIntervall)
        {
            schadenTimer = 0f;
            schadenAkkumulator += schadenProTick;

            if (schadenAkkumulator >= 1f)
            {
                int ganzerSchaden =
                    Mathf.FloorToInt(schadenAkkumulator);

                playerHealth.TakeDamage(ganzerSchaden);
                schadenAkkumulator -= ganzerSchaden;
            }
        }

        // Allmähliche Gas-Ausbreitung
        if (gasStages != null &&
            aktuellePhase < gasStages.Length - 1)
        {
            phasenTimer += Time.deltaTime;

            if (phasenTimer >= zeitProPhase)
            {
                phasenTimer = 0f;
                aktuellePhase++;

                if (gasStages[aktuellePhase] != null)
                {
                    gasStages[aktuellePhase].SetActive(true);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
{
    if (!gasIstAktiv)
        return;

    // MissionTarget auf dem getroffenen Objekt oder Parent suchen
    MissionTarget missionTarget =
        other.GetComponent<MissionTarget>() ??
        other.GetComponentInParent<MissionTarget>();

    if (missionTarget == null)
        return;

    // Nur magische Getränke akzeptieren
    if (missionTarget.TargetId != requiredMissionTargetId)
        return;

    // Das eigentliche Fläschchen bestimmen
    GameObject potion = missionTarget.gameObject;

    // Anti-Mogel:
    // Das Fläschchen darf nicht mehr getragen werden.
    if (potion.transform.parent != null)
        return;

    portionenVersenkt++;

    Debug.Log(
        $"Potion versenkt! ({portionenVersenkt}/{benoetigtePortionen})"
    );

    // Missionsfortschritt melden
    MissionManager.ReportTargetCollected(missionTarget.TargetId);

    // Treffer-Effekt
    if (portionTrefferEffekt != null)
    {
        Instantiate(
            portionTrefferEffekt,
            transform.position,
            Quaternion.identity
        );
    }

    // Fläschchen zerstören
    Destroy(potion);

    if (portionenVersenkt >= benoetigtePortionen)
    {
        StoppeGasWolke();
    }
}

    private void StoppeGasWolke()
    {
        gasIstAktiv = false;

        Debug.Log(
            "Geschafft! Das magische Loch ist versiegelt!"
        );

        // Spawnt den großen Erfolgs-Effekt
        if (raetselGeloestEffekt != null)
        {
            Instantiate(
                raetselGeloestEffekt,
                transform.position,
                Quaternion.identity
            );
        }

        if (gasStages != null)
        {
            foreach (GameObject gas in gasStages)
            {
                if (gas != null)
                {
                    gas.SetActive(false);
                }
            }
        }
    }
}