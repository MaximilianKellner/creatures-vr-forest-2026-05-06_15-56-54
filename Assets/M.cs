using UnityEngine;
using UnityEngine.SceneManagement; 

public class VRMenuManager : MonoBehaviour
{
    // Diese Funktion rufen wir gleich mit deinem StartGameButton auf
    public void SpielStarten()
    {
        Debug.Log("Start-Button wurde gedrückt!");
        
        // Variante A: Das Menü verschwindet einfach und der Spieler kann loslaufen
        gameObject.SetActive(false); 
        
        // Variante B: Ein komplett neues Level wird geladen (dafür die // vor der nächsten Zeile entfernen)
        // SceneManager.LoadScene("NameDerSzene"); 
    }
}