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

    private Vector2 moveInput;

    private bool isArmed;
    private bool isCrouching;

    void Awake()
    {
        isArmed = false;
        isCrouching = false;

        animator.SetBool("Is Armed", false);
        animator.SetBool("Is Crouching", false);
        animator.SetFloat("Speed", 0f);
    }

    void Update()
    {
        HandleRotation();
        HandleAnimation();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnArm(InputValue value)
    {
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

    void HandleRotation()
    {
        if (IsInAction())
            return;

        float inputAmount = Mathf.Clamp01(moveInput.magnitude);

        if (inputAmount < deadZone)
            return;

        Vector3 moveDirection = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        ).normalized;

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