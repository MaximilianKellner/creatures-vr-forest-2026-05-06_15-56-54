using System.Collections.Generic;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UpgradeNotificationUI upgradeNotificationUI;

    private Dictionary<PreyGivesUpgrade, int> upgrades = new Dictionary<PreyGivesUpgrade, int>();

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