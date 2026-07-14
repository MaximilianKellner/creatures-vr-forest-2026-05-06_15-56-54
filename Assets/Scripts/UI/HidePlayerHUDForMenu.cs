using UnityEngine;
using UnityEngine.InputSystem;

public class HidePlayerHUDForMenu : MonoBehaviour
{
    [Header("HUD")]
    [SerializeField] private CanvasGroup playerHUD;

    [Header("VR Toggle")]
    [SerializeField] private InputActionReference hudToggleAction;
    [SerializeField] private Key keyboardToggleKey = Key.U;
    [SerializeField] private bool startHudVisible = true;

    [Header("Menus")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject belegUebersichtMenu;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject creditsScreen;
    [SerializeField] private GameObject leaveCaveScreen;

    private InputAction fallbackHudToggleAction;
    private bool hudVisible;

    private void Awake()
    {
        hudVisible = startHudVisible;
        CreateFallbackHudToggleAction();
    }

    private void OnEnable()
    {
        EnableAction(hudToggleAction);
        fallbackHudToggleAction?.Enable();
    }

    private void OnDisable()
    {
        fallbackHudToggleAction?.Disable();
    }

    private void Update()
    {
        if (playerHUD == null)
            return;

        if (WasHudTogglePressed())
            hudVisible = !hudVisible;

        bool menuOpen =
            IsOpen(pauseMenu) ||
            IsOpen(belegUebersichtMenu) ||
            IsOpen(gameOverScreen) ||
            IsOpen(winScreen) ||
            IsOpen(creditsScreen) ||
            IsOpen(leaveCaveScreen);

        bool shouldShowHud = hudVisible && !menuOpen;

        playerHUD.alpha = shouldShowHud ? 1f : 0f;
        playerHUD.blocksRaycasts = shouldShowHud;
        playerHUD.interactable = shouldShowHud;
    }

    private bool IsOpen(GameObject target)
    {
        return target != null && target.activeInHierarchy;
    }

    private bool WasHudTogglePressed()
    {
        if (hudToggleAction != null &&
            hudToggleAction.action != null &&
            hudToggleAction.action.WasPressedThisFrame())
            return true;

        if (fallbackHudToggleAction != null &&
            fallbackHudToggleAction.WasPressedThisFrame())
            return true;

        return Keyboard.current != null &&
               Keyboard.current[keyboardToggleKey].wasPressedThisFrame;
    }

    private void CreateFallbackHudToggleAction()
    {
        fallbackHudToggleAction = new InputAction(
            "VR HUD Toggle Fallback",
            InputActionType.Button);

        fallbackHudToggleAction.AddBinding("<XRController>{LeftHand}/{GripButton}");
        fallbackHudToggleAction.AddBinding("<Keyboard>/u");
    }

    private static void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
            actionReference.action.Enable();
    }
}
