using UnityEngine;

public class RuntimeMeshColliderBuilder : MonoBehaviour
{
    [SerializeField] bool includeInactive;
    [SerializeField] bool skipObjectsWithCollider = true;

    void Awake()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(includeInactive);

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null)
                continue;

            MeshCollider existingMeshCollider = meshFilter.GetComponent<MeshCollider>();
            if (existingMeshCollider != null && existingMeshCollider.sharedMesh == null)
            {
                existingMeshCollider.sharedMesh = meshFilter.sharedMesh;
                existingMeshCollider.convex = false;
                continue;
            }

            if (skipObjectsWithCollider && meshFilter.GetComponent<Collider>() != null)
                continue;

            MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.convex = false;
        }
    }
}
