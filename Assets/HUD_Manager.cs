using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections; // Wichtig für den Timer (IEnumerator)

public class HUDManager : MonoBehaviour
{
    [Header("Quest UI")]
    public TextMeshProUGUI missionText;

    [Header("Fähigkeiten (Outlines)")]
    public Outline nightVisionOutline;
    public Outline sonarOutline;

    [Header("Fähigkeiten-Buttons (Sichtbarkeit)")]
    public GameObject nightVisionButtonObject; 

    [Header("Benachrichtigungen")]
    public TextMeshProUGUI notificationText; // Hier kommt dein neuer Text rein

    void Start()
    {
        nightVisionButtonObject.SetActive(false);
        notificationText.gameObject.SetActive(false); // Text ist am Anfang aus
    }

    // Diese Funktion wird aufgerufen, um die Nachtsicht freizuschalten
    public void SchalteNachtsichtFrei()
    {
        nightVisionButtonObject.SetActive(true);
        // Startet den 2-Sekunden-Timer für den Text
        StartCoroutine(ZeigeNotificationFuerZeit());
    }

    // Das ist der Timer (Coroutine)
    private IEnumerator ZeigeNotificationFuerZeit()
    {
        notificationText.gameObject.SetActive(true);  // Text einblenden
        yield return new WaitForSeconds(2.0f);        // Exakt 2 Sekunden warten
        notificationText.gameObject.SetActive(false); // Text wieder ausblenden
    }

    public void FledermausGejagt()
    {
        missionText.text = "- Jage eine Fledermaus (1/1)";
    }

    public void AktiviereSinn(string sinnName)
    {
        nightVisionOutline.enabled = false;
        sonarOutline.enabled = false;

        if (sinnName == "Nachtsicht")
        {
            nightVisionOutline.enabled = true;
        }
        else if (sinnName == "Sonar")
        {
            sonarOutline.enabled = true;
        }
    }
}