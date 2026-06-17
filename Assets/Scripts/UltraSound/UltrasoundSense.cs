using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class UltrasoundSense : MonoBehaviour
{
    public float radius = 50000000f;
    public Material highlightMaterial;
    public float highlightTime = 1.5f;
    public Volume seismicVolume;

    private bool isActive = false;

    private Dictionary<Renderer, Material[]> originalMaterials =
        new Dictionary<Renderer, Material[]>();

    private void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame && !isActive)
        {
            StartCoroutine(UltrasoundRoutine());
        }
    }

    private IEnumerator UltrasoundRoutine()
    {
        isActive = true;

        if (seismicVolume != null)
            seismicVolume.weight = 1;

        float timer = 0f;

        while (timer < highlightTime)
        {
            UpdateTargets();

            timer += Time.deltaTime;
            yield return null;
        }

        ResetAllTargets();

        if (seismicVolume != null)
            seismicVolume.weight = 0;

        isActive = false;
    }

    private void UpdateTargets()
    {
        HashSet<Renderer> currentlyVisible = new HashSet<Renderer>();

        foreach (UltrasoundTarget target in UltrasoundTarget.All)
        {
            if (target == null)
                continue;

            float distance = Vector3.Distance(transform.position, target.transform.position);

            if (distance > radius)
                continue;

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

            foreach (Renderer r in renderers)
            {
                currentlyVisible.Add(r);

                if (!originalMaterials.ContainsKey(r))
                {
                    originalMaterials[r] = r.sharedMaterials;
                }

                float brightness = 1f - Mathf.Clamp01(distance / radius);
                brightness = Mathf.Lerp(0.05f, 1f, brightness);

                ApplyHighlight(r, brightness);
            }
        }

        List<Renderer> toReset = new List<Renderer>();

        foreach (Renderer r in originalMaterials.Keys)
        {
            if (!currentlyVisible.Contains(r))
            {
                toReset.Add(r);
            }
        }

        foreach (Renderer r in toReset)
        {
            ResetRenderer(r);
        }
    }

    private void ApplyHighlight(Renderer renderer, float brightness)
    {
        Material[] original = originalMaterials[renderer];
        Material[] newMaterials = new Material[original.Length];

        Color color = new Color(brightness, brightness, brightness, 1f);

        for (int i = 0; i < newMaterials.Length; i++)
        {
            Material mat = new Material(highlightMaterial);
            mat.color = color;
            newMaterials[i] = mat;
        }

        renderer.sharedMaterials = newMaterials;
    }

    private void ResetRenderer(Renderer renderer)
    {
        if (!originalMaterials.ContainsKey(renderer))
            return;

        renderer.sharedMaterials = originalMaterials[renderer];
        originalMaterials.Remove(renderer);
    }

    private void ResetAllTargets()
    {
        List<Renderer> renderers = new List<Renderer>(originalMaterials.Keys);

        foreach (Renderer r in renderers)
        {
            ResetRenderer(r);
        }
    }
}
