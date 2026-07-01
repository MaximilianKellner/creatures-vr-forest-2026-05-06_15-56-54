using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;

public static class XRSetup
{
    const string OpenXRLoaderTypeName = "UnityEngine.XR.OpenXR.OpenXRLoader";

    public static void EnableOpenXRStandalone()
    {
        var buildTargetGroup = BuildTargetGroup.Standalone;

        if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget buildTargetSettings) || buildTargetSettings == null)
        {
            buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(buildTargetSettings, "Assets/XR/Settings/XRGeneralSettingsPerBuildTarget.asset");
            AssetDatabase.SaveAssets();
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildTargetSettings, true);
            Debug.Log("XRSetup: created XRGeneralSettingsPerBuildTarget asset");
        }

        if (!buildTargetSettings.HasSettingsForBuildTarget(buildTargetGroup))
        {
            buildTargetSettings.CreateDefaultSettingsForBuildTarget(buildTargetGroup);
            Debug.Log("XRSetup: created XRGeneralSettings for Standalone");
        }

        var generalSettings = buildTargetSettings.SettingsForBuildTarget(buildTargetGroup);
        generalSettings.InitManagerOnStart = true;

        if (!buildTargetSettings.HasManagerSettingsForBuildTarget(buildTargetGroup))
        {
            buildTargetSettings.CreateDefaultManagerSettingsForBuildTarget(buildTargetGroup);
            Debug.Log("XRSetup: created XRManagerSettings (Providers) for Standalone");
        }

        var manager = buildTargetSettings.ManagerSettingsForBuildTarget(buildTargetGroup);

        bool assigned = XRPackageMetadataStore.AssignLoader(manager, OpenXRLoaderTypeName, buildTargetGroup);
        Debug.Log($"XRSetup: AssignLoader({OpenXRLoaderTypeName}) returned {assigned}");

        EditorUtility.SetDirty(generalSettings);
        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(buildTargetSettings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        bool verified = false;
        foreach (var loader in manager.activeLoaders)
        {
            if (loader != null && loader.GetType().FullName == OpenXRLoaderTypeName)
                verified = true;
        }
        Debug.Log($"XRSetup: verified OpenXRLoader present after save = {verified}");

        if (!verified)
        {
            Debug.LogError("XRSetup: FAILED to assign OpenXR loader for Standalone.");
        }
    }
}
