using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAbilityUI : MonoBehaviour
{
    [System.Serializable]
    public class AbilitySlot
    {
        public PreyGivesUpgrade ability;
        public GameObject activeOverlay;
        public Image cooldownOverlay;
    }

    [Header("References")]
    [SerializeField] private UpgradeSystem upgradeSystem;

    [Header("Ability Slots")]
    [SerializeField] private List<AbilitySlot> slots = new List<AbilitySlot>();

    private Dictionary<PreyGivesUpgrade, AbilitySlot> slotDictionary;

    private void Awake()
    {
        ResolveUpgradeSystem();
        slotDictionary = new Dictionary<PreyGivesUpgrade, AbilitySlot>();

        foreach (AbilitySlot slot in slots)
        {
            if (slot == null)
                continue;

            if (!slotDictionary.ContainsKey(slot.ability))
                slotDictionary.Add(slot.ability, slot);

            if (slot.activeOverlay != null)
                slot.activeOverlay.SetActive(false);

            if (slot.cooldownOverlay != null)
                slot.cooldownOverlay.fillAmount = 1f;
        }
    }

    private void OnEnable()
    {
        ResolveUpgradeSystem();

        if (upgradeSystem != null)
            upgradeSystem.OnUpgradeUnlocked += HandleUpgradeUnlocked;
    }

    private void OnDisable()
    {
        if (upgradeSystem != null)
            upgradeSystem.OnUpgradeUnlocked -= HandleUpgradeUnlocked;
    }

    private void Start()
    {
        ResolveUpgradeSystem();
        RefreshUnlocks();
    }

    private void HandleUpgradeUnlocked(PreyGivesUpgrade upgrade)
    {
        SetReady(upgrade);
    }

    public void RefreshUnlocks()
    {
        ResolveUpgradeSystem();

        foreach (AbilitySlot slot in slots)
        {
            if (slot == null || slot.cooldownOverlay == null)
                continue;

            bool unlocked =
                upgradeSystem != null &&
                upgradeSystem.HasUpgrade(slot.ability);

            if (slot.activeOverlay != null)
                slot.activeOverlay.SetActive(false);

            slot.cooldownOverlay.fillAmount = unlocked ? 0f : 1f;
        }
    }

    public void SetAbilityState(PreyGivesUpgrade ability, bool active, float cooldownProgress)
    {
        ResolveUpgradeSystem();

        if (slotDictionary == null)
            return;

        if (!slotDictionary.ContainsKey(ability))
            return;

        AbilitySlot slot = slotDictionary[ability];

        if (slot.activeOverlay != null)
            slot.activeOverlay.SetActive(active);

        if (slot.cooldownOverlay != null)
            slot.cooldownOverlay.fillAmount = Mathf.Clamp01(cooldownProgress);
    }

    public void SetLocked(PreyGivesUpgrade ability)
    {
        SetAbilityState(ability, false, 1f);
    }

    public void SetReady(PreyGivesUpgrade ability)
    {
        SetAbilityState(ability, false, 0f);
    }

    private void ResolveUpgradeSystem()
    {
        if (upgradeSystem != null && VRUIRuntimeSupport.IsLikelyVrPlayer(upgradeSystem.transform))
            return;

        UpgradeSystem bestUpgradeSystem = VRUIRuntimeSupport.FindBestUpgradeSystem();

        if (bestUpgradeSystem == null || bestUpgradeSystem == upgradeSystem)
            return;

        if (isActiveAndEnabled && upgradeSystem != null)
            upgradeSystem.OnUpgradeUnlocked -= HandleUpgradeUnlocked;

        upgradeSystem = bestUpgradeSystem;

        if (isActiveAndEnabled)
            upgradeSystem.OnUpgradeUnlocked += HandleUpgradeUnlocked;
    }
}
