using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FireflyGlow : MonoBehaviour
{
    [SerializeField] private Light pointLight;

    [SerializeField] private Color emissionColor = Color.yellow;

    [SerializeField] private float minIntensity = 1f;
    [SerializeField] private float maxIntensity = 6f;

    [SerializeField] private float lightMin = 0.5f;
    [SerializeField] private float lightMax = 2f;

    [SerializeField] private float blinkSpeed = 2f;

    private Material materialInstance;

    private void Awake()
    {
        materialInstance = GetComponent<Renderer>().material;
        materialInstance.EnableKeyword("_EMISSION");
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;

        float emission = Mathf.Lerp(minIntensity, maxIntensity, t);
        materialInstance.SetColor("_EmissionColor", emissionColor * emission);

        if (pointLight != null)
            pointLight.intensity = Mathf.Lerp(lightMin, lightMax, t);
    }
}