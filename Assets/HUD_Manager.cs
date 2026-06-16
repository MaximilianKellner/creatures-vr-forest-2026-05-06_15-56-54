using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

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
    public TextMeshProUGUI notificationText;

    private bool nachtsichtIstAktiv = false;

    void Start()
{
    // Das sorgt dafür, dass der Button beim Start rigoros deaktiviert wird
    if (nightVisionButtonObject != null)
    {
        nightVisionButtonObject.SetActive(false);
    }
    
    if (notificationText != null)
    {
        notificationText.gameObject.SetActive(false);
    }
}

    // Die Update-Methode bleibt leer, damit das alte Input-System keine Fehler wirft!
    void Update()
    {
        // Hier drin fragen wir nichts mehr ab
    }

    // DIESE Funktion ist die perfekte Schnittstelle für deine Kollegen und das VR-System!
    public void TriggerNachtsichtLogik()
    {
        // Wir zwingen den Button aktiv zu werden, falls er noch unsichtbar war
        nightVisionButtonObject.SetActive(true); 

        nachtsichtIstAktiv = !nachtsichtIstAktiv;

        if (nachtsichtIstAktiv)
        {
            AktiviereSinn("Nachtsicht");
            Debug.Log("Nachtsicht-Effekt eingeschaltet!");
        }
        else
        {
            nightVisionOutline.enabled = false;
            Debug.Log("Nachtsicht-Effekt ausgeschaltet!");
        }
    }

    public void SchalteNachtsichtFrei()
    {
        nightVisionButtonObject.SetActive(true);
        StartCoroutine(ZeigeNotificationFuerZeit());
    }

    private IEnumerator ZeigeNotificationFuerZeit()
    {
        notificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2.0f);
        notificationText.gameObject.SetActive(false);
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