using System.Collections;
using UnityEngine;

public class SonarWave : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private float startScale = 0.2f;
    [SerializeField] private Renderer waveRenderer;

    private Material materialInstance;

    private void Awake()
    {
        if (waveRenderer == null)
            waveRenderer = GetComponentInChildren<Renderer>();

        if (waveRenderer != null)
            materialInstance = waveRenderer.material;
    }

    public void Play(float maxRadius, float duration)
    {
        StartCoroutine(WaveRoutine(maxRadius, duration));
    }

    private IEnumerator WaveRoutine(float maxRadius, float duration)
    {
        float timer = 0f;

        transform.localScale = Vector3.one * startScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            float scale = Mathf.Lerp(startScale, maxRadius * 2f, t);
            transform.localScale = new Vector3(scale, 1f, scale);

            if (materialInstance != null)
            {
                Color color = materialInstance.color;
                color.a = Mathf.Lerp(1f, 0f, t);
                materialInstance.color = color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}