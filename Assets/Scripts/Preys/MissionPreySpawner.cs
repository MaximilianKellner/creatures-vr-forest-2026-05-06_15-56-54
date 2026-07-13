using UnityEngine;

public class MissionPreySpawner : MonoBehaviour
{
    [Header("Mission")]
    [Tooltip("Bei welcher Missions-ID soll gezählt werden?")]
    [SerializeField] private string requiredTargetId = "MagicDrink";

    [Tooltip("Wie viele Ziele müssen abgeschlossen werden?")]
    [SerializeField] private int requiredAmount = 3;

    [Header("Beute")]
    [Tooltip("Prefab der lila Krabbe")]
    [SerializeField] private GameObject preyPrefab;

    [Tooltip("Positionen, an denen die Krabben erscheinen")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Layer")]
    [Tooltip("Layer, den die gespawnte Beute erhalten soll")]
    [SerializeField] private string preyLayerName = "Prey";

    [Tooltip("Setzt den Layer auch auf alle Child-Objekte der Krabbe")]
    [SerializeField] private bool setLayerOnChildren = true;

    [Header("Einstellungen")]
    [Tooltip("Sollen alle Spawnpunkte verwendet werden?")]
    [SerializeField] private bool spawnAtAllPoints = true;

    [Tooltip("Wird verwendet, wenn nicht alle Spawnpunkte benutzt werden sollen")]
    [SerializeField] private int amountToSpawn = 3;

    private int currentAmount;
    private bool hasSpawned;

    private void OnEnable()
    {
        MissionManager.OnTargetCollected += HandleTargetCollected;
    }

    private void OnDisable()
    {
        MissionManager.OnTargetCollected -= HandleTargetCollected;
    }

    private void HandleTargetCollected(string targetId)
    {
        if (hasSpawned)
            return;

        if (targetId != requiredTargetId)
            return;

        currentAmount++;

        Debug.Log(
            $"MissionPreySpawner: {requiredTargetId} " +
            $"{currentAmount}/{requiredAmount}"
        );

        if (currentAmount >= requiredAmount)
        {
            SpawnPrey();
        }
    }

    private void SpawnPrey()
    {
        if (hasSpawned)
            return;

        hasSpawned = true;

        if (preyPrefab == null)
        {
            Debug.LogError(
                $"{name}: Es wurde kein Beute-Prefab zugewiesen."
            );

            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError(
                $"{name}: Es wurden keine Spawnpunkte zugewiesen."
            );

            return;
        }

        int spawnCount = spawnAtAllPoints
            ? spawnPoints.Length
            : Mathf.Min(amountToSpawn, spawnPoints.Length);

        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawnPoint = spawnPoints[i];

            if (spawnPoint == null)
                continue;

            GameObject spawnedPrey = Instantiate(
                preyPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            spawnedPrey.name =
                preyPrefab.name + "_MissionSpawned_" + (i + 1);

            SetPreyLayer(spawnedPrey);
        }

        Debug.Log(
            $"{spawnCount} lila Krabben wurden gespawnt."
        );
    }

    private void SetPreyLayer(GameObject spawnedPrey)
    {
        int preyLayer = LayerMask.NameToLayer(preyLayerName);

        if (preyLayer == -1)
        {
            Debug.LogError(
                $"Der Layer '{preyLayerName}' wurde nicht gefunden."
            );

            return;
        }

        spawnedPrey.layer = preyLayer;

        if (!setLayerOnChildren)
            return;

        Transform[] children =
            spawnedPrey.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            child.gameObject.layer = preyLayer;
        }
    }
}