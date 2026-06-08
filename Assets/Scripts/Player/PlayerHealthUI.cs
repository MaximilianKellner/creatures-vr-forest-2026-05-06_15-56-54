using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthImage;

    [Header("Game Over Settings")]
    [SerializeField] private GameObject gameOverScreen;

    private void Start()
    {
        // Am Anfang des Spiels stellen wir sicher, dass der Game Over Screen unsichtbar ist
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }

        if (playerHealth != null)
        {
            // Initialisiere Lebensbalken
            UpdateUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);

            // Abonnieren der Events
            playerHealth.OnHealthChanged += UpdateUI;
            playerHealth.OnDeath += ShowGameOverScreen;
        }
    }

    private void UpdateUI(int current, int max)
    {
        if (healthImage != null && max > 0)
        {
            healthImage.fillAmount = (float)current / max;
        }
    }

    private void ShowGameOverScreen()
    {
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
            
            // Macht die Maus wieder frei und sichtbar
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateUI;
            playerHealth.OnDeath -= ShowGameOverScreen;
        }
    }

    
}