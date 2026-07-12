using UnityEngine;
using UnityEngine.InputSystem;

public class Throwable3 : MonoBehaviour
{
    [Header("Wurf-Einstellungen")]
    public float wurfKraft = 25f;

    [Header("Rätsel-Eigenschaften")]
    public bool kannTuerenOeffnen = true; 

    [Header("Visieren (Zielhilfe)")]
    public GameObject zielPunktVorschau; 
    public float zielReichweite = 30f;

    private Rigidbody rb;
    private Transform spielerKamera;
    private bool wirdGehalten = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        if (Camera.main != null) spielerKamera = Camera.main.transform;

        if (zielPunktVorschau != null) zielPunktVorschau.SetActive(false);
    }

    public void VonZungeGefangen()
    {
        if (wirdGehalten) return;

        wirdGehalten = true;
        rb.isKinematic = true; 

        transform.SetParent(spielerKamera);
        transform.localPosition = new Vector3(0.4f, -0.3f, 1.2f); 
        transform.localRotation = Quaternion.identity;

        if (zielPunktVorschau != null) zielPunktVorschau.SetActive(true);
    }

    private void Update()
    {
        if (wirdGehalten)
        {
            ZeigeZielVorschau();

            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                Werfen();
            }
        }
    }

    private void ZeigeZielVorschau()
    {
        if (zielPunktVorschau == null || spielerKamera == null) return;

        Ray ray = new Ray(spielerKamera.position, spielerKamera.forward);
        
        if (Physics.Raycast(ray, out RaycastHit hit, zielReichweite))
        {
            zielPunktVorschau.SetActive(true);
            zielPunktVorschau.transform.position = hit.point;
        }
        else
        {
            zielPunktVorschau.SetActive(false);
        }
    }

    private void Werfen()
    {
        wirdGehalten = false;
        transform.SetParent(null);
        
        // PHYSIK ERZWINGEN
        rb.isKinematic = false;
        rb.useGravity = true; 

        // ANTI-STORE-ANIMATOR-TRICK:
        // Wenn der Trank eine schwebende Animation aus dem Store hat, schalten wir sie jetzt aus,
        // damit die Physik das Objekt normal fallen lassen kann!
        Animator storeAnimator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        if (storeAnimator != null)
        {
            storeAnimator.enabled = false;
        }
        
        Animation alteAnimation = GetComponent<Animation>() ?? GetComponentInChildren<Animation>();
        if (alteAnimation != null)
        {
            alteAnimation.enabled = false;
        }

        // Trank vor den Spieler setzen
        transform.position = spielerKamera.position + spielerKamera.forward * 1.5f;

        if (zielPunktVorschau != null) zielPunktVorschau.SetActive(false);

        // Wurf ausführen
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(spielerKamera.forward * wurfKraft, ForceMode.Impulse);
    }
}