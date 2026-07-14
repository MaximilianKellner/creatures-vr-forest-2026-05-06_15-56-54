using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private TMP_Text tutorialText;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine currentTutorialRoutine;

    private void Awake()
    {
        XRVisualRuntimeAdapter.EnsureSceneVisuals();
        ResolveReferences();

        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 0f;
            tutorialCanvasGroup.interactable = false;
            tutorialCanvasGroup.blocksRaycasts = false;
            tutorialCanvasGroup.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        ResolveReferences();
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
        ResolveReferences();

        if (tutorialCanvasGroup == null || tutorialText == null)
            yield break;

        tutorialCanvasGroup.gameObject.SetActive(true);
        tutorialText.text = message;

        yield return FadeTo(1f);

        currentTutorialRoutine = null;
    }

    private IEnumerator ShowTutorialWithAutoHide(string message, float duration)
    {
        ResolveReferences();

        if (tutorialCanvasGroup == null || tutorialText == null)
            yield break;

        tutorialCanvasGroup.gameObject.SetActive(true);
        tutorialText.text = message;

        yield return FadeTo(1f);

        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(duration);
        else
            yield return new WaitForSeconds(duration);

        yield return FadeTo(0f);

        tutorialCanvasGroup.gameObject.SetActive(false);
        currentTutorialRoutine = null;
    }

    private IEnumerator HideRoutine()
    {
        ResolveReferences();

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
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (timer < duration)
        {
            timer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            tutorialCanvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                timer / duration
            );

            yield return null;
        }

        tutorialCanvasGroup.alpha = targetAlpha;
    }

    private void ResolveReferences()
    {
        if (tutorialCanvasGroup == null)
        {
            tutorialCanvasGroup = GetComponent<CanvasGroup>() ??
                                  GetComponentInChildren<CanvasGroup>(true);
        }

        if (tutorialText == null)
        {
            tutorialText = GetComponentInChildren<TMP_Text>(true) ??
                           VRUIRuntimeSupport.FindTextByName("TutorialText") ??
                           VRUIRuntimeSupport.FindTextByName("InfoText");
        }

        if (tutorialText != null)
        {
            CanvasGroup textCanvasGroup = tutorialText.GetComponentInParent<CanvasGroup>(true);

            if (tutorialCanvasGroup == null ||
                !tutorialText.transform.IsChildOf(tutorialCanvasGroup.transform))
            {
                tutorialCanvasGroup = textCanvasGroup ?? tutorialCanvasGroup;
            }
        }

        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.interactable = false;
            tutorialCanvasGroup.blocksRaycasts = false;

            Canvas canvas = tutorialCanvasGroup.GetComponentInParent<Canvas>(true);
            if (canvas != null)
            {
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null && VRUIRuntimeSupport.IsLikelyVrScene())
                    scaler.matchWidthOrHeight = 0.5f;
            }
        }
    }
}
