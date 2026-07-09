using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>() ??
            other.GetComponentInParent<PlayerHealth>() ??
            other.GetComponentInChildren<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.Kill();
        }
    }
}
