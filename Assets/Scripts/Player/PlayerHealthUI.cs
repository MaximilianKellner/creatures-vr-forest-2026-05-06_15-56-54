using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthImage;
    [SerializeField] private GameObject healthBarContainer; // Das gesamte UI-Element

    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverScreen;

    private Coroutine fadeCoroutine; 

    private void Start()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }

        // 1. Wir machen die Healthbar beim Start sofort sichtbar
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(true);
        }

        if (playerHealth != null)
        {
            if (healthImage != null && playerHealth.MaxHealth > 0)
            {
                healthImage.fillAmount = (float)playerHealth.CurrentHealth / playerHealth.MaxHealth;
            }

            // Events abonnieren (für Schaden/Heilung im laufenden Spiel)
            playerHealth.OnHealthChanged += HandleHealthChanged;
            playerHealth.OnDeath += ShowGameOverScreen;
        }

        // 2. BOMBENSICHER: Wir starten den 3-Sekunden-Timer DIREKT hier beim Spielstart!
        if (healthBarContainer != null)
        {
            fadeCoroutine = StartCoroutine(ZeigeHealthBarFuerZeit(3.0f));
        }
    }

    // Diese Funktion reagiert ab jetzt NUR noch auf echten Schaden oder Heilung im Spiel
    private void HandleHealthChanged(int current, int max)
    {
        if (healthImage != null && max > 0)
        {
            healthImage.fillAmount = (float)current / max;
        }

        if (healthBarContainer != null)
        {
            // Erst aktiv schalten, damit die Coroutine laufen darf
            healthBarContainer.SetActive(true); 

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine); // Laufenden Timer (z.B. den Start-Timer) abbrechen
            }

            // Bei Schaden/Heilung immer für 5 Sekunden einblenden
            fadeCoroutine = StartCoroutine(ZeigeHealthBarFuerZeit(5.0f));
        }
    }

    // Der flexible Timer (blendet nach X Sekunden aus)
    private IEnumerator ZeigeHealthBarFuerZeit(float sekunden)
    {
        yield return new WaitForSeconds(sekunden);
        if (healthBarContainer != null)
        {
            healthBarContainer.SetActive(false); // Ausblenden
        }
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
            playerHealth.OnHealthChanged -= HandleHealthChanged;
            playerHealth.OnDeath -= ShowGameOverScreen;
        }
    }
}