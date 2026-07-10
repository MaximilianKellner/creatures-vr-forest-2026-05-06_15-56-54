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
        bool menuOpen =
            pauseMenu.activeSelf ||
            gameOverScreen.activeSelf ||
            winScreen.activeSelf ||
            creditsScreen.activeSelf ||
            leaveCaveScreen.activeSelf;

        playerHUD.alpha = menuOpen ? 0f : 1f;
        playerHUD.blocksRaycasts = !menuOpen;
        playerHUD.interactable = !menuOpen;
    }
}