using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class UltrasoundSense : MonoBehaviour
{
    public float radius = 20000f;
    public LayerMask detectableLayer;
    public Material highlightMaterial;
    public float highlightTime = 1.5f;
    public Volume seismicVolume;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();

    private bool isActive = false;

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame && !isActive)
        {
            ActivateUltrasound();
        }
    }

    public void ActivateUltrasound()
    {
        isActive = true;

        seismicVolume.weight = 1;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            radius,
            detectableLayer);

        Debug.Log("Treffer: " + hits.Length);

        foreach (Collider hit in hits)
        {
            Debug.Log("Gefundener Collider: " + hit.name);
        }

        foreach (Collider hit in hits)
        {
            UltrasoundTarget target =
                hit.GetComponentInParent<UltrasoundTarget>();

            CaveEchoSurface cave =
                hit.GetComponentInParent<CaveEchoSurface>();

            if (target != null)
            {
                Debug.Log("Target gefunden: " + target.name);

                Renderer[] renderers =
                    target.GetComponentsInChildren<Renderer>();

                foreach (Renderer r in renderers)
                {
                    StartCoroutine(HighlightTarget(r));
                }
            }

            if (cave != null)
            {
                Renderer[] renderers =
                    cave.GetComponentsInChildren<Renderer>();

                foreach (Renderer r in renderers)
                {
                    StartCoroutine(HighlightCave(r));
                }
            }
        }
        
        StartCoroutine(DisableVision());

    }

    IEnumerator HighlightTarget(Renderer renderer)
    {
        Material[] originalMaterials = renderer.sharedMaterials;

        Material[] newMaterials = new Material[originalMaterials.Length];

        for (int i = 0; i < newMaterials.Length; i++)
        {
            newMaterials[i] = highlightMaterial;
        }

        renderer.sharedMaterials = newMaterials;

        yield return new WaitForSeconds(highlightTime);

        renderer.sharedMaterials = originalMaterials;
    }

    IEnumerator HighlightCave(Renderer renderer)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(block);

        block.SetColor("_BaseColor", new Color(0.15f, 0.15f, 0.15f, 1f));
        block.SetColor("_Color", new Color(0.15f, 0.15f, 0.15f, 1f));

        renderer.SetPropertyBlock(block);

        yield return new WaitForSeconds(highlightTime);

        renderer.SetPropertyBlock(null);
    }

    IEnumerator DisableVision()
    {
        yield return new WaitForSeconds(highlightTime);

        seismicVolume.weight = 0;
        isActive = false;
    }
}
