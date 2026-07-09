using UnityEngine;
using UnityEngine.Rendering;

public class IntroStartLock : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerControlAdapter playerControl;
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

    private bool introUnlocked;

    private void Start()
    {
        XRVisualRuntimeAdapter.EnsureSceneVisuals();
        ResolvePlayerControl();

        if (playerControl == null && playerMovement == null)
        {
            Debug.LogError("IntroStartLock: Player control fehlt.");
            return;
        }

        SetMovementEnabled(false);
        SetLookEnabled(true);

        if (blurVolume != null)
        {
            blurVolume.weight = 1f;
        }

        if (tutorialManager != null)
        {
            tutorialManager.ShowTutorial(introTutorialText);
        }
    }

    private void Update()
    {
        if (introUnlocked) return;
        if (upgradeSystem == null) return;

        if (upgradeSystem.HasUpgrade(requiredUpgrade))
        {
            UnlockPlayer();
        }
    }

    private void UnlockPlayer()
    {
        introUnlocked = true;

        SetMovementEnabled(true);
        SetLookEnabled(true);

        if (blurVolume != null)
        {
            blurVolume.weight = 0f;
        }

        if (tutorialManager != null)
        {
            tutorialManager.HideTutorial();

            tutorialManager.ShowTutorial(
                afterVisionTutorialText,
                afterVisionDuration
            );
        }

        Debug.Log("Intro beendet.");
    }

    private void ResolvePlayerControl()
    {
        if (playerControl != null)
            return;

        if (upgradeSystem != null)
        {
            playerControl =
                upgradeSystem.GetComponentInParent<PlayerControlAdapter>() ??
                upgradeSystem.GetComponentInChildren<PlayerControlAdapter>();

            if (playerControl == null)
                playerControl = upgradeSystem.gameObject.AddComponent<PlayerControlAdapter>();
        }
    }

    private void SetMovementEnabled(bool enabled)
    {
        if (playerControl != null)
            playerControl.SetMovementEnabled(enabled);
        else if (playerMovement != null)
            playerMovement.SetMovementEnabled(enabled);
    }

    private void SetLookEnabled(bool enabled)
    {
        if (playerControl != null)
            playerControl.SetLookEnabled(enabled);
        else if (playerMovement != null)
            playerMovement.SetLookEnabled(enabled);
    }
}
