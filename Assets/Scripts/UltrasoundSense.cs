using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

public class UltrasoundSense : MonoBehaviour
{
    public float radius = 20f;
    public LayerMask detectableLayer;
    public Material highlightMaterial;
    public float highlightTime = 1.5f;
    public Volume seismicVolume;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ActivateUltrasound();
        }
    }

    public void ActivateUltrasound()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, detectableLayer);
        seismicVolume.weight = 1;

        foreach (Collider hit in hits)
        {
            Renderer[] renderers = hit.GetComponentsInChildren<Renderer>();

            foreach (Renderer r in renderers)
            {
                StartCoroutine(Highlight(r));
            }
        }
        StartCoroutine(DisableVision());
    }

    IEnumerator Highlight(Renderer renderer)
    {
        Color oldColor = renderer.material.color;

        renderer.material.color = Color.gray;

        yield return new WaitForSeconds(highlightTime);

        renderer.material.color = oldColor;
    }

    IEnumerator DisableVision()
    {
        yield return new WaitForSeconds(highlightTime);

        seismicVolume.weight = 0;
    }
}
