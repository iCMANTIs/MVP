using UnityEngine;

public class BladeSwitch : MonoBehaviour, IInteractable
{
    [Header("Blade")]
    [SerializeField]
    private RotatingBlade[] targetBlades;

    [Header("Tutorial")]
    [SerializeField]
    private TutorialManager tutorialManager;

    private bool activated = false;

    public void Interact(GameObject interactor)
    {
        if (activated)
            return;

        PlayerController sister =
            interactor.GetComponentInParent<PlayerController>();

        if (sister == null)
            return;

        activated = true;

        foreach (RotatingBlade blade in targetBlades)
        {
            if (blade != null)
                blade.DisableBlade();
        }

        if (tutorialManager != null)
            tutorialManager.CompleteBladeSection();

        Debug.Log("Sister disabled all connected blades.");
    }
}