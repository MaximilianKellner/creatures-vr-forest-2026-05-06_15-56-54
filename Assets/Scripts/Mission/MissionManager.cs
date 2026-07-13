using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class MissionManager : MonoBehaviour
{
    public static event Action<string> OnTargetCollected;

    public static void ReportTargetCollected(string targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId))
            return;

        OnTargetCollected?.Invoke(targetId);
    }

    [System.Serializable]
    public class MissionStep
    {
        [Header("Mission")]
        public string missionText = "Fange Beute";
        public string requiredTargetId = "Fly";

        [Min(1)]
        public int requiredAmount = 2;

        [Header("Belohnung")]
        [Tooltip("Upgrade, das nach Abschluss dieser Mission freigeschaltet wird.")]
        public PreyGivesUpgrade rewardUpgrade = PreyGivesUpgrade.None;

        [Min(1)]
        [Tooltip("Level des Upgrades, das nach Abschluss vergeben wird.")]
        public int rewardUpgradeLevel = 1;
    }

    [Header("UI References")]
    [SerializeField] private GameObject missionUI;
    [SerializeField] private TMP_Text missionText;

    [Header("Upgrade System")]
    [SerializeField] private UpgradeSystem upgradeSystem;

    [Header("Missions")]
    [SerializeField] private MissionStep[] missions;

    [Header("Input")]
    [SerializeField] private Key keyToHold = Key.T;

    [Header("Auto Show")]
    [SerializeField] private float autoShowDuration = 3f;

    private int currentMissionIndex;
    private int currentAmount;

    private Coroutine autoShowRoutine;
    private bool autoShowing;

    private void Awake()
    {
        if (missionUI != null)
            missionUI.SetActive(false);

        if (upgradeSystem == null)
        {
            upgradeSystem = FindAnyObjectByType<UpgradeSystem>();
        }

        UpdateMissionUI();
    }

    private void OnEnable()
    {
        OnTargetCollected += HandleTargetCollected;
    }

    private void OnDisable()
    {
        OnTargetCollected -= HandleTargetCollected;
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        bool isHoldingKey =
            Keyboard.current[keyToHold].isPressed;

        if (missionUI != null)
        {
            missionUI.SetActive(
                isHoldingKey || autoShowing
            );
        }
    }

    private void HandleTargetCollected(string targetId)
    {
        if (missions == null || missions.Length == 0)
            return;

        if (currentMissionIndex >= missions.Length)
            return;

        MissionStep currentMission =
            missions[currentMissionIndex];

        if (targetId != currentMission.requiredTargetId)
            return;

        currentAmount++;

        Debug.Log(
            $"Missionsfortschritt: {currentMission.missionText} " +
            $"({currentAmount}/{currentMission.requiredAmount})"
        );

        if (currentAmount >= currentMission.requiredAmount)
        {
            CompleteCurrentMission();
        }

        UpdateMissionUI();
        ShowMissionTemporarily();
    }

    private void CompleteCurrentMission()
    {
        MissionStep completedMission =
            missions[currentMissionIndex];

        Debug.Log(
            $"Mission abgeschlossen: {completedMission.missionText}"
        );

        // Upgrade-Belohnung vergeben
        if (completedMission.rewardUpgrade != PreyGivesUpgrade.None)
        {
            if (upgradeSystem != null)
            {
                upgradeSystem.UnlockUpgrade(
                    completedMission.rewardUpgrade,
                    completedMission.rewardUpgradeLevel
                );

                Debug.Log(
                    $"Missionsbelohnung erhalten: " +
                    $"{completedMission.rewardUpgrade} " +
                    $"Level {completedMission.rewardUpgradeLevel}"
                );
            }
            else
            {
                Debug.LogWarning(
                    "MissionManager: Kein UpgradeSystem gefunden. " +
                    "Die Missionsbelohnung konnte nicht vergeben werden."
                );
            }
        }

        currentMissionIndex++;
        currentAmount = 0;
    }

    private void UpdateMissionUI()
    {
        if (missionText == null)
            return;

        if (missions == null || missions.Length == 0)
        {
            missionText.text =
                "Keine Missionen eingestellt.";

            return;
        }

        if (currentMissionIndex >= missions.Length)
        {
            missionText.text =
                "Verlasse die Höhle!";

            return;
        }

        MissionStep currentMission =
            missions[currentMissionIndex];

        missionText.text =
            currentMission.missionText + "\n" +
            currentAmount + " / " +
            currentMission.requiredAmount;
    }

    private void ShowMissionTemporarily()
    {
        if (autoShowRoutine != null)
        {
            StopCoroutine(autoShowRoutine);
        }

        autoShowRoutine =
            StartCoroutine(AutoShowRoutine());
    }

    private IEnumerator AutoShowRoutine()
    {
        autoShowing = true;

        if (missionUI != null)
            missionUI.SetActive(true);

        yield return new WaitForSeconds(
            autoShowDuration
        );

        autoShowing = false;

        if (missionUI != null)
            missionUI.SetActive(false);

        autoShowRoutine = null;
    }
}