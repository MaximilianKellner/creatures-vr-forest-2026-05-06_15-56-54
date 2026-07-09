using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthImage;

    [Header("Health Bar Visibility")]
    [SerializeField] private CanvasGroup healthCanvasGroup;
    [SerializeField] private float visibleTime = 3f;
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverScreen;

    private Coroutine visibilityRoutine;

    private void Start()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        if (healthCanvasGroup != null)
            healthCanvasGroup.alpha = 0f;

        if (playerHealth != null)
        {
            UpdateUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            playerHealth.OnHealthChanged += OnHealthChanged;
            playerHealth.OnDeath += ShowGameOverScreen;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        UpdateUI(current, max);
        ShowHealthBarTemporarily();
    }

    private void UpdateUI(int current, int max)
    {
        if (healthImage != null && max > 0)
            healthImage.fillAmount = (float)current / max;
    }

    private void ShowHealthBarTemporarily()
    {
        if (healthCanvasGroup == null)
            return;

        if (visibilityRoutine != null)
            StopCoroutine(visibilityRoutine);

        visibilityRoutine = StartCoroutine(HealthBarVisibilityRoutine());
    }

    private IEnumerator HealthBarVisibilityRoutine()
    {
        yield return StartCoroutine(FadeCanvasGroup(1f));

        yield return new WaitForSeconds(visibleTime);

        yield return StartCoroutine(FadeCanvasGroup(0f));
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha)
    {
        float startAlpha = healthCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            healthCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        healthCanvasGroup.alpha = targetAlpha;
    }

    private void ShowGameOverScreen()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= OnHealthChanged;
            playerHealth.OnDeath -= ShowGameOverScreen;
        }
    }
}