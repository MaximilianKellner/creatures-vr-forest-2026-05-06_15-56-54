using UnityEngine;

public class EnemyPatrolPath : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private bool loop = true;

    public int Count => waypoints.Length;

    public Transform GetWaypoint(int index)
    {
        if (waypoints == null || waypoints.Length == 0)
            return null;

        return waypoints[index];
    }

    public int GetNextIndex(int currentIndex)
    {
        if (waypoints == null || waypoints.Length == 0)
            return 0;

        int nextIndex = currentIndex + 1;

        if (nextIndex >= waypoints.Length)
            return loop ? 0 : waypoints.Length - 1;

        return nextIndex;
    }
}