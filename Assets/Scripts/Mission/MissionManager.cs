using System;
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
        public string missionText = "Fange Beute";
        public string requiredTargetId = "Fly";
        public int requiredAmount = 2;
    }

    [Header("UI References")]
    [SerializeField] private GameObject missionUI;
    [SerializeField] private TMP_Text missionText;

    [Header("Missions")]
    [SerializeField] private MissionStep[] missions;

    [Header("Input")]
    [SerializeField] private Key keyToHold = Key.Z;

    private int currentMissionIndex;
    private int currentAmount;

    private void Awake()
    {
        if (missionUI != null)
            missionUI.SetActive(false);

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

        bool isHoldingKey = Keyboard.current[keyToHold].isPressed;

        if (missionUI != null)
            missionUI.SetActive(isHoldingKey);
    }

    private void HandleTargetCollected(string targetId)
    {
        if (missions == null || missions.Length == 0)
            return;

        if (currentMissionIndex >= missions.Length)
            return;

        MissionStep currentMission = missions[currentMissionIndex];

        if (targetId != currentMission.requiredTargetId)
            return;

        currentAmount++;

        if (currentAmount >= currentMission.requiredAmount)
        {
            currentMissionIndex++;
            currentAmount = 0;
        }

        UpdateMissionUI();
    }

    private void UpdateMissionUI()
    {
        if (missionText == null)
            return;

        if (missions == null || missions.Length == 0)
        {
            missionText.text = "Keine Missionen eingestellt.";
            return;
        }

        if (currentMissionIndex >= missions.Length)
        {
            missionText.text = "Alle Missionen abgeschlossen!";
            return;
        }

        MissionStep currentMission = missions[currentMissionIndex];

        missionText.text =
            currentMission.missionText + "\n" +
            currentAmount + " / " + currentMission.requiredAmount;
    }
}