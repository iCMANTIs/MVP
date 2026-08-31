using UnityEngine;

public class HallwayEndTrigger : MonoBehaviour
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

        PlayerController_B brother =
            other.GetComponentInParent<PlayerController_B>();

        if (sister == null && brother == null)
            return;

        triggered = true;

        if (tutorialManager != null)
            tutorialManager.CompleteHallwayObjective();
    }
}