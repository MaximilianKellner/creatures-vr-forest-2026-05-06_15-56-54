using UnityEngine;

public class HidePlayerHUDForMenu : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private CanvasGroup playerHUD;

    [Header("Menus")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject creditsScreen;
    [SerializeField] private GameObject leaveCaveScreen;

    private void Update()
    {
        if (playerHUD == null)
            return;

        bool menuOpen =
            IsOpen(pauseMenu) ||
            IsOpen(gameOverScreen) ||
            IsOpen(winScreen) ||
            IsOpen(creditsScreen) ||
            IsOpen(leaveCaveScreen);

        playerHUD.alpha = menuOpen ? 0f : 1f;
        playerHUD.blocksRaycasts = !menuOpen;
        playerHUD.interactable = !menuOpen;
    }

    private bool IsOpen(GameObject target)
    {
        return target != null && target.activeInHierarchy;
    }
}
