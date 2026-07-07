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
        if (string.IsNullOrWhiteSpace(targetId)) return;
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
    [SerializeField] private TMP_Text missionText;
    [SerializeField] private CanvasGroup missionCanvasGroup;
    [SerializeField] private RectTransform missionWindowRect;

    [Header("Missions")]
    [SerializeField] private MissionStep[] missions;

    [Header("Input")]
    [SerializeField] private Key keyToHold = Key.Z;

    [Header("Animation Settings")]
    [SerializeField] private float autoShowDuration = 3f;
    [SerializeField] private float fadeDuration = 0.4f;

    private int currentMissionIndex;
    private int currentAmount;
    private Coroutine autoShowRoutine;
    private bool autoShowing;
    private float currentAnimProgress = 0f; 

    private void Awake()
    {
        // AUTOMATIK-CHECK: Falls du vergessen hast, sie im Inspector reinzuziehen
        if (missionCanvasGroup == null) missionCanvasGroup = GetComponentInChildren<CanvasGroup>();
        if (missionWindowRect == null) 
        {
            // Sucht nach dem "Background" Objekt in den Kindern
            Transform bg = transform.Find("Background");
            if(bg != null) missionWindowRect = bg.GetComponent<RectTransform>();
        }

        // Startzustand
        if (missionCanvasGroup != null) missionCanvasGroup.alpha = 0f;
        if (missionWindowRect != null)
        {
            Vector3 scale = missionWindowRect.localScale;
            scale.x = 0f;
            missionWindowRect.localScale = scale;
        }

        UpdateMissionUI();
    }

    private void OnEnable() => OnTargetCollected += HandleTargetCollected;
    private void OnDisable() => OnTargetCollected -= HandleTargetCollected;

    private void Update()
    {
        if (Keyboard.current == null) return;

        bool isHoldingKey = Keyboard.current[keyToHold].isPressed;
        bool shouldShow = isHoldingKey || autoShowing;

        if (shouldShow) currentAnimProgress += Time.deltaTime / fadeDuration;
        else currentAnimProgress -= Time.deltaTime / fadeDuration;

        currentAnimProgress = Mathf.Clamp01(currentAnimProgress);
        ApplyAnimation(currentAnimProgress);
    }

    private void ApplyAnimation(float progress)
    {
        if (missionCanvasGroup != null) missionCanvasGroup.alpha = progress;
        if (missionWindowRect != null)
        {
            Vector3 scale = missionWindowRect.localScale;
            scale.x = progress;
            missionWindowRect.localScale = scale;
        }
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
        ShowMissionTemporarily();
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
            missionText.text = "Verlasse die Höhle!";
            return;
        }

        MissionStep currentMission = missions[currentMissionIndex];

        missionText.text =
            currentMission.missionText + "\n" +
            currentAmount + " / " + currentMission.requiredAmount;
    }

    private void ShowMissionTemporarily()
    {
        if (autoShowRoutine != null)
            StopCoroutine(autoShowRoutine);

        autoShowRoutine = StartCoroutine(AutoShowRoutine());
    }

    private IEnumerator AutoShowRoutine()
    {
        autoShowing = true;
        // Die Update-Schleife kümmert sich jetzt automatisch um das Reinwischen!

        yield return new WaitForSeconds(autoShowDuration);

        autoShowing = false;
        // Die Update-Schleife kümmert sich jetzt automatisch um das Rauswischen!
        
        autoShowRoutine = null;
    }
}