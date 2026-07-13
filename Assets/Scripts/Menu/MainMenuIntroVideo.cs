using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenuIntroVideo : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] private GameObject menuUI;

    [Header("Intro Video")]
    [SerializeField] private GameObject introVideoRawImage;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "cave-scene";

    private bool isPlaying;

    private void Start()
    {
        if (introVideoRawImage != null)
            introVideoRawImage.SetActive(false);

        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    public void PlayIntro()
    {
        if (isPlaying) return;
        isPlaying = true;

        if (menuUI != null)
            menuUI.SetActive(false);

        if (introVideoRawImage != null)
            introVideoRawImage.SetActive(true);

        videoPlayer.Stop();
        videoPlayer.Play();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }
}