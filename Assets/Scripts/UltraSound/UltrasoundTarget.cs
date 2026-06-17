using UnityEngine;
using System.Collections.Generic;

public class UltrasoundTarget : MonoBehaviour
{
    public static readonly List<UltrasoundTarget> All = new List<UltrasoundTarget>();

    void OnEnable()
    {
        All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }
}
