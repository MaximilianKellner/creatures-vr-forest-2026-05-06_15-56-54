using UnityEngine;
using UnityEngine.SceneManagement; // Wichtig, damit wir Szenen neu laden können

public class GameManager : MonoBehaviour
{
    // Diese Funktion lädt die komplette Szene neu (alles auf null)
    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Diese Funktion setzt nur den Spieler zurück
    public void RespawnPlayer()
    {
        // Da du in VR arbeitest, müsste man hier später den VR-Spieler anpacken. 
        // Für den Moment geben wir nur einen Text aus, um den Button zu testen.
        Debug.Log("Spieler wurde respawnt!");
    }
}