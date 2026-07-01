using System.Collections;
using TMPro;
using UnityEngine;

public class UpgradeNotificationUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text upgradeText;

    [Header("Settings")]
    [SerializeField] private float showTime = 3f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowUpgrade(PreyGivesUpgrade upgrade, int level)
    {
        if (upgrade == PreyGivesUpgrade.None)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(upgrade, level));
    }

    private IEnumerator ShowRoutine(PreyGivesUpgrade upgrade, int level)
    {
        if (panel != null)
            panel.SetActive(true);

        if (upgradeText != null)
        {
            upgradeText.text = GetUpgradeText(upgrade, level);
        }

        yield return new WaitForSeconds(showTime);

        if (panel != null)
            panel.SetActive(false);
    }

    private string GetUpgradeText(PreyGivesUpgrade upgrade, int level)
    {
        switch (upgrade)
        {
            case PreyGivesUpgrade.Vision:
                return "Neue Fähigkeit freigeschaltet:\nSehsinn";

            case PreyGivesUpgrade.NightVision:
                return "Neue Fähigkeit freigeschaltet:\nNachtsicht";

            case PreyGivesUpgrade.ScentSense:
                return "Neue Fähigkeit freigeschaltet:\nGeruchssinn";

            case PreyGivesUpgrade.SonarSense:
                return "Neue Fähigkeit freigeschaltet:\nSonarsinn";

            case PreyGivesUpgrade.HearingSense:
                return "Neue Fähigkeit freigeschaltet:\nGehörsinn";

            case PreyGivesUpgrade.HigherJump:
                return "Upgrade erhalten:\nHöher springen Level " + level;

            case PreyGivesUpgrade.Sprint:
                return "Neue Fähigkeit freigeschaltet:\nSprinten";

            case PreyGivesUpgrade.FasterRun:
                return "Upgrade erhalten:\nSchneller laufen Level " + level;

            case PreyGivesUpgrade.TongueSpeed:
                return "Upgrade erhalten:\nSchnellere Zunge Level " + level;

            case PreyGivesUpgrade.MoreHealth:
                return "Upgrade erhalten:\nMehr Leben Level " + level;

            default:
                return "Upgrade freigeschaltet";
        }
    }
}