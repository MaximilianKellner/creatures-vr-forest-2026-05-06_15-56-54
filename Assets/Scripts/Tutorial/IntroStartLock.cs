using UnityEngine;
using UnityEngine.Rendering;

public class IntroStartLock : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private UpgradeSystem upgradeSystem;
    [SerializeField] private TutorialManager tutorialManager;

    [Header("Blur")]
    [SerializeField] private Volume blurVolume;

    [Header("Upgrade")]
    [SerializeField] private PreyGivesUpgrade requiredUpgrade = PreyGivesUpgrade.Vision;

    [Header("Tutorials")]
    [TextArea(3, 8)]
    [SerializeField] private string introTutorialText =
        "Du bist schwach. Finde Nahrung um deine Sehkraft wiederzuerlangen.";

    [TextArea(3, 8)]
    [SerializeField] private string afterVisionTutorialText =
        "Sehr gut! Du kannst wieder sehen. Bewege dich mit WASD und erkunde die Höhle.";

    [SerializeField] private float afterVisionDuration = 6f;

    [Header("Zunge Bild Tutorial")]
    [SerializeField] private GameObject tongueTutorialPopup;

    [Header("Intro UI")]
    [SerializeField] private GameObject minimapUI;

    private bool introUnlocked;

    private void Start()
    {
        if (playerMovement == null)
        {
            Debug.LogError("IntroStartLock: PlayerMovement fehlt.");
            return;
        }

        if (minimapUI != null)
            minimapUI.SetActive(false);

        playerMovement.SetMovementEnabled(false);
        playerMovement.SetLookEnabled(true);

        if (blurVolume != null)
            blurVolume.weight = 1f;

        if (tutorialManager != null)
            tutorialManager.ShowTutorial(introTutorialText);

        if (tongueTutorialPopup != null)
            tongueTutorialPopup.SetActive(true);
    }

    private void Update()
    {
        if (introUnlocked) return;
        if (upgradeSystem == null) return;

        if (upgradeSystem.HasUpgrade(requiredUpgrade))
            UnlockPlayer();
    }

    private void UnlockPlayer()
    {
        introUnlocked = true;

        playerMovement.SetMovementEnabled(true);
        playerMovement.SetLookEnabled(true);

        if (blurVolume != null)
            blurVolume.weight = 0f;

        if (tongueTutorialPopup != null)
            tongueTutorialPopup.SetActive(false);

        if (minimapUI != null)
            minimapUI.SetActive(true);

        if (tutorialManager != null)
        {
            tutorialManager.HideTutorial();
            tutorialManager.ShowTutorial(afterVisionTutorialText, afterVisionDuration);
        }

        Debug.Log("Intro beendet.");
    }
}