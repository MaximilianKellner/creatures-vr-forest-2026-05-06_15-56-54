using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class WinZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerControlAdapter playerControl;

    [Header("UI Panels")]
    [SerializeField] private GameObject leaveQuestionPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Outro Video")]
    [SerializeField] private GameObject outroVideoRawImage;
    [SerializeField] private VideoPlayer outroVideoPlayer;

    [Header("Bei Nein")]
    [SerializeField] private Transform putPlayerHere;

    [Header("Hauptmenü")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Spielzeit")]
    [SerializeField] private TMP_Text timeText;

    private bool hasTriggered;
    private Transform playerTransform;
    private CharacterController playerController;
    private bool outroIsPlaying;

    private void Start()
    {
        if (leaveQuestionPanel != null)
            leaveQuestionPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        if (outroVideoRawImage != null)
            outroVideoRawImage.SetActive(false);

        if (outroVideoPlayer != null)
        {
            outroVideoPlayer.playOnAwake = false;
            outroVideoPlayer.Stop();
            outroVideoPlayer.loopPointReached += OnOutroFinished;
        }
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
        if (outroIsPlaying)
            return;

        if (leaveQuestionPanel != null)
            leaveQuestionPanel.SetActive(false);

        if (putPlayerHere != null && playerTransform != null)
        {
            CharacterController controller = playerController;

            if (controller != null)
                controller.enabled = false;

            playerTransform.SetPositionAndRotation(
                putPlayerHere.position,
                putPlayerHere.rotation
            );

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
        if (outroIsPlaying)
            return;

        if (leaveQuestionPanel != null)
            leaveQuestionPanel.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(false);

        if (creditsPanel != null)
            creditsPanel.SetActive(false);

        if (GameTimer.Instance != null)
            GameTimer.Instance.StopTimer();

        PlayOutroVideo();
    }

    private void PlayOutroVideo()
    {
        if (outroVideoPlayer == null)
        {
            ShowWinScreen();
            return;
        }

        outroIsPlaying = true;

        // Spielsounds pausieren
        AudioListener.pause = true;

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (outroVideoRawImage != null)
            outroVideoRawImage.SetActive(true);

        outroVideoPlayer.Stop();
        outroVideoPlayer.time = 0;
        outroVideoPlayer.frame = 0;
        outroVideoPlayer.Play();
    }

    private void OnOutroFinished(VideoPlayer videoPlayer)
    {
        ShowWinScreen();
    }

    private void ShowWinScreen()
    {
        outroIsPlaying = false;
        AudioListener.pause = false;

        if (outroVideoPlayer != null)
            outroVideoPlayer.Stop();

        if (outroVideoRawImage != null)
            outroVideoRawImage.SetActive(false);

        if (winPanel != null)
            winPanel.SetActive(true);

        if (timeText != null && GameTimer.Instance != null)
        {
            timeText.text =
                "Spielzeit: " + GameTimer.Instance.GetFormattedTime();
        }

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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

    private void OnDestroy()
    {
        if (outroVideoPlayer != null)
            outroVideoPlayer.loopPointReached -= OnOutroFinished;
    }
}
