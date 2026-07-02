using System.Collections.Generic;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UpgradeNotificationUI upgradeNotificationUI;
    [SerializeField] private TutorialManager tutorialManager;

    private Dictionary<PreyGivesUpgrade, int> upgrades = new Dictionary<PreyGivesUpgrade, int>();
    private HashSet<PreyGivesUpgrade> shownTutorials = new HashSet<PreyGivesUpgrade>();

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
        switch (upgrade)
        {
            case PreyGivesUpgrade.NightVision:
                tutorialManager.ShowTutorial(
                    "Drücke [N], um die Nachtsicht zu aktivieren",
                    6f);
                break;

            case PreyGivesUpgrade.ScentSense:
                tutorialManager.ShowTutorial(
                    "Drücke [G], um den Geruchssinn zu aktivieren.",
                    6f);
                break;

            case PreyGivesUpgrade.SonarSense:
                tutorialManager.ShowTutorial(
                    "Drücke [B], um den Sonarsinn zu aktivieren.",
                    6f);
                break;

            case PreyGivesUpgrade.HearingSense:
                tutorialManager.ShowTutorial(
                    "Drücke [H], um den Hörsinn zu aktivieren.",
                    6f);
                break;

            case PreyGivesUpgrade.Sprint:
                tutorialManager.ShowTutorial(
                    "Drücke [Shift], um zu sprinten.",
                    6f);
                break;
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