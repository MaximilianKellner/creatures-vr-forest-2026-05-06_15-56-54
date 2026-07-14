using TMPro;
using UnityEngine;

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

        foreach (UpgradeSystem candidate in upgradeSystems)
        {
            if (candidate == null)
                continue;

            if (candidate.gameObject.activeInHierarchy && firstActive == null)
                firstActive = candidate;

            if (IsLikelyVrPlayer(candidate.transform))
                return candidate;
        }

        return firstActive;
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
}
