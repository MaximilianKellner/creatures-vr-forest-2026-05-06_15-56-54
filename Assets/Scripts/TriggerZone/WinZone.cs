using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerControlAdapter playerControl;

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
    private Transform playerTransform;
    private CharacterController playerController;

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

        ResolvePlayerReferences(other);

        if (leaveQuestionPanel != null)
            leaveQuestionPanel.SetActive(true);

        if (playerControl != null)
        {
            playerControl.SetMovementEnabled(false);
            playerControl.SetLookEnabled(false);
        }
        else if (playerMovement != null)
        {
            playerMovement.SetMovementEnabled(false);
            playerMovement.SetLookEnabled(false);
        }
        else
        {
            Debug.LogWarning("Keine Player-Steuerung gefunden!");
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ContinueExploring()
    {
        if (leaveQuestionPanel != null)
            leaveQuestionPanel.SetActive(false);

        if (putPlayerHere != null && playerTransform != null)
        {
            CharacterController controller = playerController;

            if (controller != null)
                controller.enabled = false;

            playerTransform.position = putPlayerHere.position;
            playerTransform.rotation = putPlayerHere.rotation;

            if (controller != null)
                controller.enabled = true;
        }

        if (playerControl != null)
        {
            playerControl.SetMovementEnabled(true);
            playerControl.SetLookEnabled(true);
        }
        else if (playerMovement != null)
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

    private void ResolvePlayerReferences(Collider other)
    {
        playerControl =
            other.GetComponent<PlayerControlAdapter>() ??
            other.GetComponentInParent<PlayerControlAdapter>() ??
            other.GetComponentInChildren<PlayerControlAdapter>();

        UpgradeSystem upgradeSystem =
            other.GetComponent<UpgradeSystem>() ??
            other.GetComponentInParent<UpgradeSystem>() ??
            other.GetComponentInChildren<UpgradeSystem>();

        if (playerControl == null && upgradeSystem != null)
            playerControl = upgradeSystem.gameObject.AddComponent<PlayerControlAdapter>();

        playerMovement =
            other.GetComponent<PlayerMovement>() ??
            other.GetComponentInParent<PlayerMovement>() ??
            other.GetComponentInChildren<PlayerMovement>();

        playerController =
            other.GetComponent<CharacterController>() ??
            other.GetComponentInParent<CharacterController>() ??
            other.GetComponentInChildren<CharacterController>();

        if (playerControl != null)
            playerTransform = playerControl.PlayerTransform;
        else if (playerMovement != null)
            playerTransform = playerMovement.transform;
        else if (playerController != null)
            playerTransform = playerController.transform;
        else
            playerTransform = other.transform.root;
    }
}
