using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera minimapCamera;

    [Header("Settings")]
    [SerializeField] private float heightAbovePlayer = 30f;
    [SerializeField] private float orthographicSize = 30f;
    [SerializeField] private bool rotateWithPlayer = true;

    private void Awake()
    {
        if (minimapCamera == null)
            minimapCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        transform.position = new Vector3(
            player.position.x,
            player.position.y + heightAbovePlayer,
            player.position.z
        );

        transform.rotation = rotateWithPlayer
            ? Quaternion.Euler(90f, player.eulerAngles.y, 0f)
            : Quaternion.Euler(90f, 0f, 0f);

        if (minimapCamera != null)
            minimapCamera.orthographicSize = orthographicSize;
    }
}