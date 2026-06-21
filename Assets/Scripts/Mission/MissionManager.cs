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
        public string missionText = "Fange Beute";
        public string requiredTargetId = "Fly";
        public int requiredAmount = 2;
    }

    [Header("UI References")]
    [SerializeField] private CanvasGroup missionCanvasGroup; // NEU: Steuert die Transparenz
    [SerializeField] private RectTransform missionWindowRect; // NEU: Das Feld für das Aufklappen
    [SerializeField] private TMP_Text missionText;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.3f; // NEU: Dauer des Aufklappens

    [Header("Missions")]
    [SerializeField] private MissionStep[] missions;

    [Header("Input")]
    [SerializeField] private Key keyToHold = Key.Z;

    private int currentMissionIndex;
    private int currentAmount;
    
    private Coroutine currentAnimationRoutine; // NEU: Hält die laufende Animation
    private bool isCurrentlyOpen = false; // NEU: Zustandstracker

    private void Awake()
    {
        // Initialisierung: Fenster unsichtbar machen und zusammenschieben
        if (missionCanvasGroup != null)
        {
            missionCanvasGroup.alpha = 0f;
            missionCanvasGroup.gameObject.SetActive(false);
        }

        if (missionWindowRect != null)
        {
            Vector3 scale = missionWindowRect.localScale;
            scale.x = 0f;
            missionWindowRect.localScale = scale;
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

        bool isHoldingKey = Keyboard.current[keyToHold].isPressed;

        // Wenn die Taste gedrückt wird und das Fenster noch zu ist -> Öffnen
        if (isHoldingKey && !isCurrentlyOpen)
        {
            ToggleMissionWindow(true);
        }
        // Wenn die Taste losgelassen wird und das Fenster noch offen ist -> Schließen
        else if (!isHoldingKey && isCurrentlyOpen)
        {
            ToggleMissionWindow(false);
        }
    }

    private void ToggleMissionWindow(bool open)
    {
        isCurrentlyOpen = open;

        if (currentAnimationRoutine != null)
        {
            StopCoroutine(currentAnimationRoutine);
        }

        currentAnimationRoutine = StartCoroutine(AnimateWindow(open ? 1f : 0f, open ? 1f : 0f));
    }

    // Die Animations-Coroutine für flüssiges Aufklappen
    private IEnumerator AnimateWindow(float targetAlpha, float targetScaleX)
    {
        if (missionCanvasGroup == null || missionWindowRect == null) yield break;

        // Falls wir öffnen, aktivieren wir das GameObject
        if (targetAlpha > 0f)
        {
            missionCanvasGroup.gameObject.SetActive(true);
        }

        float startAlpha = missionCanvasGroup.alpha;
        float startScaleX = missionWindowRect.localScale.x;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            missionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            Vector3 currentScale = missionWindowRect.localScale;
            currentScale.x = Mathf.Lerp(startScaleX, targetScaleX, progress);
            missionWindowRect.localScale = currentScale;

            yield return null;
        }

        missionCanvasGroup.alpha = targetAlpha;
        Vector3 finalScale = missionWindowRect.localScale;
        finalScale.x = targetScaleX;
        missionWindowRect.localScale = finalScale;

        // Falls wir schließen, deaktivieren wir das GameObject am Ende
        if (targetAlpha <= 0f)
        {
            missionCanvasGroup.gameObject.SetActive(false);
        }

        currentAnimationRoutine = null;
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