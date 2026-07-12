using UnityEngine;

public class Puzzle3 : MonoBehaviour
{
    [Header("Tür-Rotation")]
    [Tooltip("Um wie viel Grad soll sich die Tür drehen?")]
    [SerializeField] private Vector3 rotationsWinkel = new Vector3(0f, 90f, 0f);

    [SerializeField] private float drehGeschwindigkeit = 150f;

    [Header("Mission")]
    [Tooltip("Muss mit der Required Target Id im MissionManager übereinstimmen.")]
    [SerializeField] private string missionTargetId = "Door";

    [Header("Tutorial nach dem Öffnen")]
    [SerializeField] private TutorialManager tutorialManager;

    [TextArea(4, 8)]
    [SerializeField] private string tutorialText =
        "Du atmest giftiges Gas ein!\n\n" +
        "Finde schnell heraus, woher das Gas kommt, " +
        "und schließe die Quelle.";

    [SerializeField] private float tutorialDuration = 7f;

    private bool istOffen;
    private bool missionGemeldet;
    private Quaternion zielRotation;

    private void Start()
    {
        zielRotation = transform.rotation * Quaternion.Euler(rotationsWinkel);
    }

    private void Update()
    {
        if (!istOffen)
            return;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            zielRotation,
            drehGeschwindigkeit * Time.deltaTime
        );

        if (!missionGemeldet &&
            Quaternion.Angle(transform.rotation, zielRotation) <= 0.1f)
        {
            transform.rotation = zielRotation;

            missionGemeldet = true;

            // Erste Mission abschließen.
            MissionManager.ReportTargetCollected(missionTargetId);

            // Tutorial anzeigen.
            if (tutorialManager != null)
            {
                tutorialManager.ShowTutorial(
                    tutorialText,
                    tutorialDuration
                );
            }

            Debug.Log($"Türmission abgeschlossen: {missionTargetId}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (istOffen)
            return;

        Throwable3 geworfenesObjekt =
            collision.gameObject.GetComponentInParent<Throwable3>();

        if (geworfenesObjekt == null)
            return;

        if (geworfenesObjekt.kannTuerenOeffnen)
        {
            istOffen = true;
            Debug.Log("Tür öffnet sich durch schweres Objekt!");
        }
        else
        {
            Debug.Log("Das Objekt ist zu leicht.");
        }
    }
}