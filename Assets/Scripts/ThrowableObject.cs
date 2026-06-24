using UnityEngine;
using UnityEngine.InputSystem;

public class ThrowableObject : MonoBehaviour
{
    [Header("Wurf-Einstellungen")]
    public float wurfKraft = 20f;

    [Header("Rätsel-Eigenschaften")]
    [Tooltip("Ist dieses Objekt schwer genug, um Rätseltüren aufzubrechen?")]
    public bool kannTuerenOeffnen = false;

    private Rigidbody rb;
    private Transform spielerKamera;
    private bool wirdGehalten = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        if (Camera.main != null) spielerKamera = Camera.main.transform;
    }

    public void VonZungeGefangen()
    {
        if (wirdGehalten) return;

        wirdGehalten = true;
        rb.isKinematic = true; 

        // Heftet das Objekt vor das Gesicht des Spielers
        transform.SetParent(spielerKamera);
        transform.localPosition = new Vector3(0.4f, -0.3f, 1.2f); 
        transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        // Rechte Maustaste drücken zum Werfen
        if (wirdGehalten && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Werfen();
        }
    }

    private void Werfen()
    {
        wirdGehalten = false;
        transform.SetParent(null);
        rb.isKinematic = false;
        
        // Physikalischen Stoß nach vorne ausführen
        rb.AddForce(spielerKamera.forward * wurfKraft, ForceMode.Impulse);
    }
}