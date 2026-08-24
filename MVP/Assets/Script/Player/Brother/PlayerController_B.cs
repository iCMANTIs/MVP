using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController_B : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Input")]
    public float deadZone = 0.1f;
    public float runStartThreshold = 0.8f;

    [Header("Interaction")]
    [SerializeField] private PlayerInteractor playerInteractor;

    private Vector2 moveInput;

    private bool isArmed;
    private bool isCrouching;

    private bool holdHandPressed;
    public bool HoldHandPressed => holdHandPressed;
    public Vector2 MoveInput => moveInput;
    public bool IsCrouching => isCrouching;

    public Transform cameraTarget;

    public bool canControl = true;
    void Awake()
    {
        isArmed = false;
        isCrouching = false;

        animator.SetBool("Is Armed", false);
        animator.SetBool("Is Crouching", false);
        animator.SetFloat("Speed", 0f);

        if (playerInteractor == null)
            playerInteractor = GetComponent<PlayerInteractor>();
    }

    void Update()
    {
        if (!canControl)
        {
            animator.SetFloat("Speed", 0);
            return;
        }
        if (isHoldingHands)
            return;
        HandleRotation();
        HandleAnimation();
    }


    private bool isHoldingHands;

    public void OnHoldHand(InputValue value)
    {
        if (!canControl)
            return;
        float triggerValue = value.Get<float>();
        holdHandPressed = triggerValue > 0.5f;
        Debug.Log("Player B Hold Hand Pressed: " + holdHandPressed);
    }

    public void SetHoldingHands(bool holding)
    {
        isHoldingHands = holding;
        animator.SetBool("IsHoldingHands", holding);

        animator.applyRootMotion = !holding;
    }

    public void SetExternalMove(Vector2 externalMove, float sharedSpeed)
    {
        moveInput = externalMove;
        animator.SetFloat("Speed", sharedSpeed, 0.1f, Time.deltaTime);
    }


    public void OnMove(InputValue value)
    {
        if (!canControl)
            return;
        moveInput = value.Get<Vector2>();
    }

    public void OnArm(InputValue value)
    {
        if (!canControl)
            return;
        if (!value.isPressed)
            return;

        if (IsInAction())
            return;

        if (isCrouching)
            return;

        if (!isArmed)
        {
            isArmed = true;
            animator.SetBool("Is Armed", true);
            animator.SetTrigger("Arm");
        }
        else
        {
            isArmed = false;
            animator.SetBool("Is Armed", false);
            animator.SetTrigger("Disarm");
        }
    }

    public void OnCrouch(InputValue value)
    {
        if (!canControl)
            return;
        if (!value.isPressed)
            return;

        if (IsInAction())
            return;

        if (!isCrouching)
        {
            isCrouching = true;
            animator.SetBool("Is Crouching", true);

            if (isArmed)
            {
                isArmed = false;
                animator.SetBool("Is Armed", false);
                animator.SetTrigger("Disarm");
            }
        }
        else
        {
            isCrouching = false;
            animator.SetBool("Is Crouching", false);
        }
    }

    public void OnAttack(InputValue value)
    {
        if (!canControl)
            return;
        if (!value.isPressed)
            return;

        if (IsInAction())
            return;

        if (!isArmed)
            return;

        if (isCrouching)
            return;

        animator.SetTrigger("Attack");
    }

    public void OnEcholocation(InputValue value)
    {
        if (!canControl)
            return;
        if (!value.isPressed)
            return;

        if (IsInAction())
            return;

        if (!isArmed)
            return;

        if (isCrouching)
            return;

        animator.SetTrigger("Echolocation");
    }

    public void ForceCrouch(bool crouch)
    {
        isCrouching = crouch;
        animator.SetBool("Is Crouching", isCrouching);

        if (isCrouching && isArmed)
        {
            isArmed = false;
            animator.SetBool("Is Armed", false);
            animator.SetTrigger("Disarm");
        }
    }

    private Vector2 lookInput;
    public Vector2 LookInput => lookInput;

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnInteract(InputValue value)
    {
        PlayerHealth health = GetComponent<PlayerHealth>();

        if (health != null &&
            health.CanReviveTeammate())
        {
            health.SetReviveInput(value.isPressed);
            return;
        }

        if (health != null)
        {
            health.SetReviveInput(false);
        }

        if (!canControl)
            return;
        if (!value.isPressed)
            return;

        if (playerInteractor != null)
            playerInteractor.TryInteract();
    }

    void HandleRotation()
    {
        if (IsInAction())
            return;

        float inputAmount = Mathf.Clamp01(moveInput.magnitude);

        if (inputAmount < deadZone)
            return;

        Vector3 cameraForward = cameraTarget.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = cameraTarget.right;
        cameraRight.y = 0f;
        cameraRight.Normalize();

        Vector3 moveDirection =
            cameraForward * moveInput.y +
            cameraRight * moveInput.x;

        moveDirection.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void HandleAnimation()
    {
        float inputAmount = Mathf.Clamp01(moveInput.magnitude);

        float animSpeed = 0f;

        if (inputAmount >= deadZone)
        {
            if (isCrouching)
            {
                animSpeed = 1f;
            }
            else
            {
                if (inputAmount < runStartThreshold)
                {
                    animSpeed = Mathf.Lerp(
                        0f,
                        1f,
                        inputAmount / runStartThreshold
                    );
                }
                else
                {
                    animSpeed = Mathf.Lerp(
                        1f,
                        2f,
                        (inputAmount - runStartThreshold) / (1f - runStartThreshold)
                    );
                }
            }
        }

        animator.SetBool("Is Armed", isArmed);
        animator.SetBool("Is Crouching", isCrouching);
        animator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);
    }

    bool IsInAction()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag("Action");
    }
}