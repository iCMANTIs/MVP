using System.Collections.Generic;
using UnityEngine;

public class StartRoomExitTrigger : MonoBehaviour
{

    [Header("Tutorial")]
    [SerializeField]
    private TutorialManager tutorialManager;


    private readonly HashSet<GameObject> playersExited =
        new HashSet<GameObject>();

    private bool completed;

    private void OnTriggerEnter(Collider other)
    {
        if (completed)
            return;

        GameObject player = GetPlayer(other);

        if (player == null)
            return;

        playersExited.Add(player);

        Debug.Log(
            $"Start Room Exit: {playersExited.Count}/2 players exited."
        );

        if (playersExited.Count >= 2)
        {
            CompleteObjective();
        }
    }

    private GameObject GetPlayer(Collider other)
    {
        PlayerController sister =
            other.GetComponentInParent<PlayerController>();

        if (sister != null)
        {
            return sister.gameObject;
        }

        PlayerController_B brother =
            other.GetComponentInParent<PlayerController_B>();

        if (brother != null)
        {
            return brother.gameObject;
        }

        return null;
    }

    private void CompleteObjective()
    {
        if (completed)
            return;

        completed = true;

        Debug.Log(
            "Tutorial Objective Complete: Leave the room."
        );

        if (tutorialManager != null)
        {
            tutorialManager.CompleteStartRoomObjective();
        }
    }
}