using UnityEngine;

public class BrotherSplitEndTrigger : MonoBehaviour
{
    [SerializeField]
    private TutorialManager tutorialManager;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        PlayerController_B brother =
            other.GetComponentInParent<PlayerController_B>();

        if (brother == null)
            return;

        triggered = true;

        if (tutorialManager != null)
            tutorialManager.CompleteBrotherSplit();
    }
}