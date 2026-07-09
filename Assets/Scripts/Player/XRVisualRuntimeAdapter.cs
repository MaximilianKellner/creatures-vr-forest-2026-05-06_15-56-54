using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class XRVisualRuntimeAdapter
{
    private const float HudPlaneDistance = 1.25f;
    private static bool configured;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ConfigureAfterSceneLoad()
    {
        ConfigureSceneVisuals();
    }

    public static void ConfigureSceneVisuals()
    {
        Camera playerCamera = Camera.main ?? Object.FindAnyObjectByType<Camera>();
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
        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot == null)
            return;

        if (uiRoot.transform.localScale.sqrMagnitude < 0.0001f)
            uiRoot.transform.localScale = Vector3.one;

        Canvas[] canvases =
            Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);

        foreach (Canvas canvas in canvases)
        {
            if (canvas == null || !canvas.gameObject.scene.isLoaded)
                continue;

            if (!canvas.transform.IsChildOf(uiRoot.transform) &&
                canvas.gameObject != uiRoot)
                continue;

            ConfigureHudCanvas(canvas, playerCamera);
        }
    }

    private static void ConfigureHudCanvas(Canvas canvas, Camera playerCamera)
    {
        if (playerCamera != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = playerCamera;
            canvas.planeDistance = Mathf.Max(
                playerCamera.nearClipPlane + 0.05f,
                HudPlaneDistance);
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
}
