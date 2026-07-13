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
    [SerializeField] private bool kannTuerenOeffnen = true;

    [Header("Visieren")]
    [SerializeField] private GameObject zielPunktVorschau;
    [SerializeField] private float zielReichweite = 30f;

    [Header("Position beim Halten")]
    [SerializeField] private Vector3 haltePosition =
        new Vector3(0.4f, -0.3f, 1.2f);

    [Header("Wurf-Sicherheit")]
    [Tooltip("Abstand vor der Kamera, an dem das Objekt beim Wurf platziert wird.")]
    [SerializeField] private float wurfStartAbstand = 1.5f;

    [Tooltip("Kurze Verzögerung, bevor der Collider wieder aktiviert wird.")]
    [SerializeField] private float colliderAktivierungsVerzoegerung = 0.05f;

    private Rigidbody rb;
    private Collider objektCollider;
    private Transform spielerKamera;

    private bool wirdGehalten;
    private bool urspruenglichUseGravity;
    private bool urspruenglichIsKinematic;

    public bool KannTuerenOeffnen => kannTuerenOeffnen;
    public bool WirdGehalten => wirdGehalten;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        objektCollider = GetComponent<Collider>();

        urspruenglichUseGravity = rb.useGravity;
        urspruenglichIsKinematic = rb.isKinematic;

        KameraSuchen();

        if (zielPunktVorschau != null)
            zielPunktVorschau.SetActive(false);
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

    private void KameraSuchen()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera != null)
            spielerKamera = mainCamera.transform;
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
                $"{name}: Keine aktive Kamera mit dem Tag 'MainCamera' gefunden.",
                this
            );

            return;
        }

        wirdGehalten = true;

        // Geschwindigkeiten nur ändern, solange der Rigidbody dynamisch ist.
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        rb.useGravity = false;
        rb.isKinematic = true;

        if (objektCollider != null)
            objektCollider.enabled = false;

        transform.SetParent(spielerKamera, false);
        transform.localPosition = haltePosition;
        transform.localRotation = Quaternion.identity;

        if (zielPunktVorschau != null)
            zielPunktVorschau.SetActive(true);
    }

    private void ZeigeZielVorschau()
    {
        if (zielPunktVorschau == null || spielerKamera == null)
            return;

        Ray ray = new Ray(
            spielerKamera.position,
            spielerKamera.forward
        );

        bool hatTreffer = Physics.Raycast(
            ray,
            out RaycastHit hit,
            zielReichweite,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore
        );

        zielPunktVorschau.SetActive(hatTreffer);

        if (!hatTreffer)
            return;

        zielPunktVorschau.transform.SetPositionAndRotation(
            hit.point + hit.normal * 0.02f,
            Quaternion.LookRotation(hit.normal)
        );
    }

    public void Werfen()
    {
        if (!wirdGehalten || spielerKamera == null)
            return;

        wirdGehalten = false;

        if (zielPunktVorschau != null)
            zielPunktVorschau.SetActive(false);

        transform.SetParent(null, true);

        AnimationenDeaktivieren();

        transform.position =
            spielerKamera.position +
            spielerKamera.forward * wurfStartAbstand;

        // Zuerst wieder dynamisch machen.
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;

        // Jetzt dürfen Geschwindigkeiten gesetzt werden.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 wurfRichtung =
            spielerKamera.forward * wurfGeschwindigkeit +
            Vector3.up * wurfNachOben;

        rb.linearVelocity = wurfRichtung;

        if (rotationsKraft > 0f)
        {
            rb.AddTorque(
                Random.onUnitSphere * rotationsKraft,
                ForceMode.Impulse
            );
        }

        if (objektCollider != null)
        {
            CancelInvoke(nameof(ColliderAktivieren));
            Invoke(
                nameof(ColliderAktivieren),
                colliderAktivierungsVerzoegerung
            );
        }
    }

    private void ColliderAktivieren()
    {
        if (objektCollider != null)
            objektCollider.enabled = true;
    }

    private void AnimationenDeaktivieren()
    {
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
    }

    private void OnDisable()
    {
        CancelInvoke();

        if (zielPunktVorschau != null)
            zielPunktVorschau.SetActive(false);
    }
}