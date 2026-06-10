using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class UltrasoundSense : MonoBehaviour
{
    public float radius = 20f;
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

        foreach (Collider hit in hits)
        {
            UltrasoundTarget target =
                hit.GetComponentInParent<UltrasoundTarget>();

            if (target == null)
            {
                continue;
            }

            Renderer[] renderers =
                target.GetComponentsInChildren<Renderer>();

            foreach (Renderer r in renderers)
            {
                StartCoroutine(Highlight(r));
            }
        }

        StartCoroutine(DisableVision());
    }

    IEnumerator Highlight(Renderer renderer)
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(block);

        block.SetColor("_BaseColor", Color.white);
        block.SetColor("_Color", Color.white);

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
