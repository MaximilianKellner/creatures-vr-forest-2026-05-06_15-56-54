using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private TMP_Text tutorialText;
    
    // NEU: Zuweisung des UI-Fensters, um die Skalierung (Breite) zu verändern
    [SerializeField] private RectTransform tutorialWindowRect;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.4f;

    private Coroutine currentTutorialRoutine;

    private void Awake()
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.gameObject.SetActive(false);
        }

        // NEU: Zu Beginn das Fenster komplett von rechts nach links zusammenschieben
        if (tutorialWindowRect != null)
        {
            Vector3 scale = tutorialWindowRect.localScale;
            scale.x = 0f;
            tutorialWindowRect.localScale = scale;
        }
    }

    public void ShowTutorial(string message)
    {
        if (currentTutorialRoutine != null)
        {
            StopCoroutine(currentTutorialRoutine);
        }

        currentTutorialRoutine = StartCoroutine(
            ShowTutorialWithoutAutoHide(message)
        );
    }

    public void ShowTutorial(string message, float duration)
    {
        if (currentTutorialRoutine != null)
        {
            StopCoroutine(currentTutorialRoutine);
        }

        currentTutorialRoutine = StartCoroutine(
            ShowTutorialWithAutoHide(message, duration)
        );
    }

    public void HideTutorial()
    {
        if (currentTutorialRoutine != null)
        {
            StopCoroutine(currentTutorialRoutine);
        }

        currentTutorialRoutine = StartCoroutine(HideRoutine());
    }

    private IEnumerator ShowTutorialWithoutAutoHide(string message)
    {
        if (tutorialCanvasGroup == null || tutorialText == null)
            yield break;

        tutorialCanvasGroup.gameObject.SetActive(true);
        tutorialText.text = message;

        // NEU: Faded auf Alpha 1 und klappt die Skalierung auf X = 1 auf
        yield return AnimateWindow(1f, 1f);

        currentTutorialRoutine = null;
    }

    private IEnumerator ShowTutorialWithAutoHide(string message, float duration)
    {
        if (tutorialCanvasGroup == null || tutorialText == null)
            yield break;

        tutorialCanvasGroup.gameObject.SetActive(true);
        tutorialText.text = message;

        // NEU: Aufklappen und Einblenden
        yield return AnimateWindow(1f, 1f);

        yield return new WaitForSeconds(duration);

        // NEU: Zuklappen und Ausblenden
        yield return AnimateWindow(0f, 0f);

        tutorialCanvasGroup.gameObject.SetActive(false);
        currentTutorialRoutine = null;
    }

    private IEnumerator HideRoutine()
    {
        if (tutorialCanvasGroup == null)
            yield break;

        // NEU: Zuklappen und Ausblenden
        yield return AnimateWindow(0f, 0f);

        tutorialCanvasGroup.gameObject.SetActive(false);
        currentTutorialRoutine = null;
    }

    // NEU: Diese Methode ersetzt das alte FadeTo und steuert Alpha + Scale gleichzeitig
    private IEnumerator AnimateWindow(float targetAlpha, float targetScaleX)
    {
        float startAlpha = tutorialCanvasGroup.alpha;
        float startScaleX = tutorialWindowRect != null ? tutorialWindowRect.localScale.x : 0f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Transparenz faden
            tutorialCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            // X-Skalierung verändern
            if (tutorialWindowRect != null)
            {
                Vector3 currentScale = tutorialWindowRect.localScale;
                currentScale.x = Mathf.Lerp(startScaleX, targetScaleX, progress);
                tutorialWindowRect.localScale = currentScale;
            }

            yield return null;
        }

        // Exakte Endwerte setzen
        tutorialCanvasGroup.alpha = targetAlpha;
        if (tutorialWindowRect != null)
        {
            Vector3 finalScale = tutorialWindowRect.localScale;
            finalScale.x = targetScaleX;
            tutorialWindowRect.localScale = finalScale;
        }
    }
}