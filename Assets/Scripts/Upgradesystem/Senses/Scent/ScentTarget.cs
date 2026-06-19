using UnityEngine;

public enum ScentType
{
    Normal,
    Upgrade,
    Poison
}

public class ScentTarget : MonoBehaviour
{
    [Header("Scent Type")]
    public ScentType scentType = ScentType.Normal;

    [Header("Optional")]
    public bool canBeTracked = true;
}