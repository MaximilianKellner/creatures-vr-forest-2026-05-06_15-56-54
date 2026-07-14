using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public static class VRUIRuntimeSupport
{
    public static bool IsLikelyVrScene()
    {
        if (Object.FindAnyObjectByType<VRAbilityProvider>() != null)
            return true;

        if (Object.FindAnyObjectByType<PlayerControlAdapter>() != null)
            return true;

        Camera mainCamera = Camera.main;
        return mainCamera != null && mainCamera.stereoEnabled;
    }

    public static UpgradeSystem FindBestUpgradeSystem()
    {
        UpgradeSystem[] upgradeSystems =
            Object.FindObjectsByType<UpgradeSystem>(FindObjectsInactive.Include);

        UpgradeSystem firstActive = null;
        UpgradeSystem inactiveVr = null;

        foreach (UpgradeSystem candidate in upgradeSystems)
        {
            if (candidate == null)
                continue;

            bool isActive = candidate.gameObject.activeInHierarchy;
            bool isVrPlayer = IsLikelyVrPlayer(candidate.transform);

            if (isActive && firstActive == null)
                firstActive = candidate;

            if (isActive && isVrPlayer)
                return candidate;

            if (isVrPlayer && inactiveVr == null)
                inactiveVr = candidate;
        }

        return firstActive != null ? firstActive : inactiveVr;
    }

    public static PlayerHealth FindBestPlayerHealth()
    {
        PlayerHealth[] healthComponents =
            Object.FindObjectsByType<PlayerHealth>(FindObjectsInactive.Include);

        PlayerHealth firstActive = null;
        PlayerHealth inactiveVr = null;

        foreach (PlayerHealth candidate in healthComponents)
        {
            if (candidate == null)
                continue;

            bool isActive = candidate.gameObject.activeInHierarchy;
            bool isVrPlayer = IsLikelyVrPlayer(candidate.transform);

            if (isActive && firstActive == null)
                firstActive = candidate;

            if (isActive && isVrPlayer)
                return candidate;

            if (isVrPlayer && inactiveVr == null)
                inactiveVr = candidate;
        }

        return firstActive != null ? firstActive : inactiveVr;
    }

    public static Transform FindBestPlayerTransform()
    {
        UpgradeSystem upgradeSystem = FindBestUpgradeSystem();
        if (upgradeSystem != null)
            return upgradeSystem.transform;

        PlayerHealth playerHealth = FindBestPlayerHealth();
        if (playerHealth != null)
            return playerHealth.transform;

        Camera mainCamera = Camera.main;
        return mainCamera != null ? mainCamera.transform.root : null;
    }

    public static Transform FindBestPlayerBodyTransform()
    {
        Transform playerTransform = FindBestPlayerTransform();

        if (playerTransform != null)
        {
            Transform body = FindChildByName(playerTransform, "PlayerBody") ??
                             FindChildByName(playerTransform, "XR Player Body Trigger");

            if (body != null)
                return body;
        }

        Camera mainCamera = Camera.main;
        return mainCamera != null ? mainCamera.transform : playerTransform;
    }

    public static PlayerAbilityUI FindBestPlayerAbilityUI()
    {
        PlayerAbilityUI[] abilityUis =
            Object.FindObjectsByType<PlayerAbilityUI>(FindObjectsInactive.Include);

        PlayerAbilityUI first = null;

        foreach (PlayerAbilityUI abilityUi in abilityUis)
        {
            if (abilityUi == null)
                continue;

            if (first == null)
                first = abilityUi;

            if (abilityUi.gameObject.activeInHierarchy)
                return abilityUi;
        }

        return first;
    }

    public static bool IsPlayerCollider(Collider other)
    {
        if (other == null)
            return false;

        return other.CompareTag("Player") ||
               other.GetComponentInParent<VRAbilityProvider>() != null ||
               other.GetComponentInParent<PlayerControlAdapter>() != null ||
               other.GetComponentInParent<PlayerHealth>() != null ||
               other.GetComponentInParent<UpgradeSystem>() != null ||
               other.GetComponentInParent<CharacterController>() != null;
    }

    public static bool IsLikelyVrPlayer(Transform root)
    {
        if (root == null)
            return false;

        return root.GetComponentInParent<VRAbilityProvider>() != null ||
               root.GetComponentInChildren<VRAbilityProvider>(true) != null ||
               root.GetComponentInParent<PlayerControlAdapter>() != null ||
               root.GetComponentInChildren<PlayerControlAdapter>(true) != null ||
               root.GetComponentInParent<XRUpgradeLocomotionAdapter>() != null ||
               root.GetComponentInChildren<XRUpgradeLocomotionAdapter>(true) != null;
    }

    public static TMP_Text FindTextByName(string namePart)
    {
        TMP_Text[] texts =
            Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include);

        TMP_Text firstActive = null;

        foreach (TMP_Text text in texts)
        {
            if (text == null)
                continue;

            if (text.gameObject.activeInHierarchy && firstActive == null)
                firstActive = text;

            if (text.name.IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return text;
        }

        return firstActive;
    }

    public static Volume FindVolumeByName(params string[] nameParts)
    {
        Volume[] volumes =
            Object.FindObjectsByType<Volume>(FindObjectsInactive.Include);

        Volume firstMatch = null;

        foreach (Volume volume in volumes)
        {
            if (volume == null)
                continue;

            if (!NameContainsAny(volume.name, nameParts))
                continue;

            if (volume.gameObject.activeInHierarchy)
                return volume;

            if (firstMatch == null)
                firstMatch = volume;
        }

        return firstMatch;
    }

    private static bool NameContainsAny(string value, string[] parts)
    {
        if (string.IsNullOrEmpty(value) || parts == null)
            return false;

        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            if (value.IndexOf(part, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
        }

        return false;
    }

    private static Transform FindChildByName(Transform root, string childName)
    {
        if (root == null)
            return null;

        foreach (Transform child in root)
        {
            if (child.name == childName)
                return child;

            Transform match = FindChildByName(child, childName);
            if (match != null)
                return match;
        }

        return null;
    }
}
