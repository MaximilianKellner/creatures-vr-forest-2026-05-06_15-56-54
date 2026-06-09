using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void RestartGame()
    {
        Time.timeScale = 1f;

        // Damit die Maus im Spiel verschwindet
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Lädt die aktuell aktive Szene einfach direkt neu
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        // Macht die Maus fürs Hauptmenü frei
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MainMenu"); 
    }

    public void LoadSceneByName()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("cave-scene");
    }
}