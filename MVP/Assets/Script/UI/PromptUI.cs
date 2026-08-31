using System.Collections.Generic;
using UnityEngine;

public class PromptUITrigger : MonoBehaviour
{
    public enum PlayerType
    {
        Both,
        SisterOnly,
        BrotherOnly
    }

    [Header("Prompt UI")]
    [SerializeField]
    private GameObject promptCanvas;

    [Header("Who Can See It")]
    [SerializeField]
    private PlayerType allowedPlayer = PlayerType.Both;

    [Header("Settings")]
    [SerializeField]
    private bool hideOnStart = true;

    [SerializeField]
    private bool oneTimeOnly = false;

    private bool completed;

    private readonly HashSet<GameObject> playersInside =
        new HashSet<GameObject>();

    private void Start()
    {
        if (hideOnStart && promptCanvas != null)
        {
            promptCanvas.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (completed)
            return;

        GameObject player = GetValidPlayer(other);

        if (player == null)
            return;

        playersInside.Add(player);

        UpdatePrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject player = GetValidPlayer(other);

        if (player == null)
            return;

        playersInside.Remove(player);

        UpdatePrompt();
    }

    private GameObject GetValidPlayer(Collider other)
    {
        PlayerController sister =
            other.GetComponentInParent<PlayerController>();

        if (sister != null)
        {
            if (allowedPlayer == PlayerType.BrotherOnly)
                return null;

            return sister.gameObject;
        }

        PlayerController_B brother =
            other.GetComponentInParent<PlayerController_B>();

        if (brother != null)
        {
            if (allowedPlayer == PlayerType.SisterOnly)
                return null;

            return brother.gameObject;
        }

        return null;
    }

    private void UpdatePrompt()
    {
        if (promptCanvas == null)
            return;

        promptCanvas.SetActive(
            playersInside.Count > 0 && !completed
        );
    }

    public void HidePrompt()
    {
        playersInside.Clear();

        if (promptCanvas != null)
        {
            promptCanvas.SetActive(false);
        }
    }

    public void CompletePrompt()
    {
        completed = true;

        playersInside.Clear();

        if (promptCanvas != null)
        {
            promptCanvas.SetActive(false);
        }
    }

    public void ResetPrompt()
    {
        completed = false;
        playersInside.Clear();

        if (promptCanvas != null)
        {
            promptCanvas.SetActive(false);
        }
    }
}