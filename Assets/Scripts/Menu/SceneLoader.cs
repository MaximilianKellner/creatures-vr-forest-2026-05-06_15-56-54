using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private MainMenuIntroVideo mainMenuIntroVideo;

    public void RestartGame()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MainMenu");
    }

    public void LoadSceneByName()
    {
        Time.timeScale = 1f;

        if (mainMenuIntroVideo != null)
        {
            mainMenuIntroVideo.PlayIntro();
        }
        else
        {
            SceneManager.LoadScene("cave-scene");
        }
    }
}