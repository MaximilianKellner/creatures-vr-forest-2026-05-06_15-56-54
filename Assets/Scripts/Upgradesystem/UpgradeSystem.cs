using System.Collections.Generic;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UpgradeNotificationUI upgradeNotificationUI;
    [SerializeField] private TutorialManager tutorialManager;

    [Header("Tutorial Text")]
    [SerializeField] private bool useVrTutorialText;
    [SerializeField] private bool autoDetectVrTutorialText = true;

    [Header("VR Runtime Adapters")]
    [SerializeField] private bool autoInstallVrAdapters = true;

    [Header("Health Upgrade")]
    [SerializeField] private int healthBonusPerLevel = 20;

    private Dictionary<PreyGivesUpgrade, int> upgrades = new Dictionary<PreyGivesUpgrade, int>();
    private HashSet<PreyGivesUpgrade> shownTutorials = new HashSet<PreyGivesUpgrade>();
    public event System.Action<PreyGivesUpgrade> OnUpgradeUnlocked;

    private void Awake()
    {
        ResolveUiReferences();

        if (autoInstallVrAdapters)
            InstallVrAdapters();
    }

    public void UnlockUpgrade(PreyGivesUpgrade upgrade, int level = 1)
    {
        if (upgrade == PreyGivesUpgrade.None)
            return;

        if (!upgrades.ContainsKey(upgrade))
        {
            upgrades.Add(upgrade, level);
        }
        else
        {
            upgrades[upgrade] = Mathf.Max(upgrades[upgrade], level);
        }

        Debug.Log("Upgrade freigeschaltet: " + upgrade + " Level " + level);
        OnUpgradeUnlocked?.Invoke(upgrade);

        ApplyImmediateUpgradeEffect(upgrade, GetUpgradeLevel(upgrade));

        if (upgradeNotificationUI != null)
        {
            upgradeNotificationUI.ShowUpgrade(upgrade, level);
        }

        if (tutorialManager != null && !shownTutorials.Contains(upgrade))
        {
            shownTutorials.Add(upgrade);
            ShowTutorialForUpgrade(upgrade);
        }
    }

    private void ShowTutorialForUpgrade(PreyGivesUpgrade upgrade)
    {
        ResolveUiReferences();

        bool showVrText = ShouldShowVrTutorialText();

        switch (upgrade)
        {
            case PreyGivesUpgrade.NightVision:
                tutorialManager.ShowTutorial(
                    showVrText
                        ? "Rechten Stick oder Touchpad nach oben bewegen, um die Nachtsicht zu aktivieren."
                        : "Druecke [N], um die Nachtsicht zu aktivieren.",
                    6f);
                break;

            case PreyGivesUpgrade.ScentSense:
                tutorialManager.ShowTutorial(
                    showVrText
                        ? "Geruchssinn freigeschaltet!\n\nRechten Stick oder Touchpad nach links bewegen, um ihn zu aktivieren.\n\nFarben:\n- Rot = Gefahr\n- Blau = Beute mit Upgrades\n- Lila = normale Beute"
                        : "Du hast den Geruchssinn freigeschaltet!\n\nDruecke [G], um ihn zu aktivieren.\n\nFarben:\n- Rot = Gefahr\n- Blau = Beute mit Upgrades\n- Lila = normale Beute",
                    10f);
                break;

            case PreyGivesUpgrade.SonarSense:
                tutorialManager.ShowTutorial(
                    showVrText
                        ? "Rechten Stick oder Touchpad nach rechts bewegen, um den Sonarsinn zu aktivieren."
                        : "Druecke [B], um den Sonarsinn zu aktivieren.",
                    6f);
                break;

            case PreyGivesUpgrade.HearingSense:
                tutorialManager.ShowTutorial(
                    showVrText
                        ? "Rechten Stick oder Touchpad nach unten bewegen, um den Hoersinn zu aktivieren."
                        : "Druecke [H], um den Hoersinn zu aktivieren.",
                    6f);
                break;

            case PreyGivesUpgrade.Sprint:
                tutorialManager.ShowTutorial(
                    showVrText
                        ? "Sprint freigeschaltet."
                        : "Druecke [Shift], um zu sprinten.",
                    6f);
                break;
        }
    }

    private bool ShouldShowVrTutorialText()
    {
        if (useVrTutorialText)
            return true;

        if (!autoDetectVrTutorialText)
            return false;

        return GetComponentInParent<VRAbilityProvider>() != null ||
               GetComponentInChildren<VRAbilityProvider>(true) != null;
    }

    private void ApplyImmediateUpgradeEffect(PreyGivesUpgrade upgrade, int level)
    {
        if (upgrade != PreyGivesUpgrade.MoreHealth)
            return;

        PlayerHealth health =
            GetComponentInParent<PlayerHealth>() ??
            GetComponentInChildren<PlayerHealth>();

        if (health != null)
            health.ApplyMaxHealthBonus(level * healthBonusPerLevel);
    }

    private void ResolveUiReferences()
    {
        if (upgradeNotificationUI == null)
        {
            upgradeNotificationUI =
                FindBestSceneComponent<UpgradeNotificationUI>();
        }

        if (tutorialManager == null)
        {
            tutorialManager =
                FindBestSceneComponent<TutorialManager>();
        }
    }

    private static T FindBestSceneComponent<T>() where T : Component
    {
        T[] components =
            FindObjectsByType<T>(FindObjectsInactive.Include);

        T first = null;

        foreach (T component in components)
        {
            if (component == null)
                continue;

            if (first == null)
                first = component;

            if (component.gameObject.activeInHierarchy)
                return component;
        }

        return first;
    }

    private void InstallVrAdapters()
    {
        if (GetComponent<PlayerControlAdapter>() == null &&
            GetComponentInChildren<PlayerControlAdapter>(true) == null)
        {
            gameObject.AddComponent<PlayerControlAdapter>();
        }

        if (GetComponent<XRUpgradeLocomotionAdapter>() == null &&
            GetComponentInChildren<XRUpgradeLocomotionAdapter>(true) == null)
        {
            gameObject.AddComponent<XRUpgradeLocomotionAdapter>();
        }

        if (GetComponent<XRControllerMoveFallback>() == null &&
            GetComponentInChildren<XRControllerMoveFallback>(true) == null)
        {
            gameObject.AddComponent<XRControllerMoveFallback>();
        }

        if (GetComponent<XRJumpFallback>() == null &&
            GetComponentInChildren<XRJumpFallback>(true) == null)
        {
            gameObject.AddComponent<XRJumpFallback>();
        }

        if (GetComponent<XRCharacterControllerSafety>() == null &&
            GetComponentInChildren<XRCharacterControllerSafety>(true) == null)
        {
            gameObject.AddComponent<XRCharacterControllerSafety>();
        }

        if (GetComponent<XRPlayerBodyTrigger>() == null &&
            GetComponentInChildren<XRPlayerBodyTrigger>(true) == null)
        {
            gameObject.AddComponent<XRPlayerBodyTrigger>();
        }
    }

    public bool HasUpgrade(PreyGivesUpgrade upgrade)
    {
        return upgrades.ContainsKey(upgrade);
    }

    public int GetUpgradeLevel(PreyGivesUpgrade upgrade)
    {
        if (upgrades.TryGetValue(upgrade, out int level))
            return level;

        return 0;
    }
}
