using UnityEngine;

public class MinimapDrawController : MonoBehaviour
{
    public Transform player; // Hier ziehst du im Inspector deinen Spieler rein

    void LateUpdate()
    {
        if (player != null)
        {
            // Folgt der X- und Z-Position des Spielers, bleibt aber auf der hohen Y-Ebene
            Vector3 newPosition = player.position;
            newPosition.y = transform.position.y; 
            transform.position = newPosition;
        }
    }
}