using UnityEngine;

public class FlashlightReveal : MonoBehaviour
{
    [Header("Reveal Visual")]
    [SerializeField]
    private GameObject weakPointVisual;

    [SerializeField]
    private TutorialManager tutorialManager;

    private bool tutorialCompleted;
    private bool isRevealed;

    private void Start()
    {
        Hide();
    }

    public void Reveal()
    {
        if (isRevealed)
            return;

        isRevealed = true;

        if (weakPointVisual != null)
            weakPointVisual.SetActive(true);

        if (!tutorialCompleted)
        {
            tutorialCompleted = true;

            if (tutorialManager != null)
                tutorialManager.CompleteSisterRevealSection();
        }
    }

    public void Hide()
    {
        isRevealed = false;

        if (weakPointVisual != null)
            weakPointVisual.SetActive(false);
    }
}