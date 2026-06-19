using UnityEngine;

public class MissionTarget : MonoBehaviour
{
    [SerializeField] private string targetId;

    public string TargetId => targetId;
}