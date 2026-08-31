using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text objectiveText;

    [Header("Game Systems")]
    [SerializeField]
    private PanicSystem[] panicSystems;


    [Header("Objectives")]
    [SerializeField]
    private string firstObjective = "Exit Room";

    [SerializeField] 
    private string secondObjective = "Go Through Hall way";

    [SerializeField]
    private string ThirdObjective = "Find your own Way";

    [Header("Split Room UI")]
    [SerializeField]
    private GameObject sisterObjectiveUI;

    [SerializeField]
    private TMP_Text sisterObjectiveText;

    [SerializeField]
    private GameObject brotherObjectiveUI;

    [SerializeField]
    private TMP_Text brotherObjectiveText;

    [Header("Split Room")]
    [SerializeField]
    private bool sisterSplitCompleted;

    [SerializeField]
    private bool brotherSplitCompleted;

    private bool splitRoomCompleted;

    [Header("Hand Hold Tutorial")]
    [SerializeField]
    //private GameObject handHoldPrompt;

    private bool meetingWallDestroyed;
    private void Start()
    {
        SetPanicEnabled(false);
        ShowObjective(firstObjective);
        //if (handHoldPrompt != null)
            //handHoldPrompt.SetActive(false);

        HideSplitRoomUI();
    }

    private void SetPanicEnabled(bool enabled)
    {
        foreach (PanicSystem panicSystem in panicSystems)
        {
            if (panicSystem != null)
            {
                panicSystem.SetPanicEnabled(enabled);
            }
        }

        Debug.Log(
            "Tutorial Panic: " +
            (enabled ? "Enabled" : "Disabled")
        );
    }

    public void CompleteStartRoomObjective()
    {
        Debug.Log("Tutorial: Start Room objective completed.");

        ShowObjective(secondObjective);
    }
    public void CompleteHallwayObjective()
    {
        ShowObjective(ThirdObjective);
    }

    private void ShowObjective(string message)
    {
        if (objectiveText == null)
            return;

        objectiveText.gameObject.SetActive(true);
        objectiveText.text = message;
    }

    public void ShowSisterObjective(string message)
    {
        if (sisterObjectiveUI != null)
            sisterObjectiveUI.SetActive(true);

        if (sisterObjectiveText != null)
            sisterObjectiveText.text = message;
    }

    public void ShowBrotherObjective(string message)
    {
        if (brotherObjectiveUI != null)
            brotherObjectiveUI.SetActive(true);

        if (brotherObjectiveText != null)
            brotherObjectiveText.text = message;
    }
    public void StartSplitRoomObjectives()
    {
        if (objectiveText != null)
            objectiveText.gameObject.SetActive(false);

        ShowSisterObjective(
            "Find a way to help Brother"
        );

        ShowBrotherObjective(
            "Find a way past the blades"
        );
    }
    public void CompleteBladeSection()
    {
        ShowSisterObjective(
            "Use your flashlight to reveal the weak point"
        );

        ShowBrotherObjective(
            "Use your axe to break the matching obstacle"
        );
    }

    public void CompleteSisterRevealSection()
    {
        ShowSisterObjective(
            "Reach the end of your path"
        );
    }
    public void CompleteBrotherAxeSection()
    {
        ShowBrotherObjective(
            "Find Your Sister"
        );
    }

    public void CompleteSisterSplit()
    {
        if (sisterSplitCompleted)
            return;

        sisterSplitCompleted = true;

        if (sisterObjectiveUI != null)
            sisterObjectiveUI.SetActive(false);

        Debug.Log("Sister completed Split Room.");

        CheckSplitRoomComplete();
    }

    public void CompleteBrotherSplit()
    {
        if (brotherSplitCompleted)
            return;

        brotherSplitCompleted = true;

        if (brotherObjectiveUI != null)
            brotherObjectiveUI.SetActive(false);

        Debug.Log("Brother completed Split Room.");

        CheckSplitRoomComplete();
    }

    private void HideSplitRoomUI()
    {
        if (sisterObjectiveUI != null)
            sisterObjectiveUI.SetActive(false);

        if (brotherObjectiveUI != null)
            brotherObjectiveUI.SetActive(false);
    }

    private void CheckSplitRoomComplete()
    {
        if (splitRoomCompleted)
            return;

        if (!sisterSplitCompleted || !brotherSplitCompleted)
            return;

        splitRoomCompleted = true;

        CompleteSplitRoom();
    }

    public void CompleteSplitRoom()
    {
        HideSplitRoomUI();

        SetPanicEnabled(true);

        Debug.Log("Split Room completed. Panic enabled.");
    }

    public void OnMeetingWallDestroyed()
    {
        if (meetingWallDestroyed)
            return;

        meetingWallDestroyed = true;

        ShowObjective("Hold LB to Hold hands with each other");

        //if (handHoldPrompt != null)
            //handHoldPrompt.SetActive(true);

        Debug.Log("Meeting wall destroyed. Hand Hold tutorial started.");
    }

    public void CompleteHandHoldObjective()
    {
        ShowObjective("Move together to exit");
    }

    public void CompleteTutorial()
    {
        if (objectiveText != null)
            objectiveText.gameObject.SetActive(false);

        Debug.Log("Tutorial Complete.");
    }


}