using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("Menüs")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject belegUebersichtMenu;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseToggleAction;
    [SerializeField] private Key keyboardPauseKey = Key.M;

    private InputAction fallbackPauseToggleAction;
    private bool isPaused;

    private void Awake()
    {
        CreateFallbackPauseToggleAction();
    }

    private void OnEnable()
    {
        EnableAction(pauseToggleAction);
        fallbackPauseToggleAction?.Enable();
    }

    private void OnDisable()
    {
        fallbackPauseToggleAction?.Disable();
    }

    private void Update()
    {
        if (WasPauseTogglePressed())
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        if (belegUebersichtMenu != null)
            belegUebersichtMenu.SetActive(false);

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (belegUebersichtMenu != null)
            belegUebersichtMenu.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenKeyBindings()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        if (belegUebersichtMenu != null)
            belegUebersichtMenu.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        if (belegUebersichtMenu != null)
            belegUebersichtMenu.SetActive(false);

        if (pauseMenu != null)
            pauseMenu.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private bool WasPauseTogglePressed()
    {
        if (pauseToggleAction != null &&
            pauseToggleAction.action != null &&
            pauseToggleAction.action.WasPressedThisFrame())
            return true;

        if (fallbackPauseToggleAction != null &&
            fallbackPauseToggleAction.WasPressedThisFrame())
            return true;

        return Keyboard.current != null &&
               Keyboard.current[keyboardPauseKey].wasPressedThisFrame;
    }

    private void CreateFallbackPauseToggleAction()
    {
        fallbackPauseToggleAction = new InputAction(
            "Pause Toggle Fallback",
            InputActionType.Button);

        fallbackPauseToggleAction.AddBinding("<XRController>{LeftHand}/{MenuButton}");
        fallbackPauseToggleAction.AddBinding("<Keyboard>/m");
    }

    private static void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
            actionReference.action.Enable();
    }
}
