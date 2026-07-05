using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

    [Header("UI Panels")]
    [SerializeField] private GameObject leaveQuestionPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Bei Nein")]
    [SerializeField] private Transform putPlayerHere;

    [Header("Hauptmenü")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [SerializeField] private TMP_Text timeText;

    private bool hasTriggered;

    private void Start()
    {
        if (leaveQuestionPanel != null)
            leaveQuestionPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        hasTriggered = true;

        playerMovement =
            other.GetComponent<PlayerMovement>() ??
            other.GetComponentInParent<PlayerMovement>() ??
            other.GetComponentInChildren<PlayerMovement>();

        if (leaveQuestionPanel != null)
            leaveQuestionPanel.SetActive(true);

        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
            playerMovement.SetLookEnabled(false);
        }
        else
        {
            Debug.LogWarning("Kein PlayerMovement gefunden!");
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ContinueExploring()
    {
        if (leaveQuestionPanel != null)
            leaveQuestionPanel.SetActive(false);

        if (putPlayerHere != null && playerMovement != null)
        {
            CharacterController controller = playerMovement.GetComponent<CharacterController>();

            if (controller != null)
                controller.enabled = false;

            playerMovement.transform.position = putPlayerHere.position;
            playerMovement.transform.rotation = putPlayerHere.rotation;

            if (controller != null)
                controller.enabled = true;
        }

        if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(true);
            playerMovement.SetLookEnabled(true);
        }

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        hasTriggered = false;
    }

    public void LeaveCave()
    {
        if (leaveQuestionPanel != null)
            leaveQuestionPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(true);

        // Steuerung bleibt deaktiviert
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameTimer.Instance.StopTimer();
        timeText.text = "Spielzeit: " + GameTimer.Instance.GetFormattedTime();
    }

    public void ShowCredits()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}