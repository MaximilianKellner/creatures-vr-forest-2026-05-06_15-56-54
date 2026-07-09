using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private TMP_Text tutorialText;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("UI")]
    [SerializeField] private UpgradeNotificationUI upgradeNotificationUI;
    [SerializeField] private TutorialManager tutorialManager;

    private Coroutine currentTutorialRoutine;

    private void Awake()
    {
        XRVisualRuntimeAdapter.EnsureSceneVisuals();

        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.gameObject.SetActive(false);
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

        yield return FadeTo(1f);

        currentTutorialRoutine = null;
    }

    private IEnumerator ShowTutorialWithAutoHide(string message, float duration)
    {
        if (tutorialCanvasGroup == null || tutorialText == null)
            yield break;

        tutorialCanvasGroup.gameObject.SetActive(true);
        tutorialText.text = message;

        yield return FadeTo(1f);

        yield return new WaitForSeconds(duration);

        yield return FadeTo(0f);

        tutorialCanvasGroup.gameObject.SetActive(false);
        currentTutorialRoutine = null;
    }

    private IEnumerator HideRoutine()
    {
        if (tutorialCanvasGroup == null)
            yield break;

        yield return FadeTo(0f);

        tutorialCanvasGroup.gameObject.SetActive(false);
        currentTutorialRoutine = null;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = tutorialCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            tutorialCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                timer / fadeDuration
            );

            yield return null;
        }

        tutorialCanvasGroup.alpha = targetAlpha;
    }
}
