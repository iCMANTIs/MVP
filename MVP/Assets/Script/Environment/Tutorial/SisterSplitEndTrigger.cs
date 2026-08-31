using UnityEngine;

public class SisterSplitEndTrigger : MonoBehaviour
{
    [SerializeField]
    private TutorialManager tutorialManager;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        PlayerController sister =
            other.GetComponentInParent<PlayerController>();

        if (sister == null)
            return;

        triggered = true;

        if (tutorialManager != null)
            tutorialManager.CompleteSisterSplit();
    }
}