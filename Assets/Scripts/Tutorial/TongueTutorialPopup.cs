using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject tutorialImageObject;
    
    private Image uiImage; 

    [Header("Pulse Animation Settings")]
    [SerializeField] private float pulseSpeed = 4f;       
    [SerializeField] private float sizeDeviation = 0.15f;  
    [SerializeField] private float alphaDeviation = 0.25f; 

    private Vector3 originalScale;
    private Color originalColor;
    private bool isPulsing = false;

    private void Awake()
    {
        if (tutorialImageObject != null)
        {
            uiImage = tutorialImageObject.GetComponent<Image>();
            originalScale = tutorialImageObject.transform.localScale;
            
            if (uiImage != null)
                originalColor = uiImage.color;
        }
    }

    private void Start()
    {
        // Startet das Pulsieren sofort
        if (tutorialImageObject != null)
        {
            tutorialImageObject.SetActive(true);
            isPulsing = true;
        }
    }

    private void Update()
    {
        if (isPulsing && tutorialImageObject != null)
        {
            float wave = Mathf.Sin(Time.time * pulseSpeed);

            // 1. GRÖSSE
            float scaleFactor = 1f + (wave * sizeDeviation);
            tutorialImageObject.transform.localScale = originalScale * scaleFactor;

            // 2. HELLIGKEIT
            if (uiImage != null)
            {
                Color newColor = originalColor;
                newColor.a = Mathf.Clamp01(originalColor.a - (alphaDeviation * 0.5f) + (wave * alphaDeviation * 0.5f));
                uiImage.color = newColor;
            }
        }
    }

    // Diese Methode wird vom Zungen-Skript aufgerufen
    public void HideTutorial()
    {
        if (tutorialImageObject != null && isPulsing)
        {
            isPulsing = false;
            tutorialImageObject.SetActive(false);
            
            // Werte zurücksetzen
            tutorialImageObject.transform.localScale = originalScale;
            if (uiImage != null) uiImage.color = originalColor;
        }
    }
}