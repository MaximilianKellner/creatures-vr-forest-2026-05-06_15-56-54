using UnityEngine;

public class MissionTriggerZone : MonoBehaviour
{
    [Header("Mission")]
    [SerializeField] private string targetId = "GasHole";

    [Header("Einstellungen")]
    [SerializeField] private bool triggerOnlyOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnlyOnce && hasTriggered)
            return;

        if (!VRUIRuntimeSupport.IsPlayerCollider(other))
            return;

        hasTriggered = true;

        MissionManager.ReportTargetCollected(targetId);

        Debug.Log($"Missionsziel entdeckt: {targetId}");
    }
}
