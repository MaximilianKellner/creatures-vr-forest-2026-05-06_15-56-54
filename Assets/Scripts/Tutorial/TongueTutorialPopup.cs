using System.Collections;
using UnityEngine;
using UnityEngine.UI; // WICHTIG: Für die Image-Komponente

public class TutorialPopup : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject tutorialImageObject;
    
    private Image uiImage; // Referenz für das Heller/Dunkler-Werden

    [Header("Settings")]
    [SerializeField] private float displayDuration = 7f; // Etwas länger, damit man das Pulsieren sieht

    [Header("Pulse Animation Settings")]
    [SerializeField] private float pulseSpeed = 4f;       // Wie schnell pulsiert es?
    [SerializeField] private float sizeDeviation = 0.15f;  // Wie viel größer/kleiner wird es? (0.15 = +/- 15%)
    [SerializeField] private float alphaDeviation = 0.25f; // Wie viel heller/dunkler wird es? (0.25 = +/- 25% Sichtbarkeit)

    private Vector3 originalScale;
    private Color originalColor;
    private bool isPulsing = false;

    private void Awake()
    {
        if (tutorialImageObject != null)
        {
            // Holt sich die Image-Komponente für die Farbe/Helligkeit
            uiImage = tutorialImageObject.GetComponent<Image>();
            originalScale = tutorialImageObject.transform.localScale;
            
            if (uiImage != null)
                originalColor = uiImage.color;
        }
    }

    private void Start()
    {
        if (tutorialImageObject != null)
        {
            StartCoroutine(ShowAndPulseRoutine());
        }
    }

    private void Update()
    {
        // Wenn das Pulsieren aktiv ist, verändere Größe und Helligkeit jede Frame
        if (isPulsing && tutorialImageObject != null)
        {
            // Mathf.Sin erzeugt eine perfekte Welle zwischen -1 und 1 basierend auf der Zeit
            float wave = Mathf.Sin(Time.time * pulseSpeed);

            // 1. GRÖSSE (Größer und kleiner)
            float scaleFactor = 1f + (wave * sizeDeviation);
            tutorialImageObject.transform.localScale = originalScale * scaleFactor;

            // 2. HELLIGKEIT / ALPHA (Heller und dunkler über die Durchsichtigkeit)
            if (uiImage != null)
            {
                Color newColor = originalColor;
                // Berechnet das neue Alpha (z.B. zwischen 0.55 und 1.05, gecampt auf max 1f)
                newColor.a = Mathf.Clamp01(originalColor.a - (alphaDeviation * 0.5f) + (wave * alphaDeviation * 0.5f));
                uiImage.color = newColor;
            }
        }
    }

    private IEnumerator ShowAndPulseRoutine()
    {
        tutorialImageObject.SetActive(true);
        isPulsing = true; // Aktiviert den Effekt in der Update-Methode

        // Warte die Zeit ab, in der das Tutorial angezeigt wird
        yield return new WaitForSeconds(displayDuration);

        isPulsing = false;
        tutorialImageObject.SetActive(false);
        
        // Werte wieder zurücksetzen für das nächste Mal
        tutorialImageObject.transform.localScale = originalScale;
        if (uiImage != null) uiImage.color = originalColor;
    }
}