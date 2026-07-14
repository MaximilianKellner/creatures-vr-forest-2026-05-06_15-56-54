using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float blurFadeDuration = 0.75f;

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
    private readonly List<UpgradeSystem> observedUpgradeSystems = new List<UpgradeSystem>();
    private Coroutine blurRoutine;

    private void OnEnable()
    {
        SubscribeUpgradeSystems();
    }

    private void OnDisable()
    {
        UnsubscribeUpgradeSystems();
    }

    private void Start()
    {
        XRVisualRuntimeAdapter.EnsureSceneVisuals();
        ResolveReferences();
        SubscribeUpgradeSystems();

        if (playerControl == null && playerMovement == null)
        {
            Debug.LogError("IntroStartLock: Player control fehlt.");
            return;
        }

        if (minimapUI != null)
            minimapUI.SetActive(false);

        SetMovementEnabled(false);
        SetLookEnabled(true);

        SetBlurWeight(1f);

        if (tutorialManager != null)
            tutorialManager.ShowTutorial(introTutorialText);

        if (tongueTutorialPopup != null)
            tongueTutorialPopup.SetActive(true);

        if (HasRequiredUpgrade())
            UnlockPlayer();
    }

    private void Update()
    {
        if (introUnlocked) return;
        if (HasRequiredUpgrade())
            UnlockPlayer();
    }

    private void HandleUpgradeUnlocked(PreyGivesUpgrade upgrade)
    {
        if (!introUnlocked && upgrade == requiredUpgrade)
            UnlockPlayer();
    }

    private void UnlockPlayer()
    {
        if (introUnlocked)
            return;

        introUnlocked = true;

        SetMovementEnabled(true);
        SetLookEnabled(true);

        FadeBlurTo(0f);

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

    private void ResolveReferences()
    {
        ResolveUpgradeSystem();
        ResolvePlayerControl();
        ResolveBlurVolume();

        if (tutorialManager == null)
            tutorialManager = FindAnyObjectByType<TutorialManager>();
    }

    private void ResolveUpgradeSystem()
    {
        if (upgradeSystem != null && VRUIRuntimeSupport.IsLikelyVrPlayer(upgradeSystem.transform))
            return;

        UpgradeSystem bestUpgradeSystem = VRUIRuntimeSupport.FindBestUpgradeSystem();

        if (bestUpgradeSystem != null)
            upgradeSystem = bestUpgradeSystem;
    }

    private void ResolvePlayerControl()
    {
        if (playerControl != null && VRUIRuntimeSupport.IsLikelyVrPlayer(playerControl.transform))
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

    private void ResolveBlurVolume()
    {
        if (blurVolume != null)
            return;

        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsInactive.Include);

        foreach (Volume volume in volumes)
        {
            if (volume != null &&
                volume.name.IndexOf("blur", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                blurVolume = volume;
                return;
            }
        }

        foreach (Volume volume in volumes)
        {
            if (HasDepthOfField(volume))
            {
                blurVolume = volume;
                return;
            }
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

    private bool HasRequiredUpgrade()
    {
        if (upgradeSystem != null && upgradeSystem.HasUpgrade(requiredUpgrade))
            return true;

        foreach (UpgradeSystem observedUpgradeSystem in observedUpgradeSystems)
        {
            if (observedUpgradeSystem != null && observedUpgradeSystem.HasUpgrade(requiredUpgrade))
            {
                upgradeSystem = observedUpgradeSystem;
                return true;
            }
        }

        UpgradeSystem bestUpgradeSystem = VRUIRuntimeSupport.FindBestUpgradeSystem();

        if (bestUpgradeSystem != null)
        {
            if (!observedUpgradeSystems.Contains(bestUpgradeSystem))
                SubscribeUpgradeSystem(bestUpgradeSystem);

            upgradeSystem = bestUpgradeSystem;
            return bestUpgradeSystem.HasUpgrade(requiredUpgrade);
        }

        return false;
    }

    private void SubscribeUpgradeSystems()
    {
        UpgradeSystem[] upgradeSystems =
            FindObjectsByType<UpgradeSystem>(FindObjectsInactive.Include);

        foreach (UpgradeSystem candidate in upgradeSystems)
        {
            if (candidate != null)
                SubscribeUpgradeSystem(candidate);
        }
    }

    private void SubscribeUpgradeSystem(UpgradeSystem candidate)
    {
        if (observedUpgradeSystems.Contains(candidate))
            return;

        candidate.OnUpgradeUnlocked += HandleUpgradeUnlocked;
        observedUpgradeSystems.Add(candidate);
    }

    private void UnsubscribeUpgradeSystems()
    {
        foreach (UpgradeSystem observedUpgradeSystem in observedUpgradeSystems)
        {
            if (observedUpgradeSystem != null)
                observedUpgradeSystem.OnUpgradeUnlocked -= HandleUpgradeUnlocked;
        }

        observedUpgradeSystems.Clear();
    }

    private void SetBlurWeight(float weight)
    {
        if (blurVolume == null)
            return;

        blurVolume.enabled = weight > 0.001f;
        blurVolume.weight = weight;
    }

    private void FadeBlurTo(float targetWeight)
    {
        if (blurVolume == null)
            return;

        if (blurRoutine != null)
            StopCoroutine(blurRoutine);

        blurRoutine = StartCoroutine(FadeBlurRoutine(targetWeight));
    }

    private IEnumerator FadeBlurRoutine(float targetWeight)
    {
        float startWeight = blurVolume.weight;
        float timer = 0f;
        float duration = Mathf.Max(0.01f, blurFadeDuration);

        blurVolume.enabled = true;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(timer / duration);
            SetBlurWeight(Mathf.Lerp(startWeight, targetWeight, t));
            yield return null;
        }

        SetBlurWeight(targetWeight);
        blurRoutine = null;
    }

    private bool HasDepthOfField(Volume volume)
    {
        if (volume == null || volume.sharedProfile == null)
            return false;

        foreach (VolumeComponent component in volume.sharedProfile.components)
        {
            if (component != null &&
                component.GetType().Name.IndexOf("DepthOfField", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }
}
