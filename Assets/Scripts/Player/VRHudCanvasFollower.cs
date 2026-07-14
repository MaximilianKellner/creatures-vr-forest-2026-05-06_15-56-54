using UnityEngine;

public class VRHudCanvasFollower : MonoBehaviour
{
    private Camera playerCamera;
    private float distance = 1.4f;
    private float verticalOffset = -0.08f;
    private float worldScale = 0.0015f;

    public void Configure(Camera camera, float canvasDistance, float yOffset, float scale)
    {
        playerCamera = camera;
        distance = canvasDistance;
        verticalOffset = yOffset;
        worldScale = scale;
        FollowCamera();
    }

    private void LateUpdate()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        FollowCamera();
    }

    private void FollowCamera()
    {
        if (playerCamera == null)
            return;

        Transform cameraTransform = playerCamera.transform;
        transform.position =
            cameraTransform.position +
            cameraTransform.forward * distance +
            cameraTransform.up * verticalOffset;
        transform.rotation = cameraTransform.rotation;
        transform.localScale = Vector3.one * worldScale;
    }
}
