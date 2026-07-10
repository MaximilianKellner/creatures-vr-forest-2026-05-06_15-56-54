using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class WinZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;

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
        if (outroIsPlaying)
            return;

        if (leaveQuestionPanel != null)
            leaveQuestionPanel.SetActive(false);

        if (putPlayerHere != null && playerMovement != null)
        {
            CharacterController controller =
                playerMovement.GetComponent<CharacterController>();

            if (controller != null)
                controller.enabled = false;

            playerMovement.transform.SetPositionAndRotation(
                putPlayerHere.position,
                putPlayerHere.rotation
            );

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

    private void OnDestroy()
    {
        if (outroVideoPlayer != null)
            outroVideoPlayer.loopPointReached -= OnOutroFinished;
    }
}