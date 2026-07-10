using System.Collections;
using UnityEngine;

public enum UIShowDirection
{
    FromBottom,
    FromTop,
    FromRight,
    FromLeft
}

[RequireComponent(typeof(CanvasGroup))]
public class UIWindowAnimator : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private UIShowDirection showDirection = UIShowDirection.FromBottom;
    [SerializeField] private float moveDistance = 120f;
    [SerializeField] private float duration = 0.35f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Scale")]
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private Vector3 hiddenScale = new Vector3(0.85f, 0.85f, 1f);

    [Header("Auto")]
    [SerializeField] private bool playOnEnable = true;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 shownPosition;
    private Vector2 hiddenPosition;
    private Vector3 shownScale;

    private Coroutine animationRoutine;
    private bool initialized;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();

        if (playOnEnable)
        {
            Show();
        }
    }

    private void Initialize()
    {
        if (initialized)
            return;

        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        shownPosition = rectTransform.anchoredPosition;
        shownScale = rectTransform.localScale;
        hiddenPosition = GetHiddenPosition();

        initialized = true;
    }

    private Vector2 GetHiddenPosition()
    {
        switch (showDirection)
        {
            case UIShowDirection.FromBottom:
                return shownPosition + Vector2.down * moveDistance;

            case UIShowDirection.FromTop:
                return shownPosition + Vector2.up * moveDistance;

            case UIShowDirection.FromRight:
                return shownPosition + Vector2.right * moveDistance;

            case UIShowDirection.FromLeft:
                return shownPosition + Vector2.left * moveDistance;
        }

        return shownPosition;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(AnimateWindow(
            hiddenPosition,
            shownPosition,
            0f,
            1f,
            useScaleAnimation ? hiddenScale : shownScale,
            shownScale
        ));
    }

    public void Hide()
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        animationRoutine = StartCoroutine(AnimateWindow(
            shownPosition,
            hiddenPosition,
            1f,
            0f,
            shownScale,
            useScaleAnimation ? hiddenScale : shownScale,
            true
        ));
    }

    public void HideInstant()
    {
        Initialize();

        rectTransform.anchoredPosition = hiddenPosition;
        rectTransform.localScale = useScaleAnimation ? hiddenScale : shownScale;
        canvasGroup.alpha = 0f;
    }

    private IEnumerator AnimateWindow(
        Vector2 startPos,
        Vector2 endPos,
        float startAlpha,
        float endAlpha,
        Vector3 startScale,
        Vector3 endScale,
        bool disableAfter = false)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float easedT = easeCurve.Evaluate(t);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, easedT);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, easedT);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, easedT);

            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
        rectTransform.localScale = endScale;
        canvasGroup.alpha = endAlpha;

        if (disableAfter)
            gameObject.SetActive(false);
    }
}