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

    private Quaternion closedRotation;
    private Quaternion openRotation;

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