using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Door")]
    [SerializeField]
    private Transform doorPivot;

    [SerializeField]
    private float openAngle = 90f;

    [SerializeField]
    private float rotationSpeed = 180f;

    [Header("State")]
    [SerializeField]
    private bool isOpen;

    [Header("Interaction Prompt")]
    [SerializeField]
    private GameObject promptCanvas;

    private Quaternion closedRotation;
    private Quaternion openRotation;


    private readonly HashSet<GameObject> nearbyPlayers =
        new HashSet<GameObject>();


    private void Awake()
    {
        if (doorPivot == null)
        {
            Debug.LogError(
                $"{name}: Door Pivot is missing.",
                this
            );

            return;
        }

        closedRotation = doorPivot.localRotation;

        openRotation =
            closedRotation *
            Quaternion.Euler(0f, openAngle, 0f);

        if (promptCanvas != null)
        {
            promptCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        if (doorPivot == null)
            return;

        Quaternion targetRotation =
            isOpen ? openRotation : closedRotation;

        doorPivot.localRotation =
            Quaternion.RotateTowards(
                doorPivot.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }

    public void Interact(GameObject interactor)
    {
        ToggleDoor();
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        Debug.Log(
            $"{name} door state: " +
            (isOpen ? "Open" : "Closed")
        );

        UpdatePrompt();
    }


    public void OpenForAI()
    {
        if (isOpen)
            return;

        isOpen = true;

        Debug.Log(
            $"{name} opened for Enemy AI."
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject player = GetPlayer(other);

        if (player == null)
            return;

        nearbyPlayers.Add(player);

        UpdatePrompt();
    }

    private void OnTriggerExit(Collider other)
    {
        GameObject player = GetPlayer(other);

        if (player == null)
            return;

        nearbyPlayers.Remove(player);

        UpdatePrompt();
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

    private void UpdatePrompt()
    {
        if (promptCanvas == null)
            return;

        bool shouldShow =
            !isOpen &&
            nearbyPlayers.Count > 0;

        promptCanvas.SetActive(shouldShow);
    }


    public void OpenDoor()
    {
        isOpen = true;
    }

    public void CloseDoor()
    {
        isOpen = false;
    }

    public bool IsOpen()
    {
        return isOpen;
    }
}