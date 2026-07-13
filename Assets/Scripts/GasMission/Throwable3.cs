using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Throwable3 : MonoBehaviour
{
    [Header("Wurf-Einstellungen")]
    [SerializeField] private float wurfGeschwindigkeit = 18f;
    [SerializeField] private float wurfNachOben = 3f;
    [SerializeField] private float rotationsKraft = 5f;

    [Header("Rätsel-Eigenschaften")]
    public bool kannTuerenOeffnen = true;

    [Header("Visieren")]
    [SerializeField] private GameObject zielPunktVorschau;
    [SerializeField] private float zielReichweite = 30f;

    [Header("Position beim Halten")]
    [SerializeField] private Vector3 haltePosition =
        new Vector3(0.4f, -0.3f, 1.2f);

    private Rigidbody rb;
    private Transform spielerKamera;
    private Collider objektCollider;

    private bool wirdGehalten;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        objektCollider = GetComponent<Collider>();

        KameraSuchen();

        if (zielPunktVorschau != null)
            zielPunktVorschau.SetActive(false);
    }

    private void KameraSuchen()
    {
        if (Camera.main != null)
            spielerKamera = Camera.main.transform;
    }

    public void VonZungeGefangen()
    {
        if (wirdGehalten)
            return;

        if (spielerKamera == null)
            KameraSuchen();

        if (spielerKamera == null)
        {
            Debug.LogError(
                $"{name}: Keine Kamera mit dem Tag 'MainCamera' gefunden."
            );
            return;
        }

        wirdGehalten = true;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.isKinematic = true;

        if (objektCollider != null)
            objektCollider.enabled = false;

        transform.SetParent(spielerKamera);
        transform.localPosition = haltePosition;
        transform.localRotation = Quaternion.identity;

        if (zielPunktVorschau != null)
            zielPunktVorschau.SetActive(true);
    }

    private void Update()
    {
        if (!wirdGehalten)
            return;

        ZeigeZielVorschau();

        if (Mouse.current != null &&
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            Werfen();
        }
    }

    private void ZeigeZielVorschau()
    {
        if (zielPunktVorschau == null || spielerKamera == null)
            return;

        Ray ray = new Ray(
            spielerKamera.position,
            spielerKamera.forward
        );

        if (Physics.Raycast(
                ray,
                out RaycastHit hit,
                zielReichweite,
                ~0,
                QueryTriggerInteraction.Ignore))
        {
            zielPunktVorschau.SetActive(true);
            zielPunktVorschau.transform.position =
                hit.point + hit.normal * 0.02f;

            zielPunktVorschau.transform.rotation =
                Quaternion.LookRotation(hit.normal);
        }
        else
        {
            zielPunktVorschau.SetActive(false);
        }
    }

    private void Werfen()
    {
        if (spielerKamera == null)
            return;

        wirdGehalten = false;

        transform.SetParent(null, true);

        Animator animator =
            GetComponent<Animator>() ??
            GetComponentInChildren<Animator>();

        if (animator != null)
            animator.enabled = false;

        Animation animationComponent =
            GetComponent<Animation>() ??
            GetComponentInChildren<Animation>();

        if (animationComponent != null)
            animationComponent.enabled = false;

        transform.position =
            spielerKamera.position +
            spielerKamera.forward * 1.5f;

        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 wurfRichtung =
            spielerKamera.forward +
            Vector3.up * (wurfNachOben / wurfGeschwindigkeit);

        wurfRichtung.Normalize();

        rb.linearVelocity =
            wurfRichtung * wurfGeschwindigkeit;

        rb.AddTorque(
            Random.onUnitSphere * rotationsKraft,
            ForceMode.Impulse
        );

        if (objektCollider != null)
            objektCollider.enabled = true;

        if (zielPunktVorschau != null)
            zielPunktVorschau.SetActive(false);
    }
}