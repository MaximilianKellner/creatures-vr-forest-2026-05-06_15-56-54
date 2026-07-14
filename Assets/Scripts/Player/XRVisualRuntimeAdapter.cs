using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Unity.XR.CoreUtils;

public static class XRVisualRuntimeAdapter
{
    private const float HudPlaneDistance = 1.4f;
    private const float HudVerticalOffset = -0.08f;
    private const float HudWorldScale = 0.0015f;
    private static readonly Vector2 HudReferenceSize = new Vector2(800f, 600f);
    private static bool configured;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ConfigureAfterSceneLoad()
    {
        ConfigureSceneVisuals();
    }

    public static void ConfigureSceneVisuals()
    {
        Camera playerCamera = FindPlayerCamera();
        ConfigurePostProcessing(playerCamera);
        ConfigureHudCanvases(playerCamera);
        configured = true;
    }

    public static void EnsureSceneVisuals()
    {
        if (!configured)
            ConfigureSceneVisuals();
    }

    private static void ConfigurePostProcessing(Camera playerCamera)
    {
        if (playerCamera == null)
            return;

        UniversalAdditionalCameraData cameraData =
            playerCamera.GetComponent<UniversalAdditionalCameraData>();

        if (cameraData == null)
            return;

        cameraData.renderPostProcessing = true;
        cameraData.volumeLayerMask = ~0;
    }

    private static void ConfigureHudCanvases(Camera playerCamera)
    {
        Canvas[] canvases =
            Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);

        foreach (Canvas canvas in canvases)
        {
            if (canvas == null || !canvas.gameObject.scene.isLoaded)
                continue;

            if (!ShouldConfigureCanvas(canvas))
                continue;

            if (canvas.transform.localScale.sqrMagnitude < 0.0001f)
                canvas.transform.localScale = Vector3.one;

            Transform parent = canvas.transform.parent;
            while (parent != null)
            {
                if (parent.localScale.sqrMagnitude < 0.0001f)
                    parent.localScale = Vector3.one;

                parent = parent.parent;
            }

            if (canvas.transform.GetComponentInParent<Canvas>(true) != canvas)
                continue;

            ConfigureHudCanvas(canvas, playerCamera);
        }
    }

    private static void ConfigureHudCanvas(Canvas canvas, Camera playerCamera)
    {
        if (playerCamera != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = playerCamera;
            canvas.planeDistance = HudPlaneDistance;

            RectTransform rectTransform = canvas.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                if (rectTransform.sizeDelta.sqrMagnitude < 1f)
                    rectTransform.sizeDelta = HudReferenceSize;

                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }

            canvas.transform.localScale = Vector3.one * HudWorldScale;

            VRHudCanvasFollower follower =
                canvas.GetComponent<VRHudCanvasFollower>() ??
                canvas.gameObject.AddComponent<VRHudCanvasFollower>();

            follower.Configure(playerCamera, HudPlaneDistance, HudVerticalOffset, HudWorldScale);
        }
        else
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        canvas.overrideSorting = true;
        canvas.sortingOrder = Mathf.Max(canvas.sortingOrder, 100);

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800f, 600f);
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private static Camera FindPlayerCamera()
    {
        XROrigin[] origins =
            Object.FindObjectsByType<XROrigin>(FindObjectsInactive.Include);

        foreach (XROrigin origin in origins)
        {
            if (origin == null)
                continue;

            if (!origin.gameObject.activeSelf)
                origin.gameObject.SetActive(true);

            Camera xrCamera = origin.Camera;
            if (xrCamera != null && xrCamera.gameObject.activeInHierarchy)
            {
                EnsureMainCameraTag(xrCamera);
                return xrCamera;
            }
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
            return mainCamera;

        Camera[] cameras =
            Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);

        foreach (Camera camera in cameras)
        {
            if (camera != null && camera.gameObject.activeInHierarchy)
            {
                EnsureMainCameraTag(camera);
                return camera;
            }
        }

        return null;
    }

    private static void EnsureMainCameraTag(Camera camera)
    {
        if (camera == null || camera.CompareTag("MainCamera"))
            return;

        try
        {
            camera.tag = "MainCamera";
        }
        catch (UnityException)
        {
            // Tag may not exist in an unusual test scene; Camera.main fallback will just be unavailable.
        }
    }

    private static bool ShouldConfigureCanvas(Canvas canvas)
    {
        if (canvas.renderMode == RenderMode.WorldSpace)
            return false;

        string objectName = canvas.name;
        Transform root = canvas.transform.root;
        string rootName = root != null ? root.name : string.Empty;

        return objectName.IndexOf("UI", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               objectName.IndexOf("HUD", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               objectName.IndexOf("Menu", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               rootName.IndexOf("UI", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               rootName.IndexOf("HUD", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
               rootName.IndexOf("Menu", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

}
