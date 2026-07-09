using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] private float height = 30f;
    [SerializeField] private bool rotateWithPlayer = true;

    private void LateUpdate()
    {
        if (player == null)
            return;

        transform.position = new Vector3(
            player.position.x,
            height,
            player.position.z
        );

        if (rotateWithPlayer)
        {
            transform.rotation = Quaternion.Euler(
                90f,
                player.eulerAngles.y,
                0f
            );
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}