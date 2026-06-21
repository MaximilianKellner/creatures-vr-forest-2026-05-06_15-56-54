using System.Collections;
using UnityEngine;

public class SonarTarget : MonoBehaviour
{
    [Header("Renderer, die beim Sonar leuchten sollen")]
    [SerializeField] private Renderer[] renderers;

    [Header("Material während Ultraschall")]
    [SerializeField] private Material sonarMaterial;

    private Material[][] originalMaterials;
    private Coroutine sonarRoutine;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>();

        SaveOriginalMaterials();
    }

    private void SaveOriginalMaterials()
    {
        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                originalMaterials[i] = renderers[i].materials;
        }
    }

    public void ShowSonar(float duration)
    {
        if (sonarMaterial == null)
        {
            Debug.LogWarning("Kein Sonar Material zugewiesen bei: " + gameObject.name);
            return;
        }

        if (sonarRoutine != null)
            StopCoroutine(sonarRoutine);

        sonarRoutine = StartCoroutine(SonarRoutine(duration));
    }

    private IEnumerator SonarRoutine(float duration)
    {
        // Alle Renderer blau/leuchtend machen
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];

            if (r == null)
                continue;

            Material[] sonarMaterials = new Material[r.materials.Length];

            for (int m = 0; m < sonarMaterials.Length; m++)
                sonarMaterials[m] = sonarMaterial;

            r.materials = sonarMaterials;
        }

        yield return new WaitForSeconds(duration);

        // Danach echte Original-Materialien zurücksetzen
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && originalMaterials[i] != null)
                renderers[i].materials = originalMaterials[i];
        }

        sonarRoutine = null;
    }
}