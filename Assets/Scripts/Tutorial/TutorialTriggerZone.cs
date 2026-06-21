using UnityEngine;

public class TutorialTriggerZone : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TutorialManager tutorialManager;

    [Header("Tutorial Settings")]
    [TextArea(3, 8)]
    [SerializeField] private string tutorialText;

    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private bool onlyOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (onlyOnce && hasTriggered)
            return;

        hasTriggered = true;

        tutorialManager.ShowTutorial(
            tutorialText,
            displayDuration
        );
    }
}