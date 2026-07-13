using System.Collections;
using UnityEngine;

public class Puzzle3 : MonoBehaviour
{
    [Header("Tür-Rotation")]
    [Tooltip("Um wie viel Grad soll sich die Tür drehen?")]
    [SerializeField] private Vector3 rotationsWinkel =
        new Vector3(0f, 90f, 0f);

    [SerializeField] private float drehGeschwindigkeit = 150f;

    [Header("Mission")]
    [Tooltip("Muss mit der Required Target Id im MissionManager übereinstimmen.")]
    [SerializeField] private string missionTargetId = "Door";

    [Header("Tutorial Manager")]
    [SerializeField] private TutorialManager tutorialManager;

    [Header("Tutorial 1 - Giftiges Gas")]
    [TextArea(4, 8)]
    [SerializeField] private string gasTutorialText =
        "Du atmest giftiges Gas ein!\n\n" +
        "Finde schnell heraus, woher das Gas kommt, " +
        "und schließe die Quelle.";

    [SerializeField] private float gasTutorialDuration = 7f;

    [Header("Tutorial 2 - Missionsziele")]
    [TextArea(4, 8)]
    [SerializeField] private string missionTutorialText =
        "Du kannst deine aktuellen Missionsziele jederzeit einsehen.\n\n" +
        "Halte dazu die Taste [T] gedrückt.";

    [SerializeField] private float missionTutorialDuration = 5f;

    [Header("Tutorial 3 - Beute")]
    [TextArea(4, 8)]
    [SerializeField] private string preyTutorialText =
        "Friss Beute, um Leben aufzufüllen und neue Fähigkeiten " +
        "oder Verbesserungen zu erhalten.\n\n" +
        "Aber Vorsicht: Manche Beutetiere sind giftig!";

    [SerializeField] private float preyTutorialDuration = 7f;

    [Header("Tutorial 4 - Pause")]
    [TextArea(3, 6)]
    [SerializeField] private string pauseTutorialText =
        "Du kannst das Spiel jederzeit pausieren.\n\n" +
        "Drücke dazu die Taste [M].";

    [SerializeField] private float pauseTutorialDuration = 5f;

    [Header("Abstand zwischen Tutorials")]
    [Tooltip("Kurze Pause zwischen zwei Tutorialfenstern.")]
    [SerializeField] private float zeitZwischenTutorials = 1f;

    private bool istOffen;
    private bool missionGemeldet;
    private Quaternion zielRotation;

    private void Start()
    {
        zielRotation =
            transform.rotation *
            Quaternion.Euler(rotationsWinkel);
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
            Quaternion.Angle(
                transform.rotation,
                zielRotation
            ) <= 0.1f)
        {
            transform.rotation = zielRotation;

            missionGemeldet = true;

            // Erste Mission abschließen
            MissionManager.ReportTargetCollected(
                missionTargetId
            );

            // Tutorials nacheinander anzeigen
            if (tutorialManager != null)
            {
                StartCoroutine(
                    ShowTutorialSequence()
                );
            }

            Debug.Log(
                $"Türmission abgeschlossen: {missionTargetId}"
            );
        }
    }

    private IEnumerator ShowTutorialSequence()
    {
        // Tutorial 1: Giftiges Gas
        tutorialManager.ShowTutorial(
            gasTutorialText,
            gasTutorialDuration
        );

        yield return new WaitForSeconds(
            gasTutorialDuration + zeitZwischenTutorials
        );

        // Tutorial 2: Missionsziele mit T anzeigen
        tutorialManager.ShowTutorial(
            missionTutorialText,
            missionTutorialDuration
        );

        yield return new WaitForSeconds(
            missionTutorialDuration + zeitZwischenTutorials
        );

        // Tutorial 3: Beute, Leben und Fähigkeiten
        tutorialManager.ShowTutorial(
            preyTutorialText,
            preyTutorialDuration
        );

        yield return new WaitForSeconds(
            preyTutorialDuration + zeitZwischenTutorials
        );

        // Tutorial 4: Spiel mit M pausieren
        tutorialManager.ShowTutorial(
            pauseTutorialText,
            pauseTutorialDuration
        );
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (istOffen)
            return;

        Throwable3 geworfenesObjekt =
            collision.gameObject
                .GetComponentInParent<Throwable3>();

        if (geworfenesObjekt == null)
            return;

        if (geworfenesObjekt.kannTuerenOeffnen)
        {
            istOffen = true;

            Debug.Log(
                "Tür öffnet sich durch schweres Objekt!"
            );
        }
        else
        {
            Debug.Log(
                "Das Objekt ist zu leicht."
            );
        }
    }
}