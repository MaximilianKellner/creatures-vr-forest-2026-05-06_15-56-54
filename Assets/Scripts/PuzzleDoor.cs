using UnityEngine;

public class PuzzleDoor : MonoBehaviour
{
    [Header("Tür-Rotation")]
    [Tooltip("Um wie viel Grad soll sich die Tür drehen? (Y = 90 oder -90)")]
    public Vector3 rotationsWinkel = new Vector3(0, 90f, 0);
    public float drehGeschwindigkeit = 150f;

    private bool istOffen = false;
    private Quaternion zielRotation;

    private void Start()
    {
        zielRotation = transform.rotation * Quaternion.Euler(rotationsWinkel);
    }

    private void Update()
    {
        if (istOffen)
        {
            // Dreht die Tür flüssig auf
            transform.rotation = Quaternion.RotateTowards(transform.rotation, zielRotation, drehGeschwindigkeit * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ThrowableObject geworfenesObjekt = collision.gameObject.GetComponent<ThrowableObject>();

        if (geworfenesObjekt != null)
        {
            // Wenn das Objekt die Erlaubnis hat, öffnet sich die Tür
            if (geworfenesObjekt.kannTuerenOeffnen)
            {
                istOffen = true;
                Debug.Log("Tür öffnet sich durch schweres Objekt!");
            }
            else
            {
                Debug.Log("Das Objekt ist zu leicht. Die Tür bleibt zu.");
            }
        }
    }
}