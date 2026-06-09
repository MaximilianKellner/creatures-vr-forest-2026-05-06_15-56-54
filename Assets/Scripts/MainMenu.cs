using UnityEngine;
using UnityEngine.SceneManagement; // Wichtig, um Szenen laden zu können!

public class MainMenu : MonoBehaviour
{
    // Diese Funktion wird aufgerufen, wenn wir auf "Play" klicken
    public void PlayGame()
    {
        // Lädt die nächste Szene (die Spiel-Szene). 
        // Die "+ 1" bedeutet, er nimmt die Szene, die in den Build Settings als nächstes kommt.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Diese Funktion wird aufgerufen, wenn wir auf "Exit" klicken
    public void QuitGame()
    {
        Debug.Log("Spiel wurde beendet!"); // Das zeigt uns im Editor an, dass der Knopf funktioniert
        Application.Quit(); // Schließt das Spiel (funktioniert nur im fertigen Build, nicht im Editor)
    }
}