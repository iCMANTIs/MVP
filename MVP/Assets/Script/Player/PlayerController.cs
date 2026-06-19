using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Input")]
    public float deadZone = 0.1f;
    public float runStartThreshold = 0.8f;

    [Header("Vault")]
    public float vaultMoveDuration = 0.6f;
    public float vaultArcHeight = 0f;

    [Header("Ground Snap")]
    public float groundCheckHeight = 3f;
    public float groundCheckDistance = 10f;
    public float groundOffset = 0.9f;
    private VaultPoint currentVaultPoint;
    private bool isVaulting;

    private Vector2 moveInput;
    private bool isCrouching;
    private bool HasTorch;
    private bool torchBusy;

    [Header("Flash Stun")]
    public Transform flashOrigin;
    public float stunRange = 8f;
    public float stunRadius = 2f;
    public float stunDuration = 3f;
    public LayerMask enemyLayer;

    private bool holdHandPressed;

    public bool HoldHandPressed => holdHandPressed;
    public Vector2 MoveInput => moveInput;
    public bool IsCrouching => isCrouching;
    public bool IsVaulting => isVaulting;

    public Transform cameraTarget;

    void Awake()
    {
        isCrouching = false;

        if (animator != null)
        {
            animator.SetBool("IsCrouching", false);
            animator.SetFloat("Speed", 0f);
        }
    }

    public void OnHoldHand(InputValue value)
    {
        float triggerValue = value.Get<float>();
        holdHandPressed = triggerValue > 0.5f;
        Debug.Log("Player A Hold Hand Pressed: " + holdHandPressed);
    }


    private bool isHoldingHands;

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
        Debug.Log("Sister(PlayerA) Move = " + moveInput);
        moveInput = value.Get<Vector2>();
    }

    public void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            if (!value.isPressed)
                return;

            if (IsInAction())
                return;

            isCrouching = !isCrouching;
            Debug.Log("Crouch Toggle: " + isCrouching);
        }
    }
    bool IsInAction()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag("Action");
    }

    public void OnTorch(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (IsInAction())
            return;

        if (!HasTorch)
        {
            HasTorch = true;
            animator.SetBool("HasTorch", true);
            animator.SetTrigger("DrawTorch");
        }
        else
        {
            HasTorch = false;
            animator.SetBool("HasTorch", false);
            animator.SetTrigger("HolsterTorch");
        }
    }
    public void OnAttack(InputValue value)
    {
        if (!value.isPressed)
            return;

        if (IsInAction())
            return;

        if (!HasTorch)
            return;

        animator.SetTrigger("TorchAttack");
    }


    public void OnInteract(InputValue value)
    {
        if (!value.isPressed)
            return;

        Debug.Log("Interact Pressed");

        if (IsInAction())
            return;

        if (isVaulting)
            return;

        if (currentVaultPoint == null)
        {
            Debug.Log("No Vault Point");
            return;
        }

        if (!currentVaultPoint.CanVault(transform))
        {
            Debug.Log("Not facing Vault Point");
            return;
        }

        Debug.Log("Start Vault");
        StartCoroutine(VaultRoutine(currentVaultPoint));
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter Trigger: " + other.name);

        VaultPoint vaultPoint = other.GetComponent<VaultPoint>();

        if (vaultPoint != null)
        {
            Debug.Log("Enter Vault Point");
            currentVaultPoint = vaultPoint;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Exit Trigger: " + other.name);

        VaultPoint vaultPoint = other.GetComponent<VaultPoint>();

        if (vaultPoint != null && vaultPoint == currentVaultPoint)
        {
            currentVaultPoint = null;
        }
    }

    Vector3 GetGroundedPosition(Vector3 position)
    {
        Vector3 rayStart = position + Vector3.up * groundCheckHeight;

        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance))
        {
            position.y = hit.point.y + groundOffset;

            Debug.Log("Ground Hit: " + hit.collider.name);
        }
        else
        {
            Debug.Log("No Ground Hit");
        }

        return position;
    }

    private System.Collections.IEnumerator VaultRoutine(VaultPoint vaultPoint)
    {
        isVaulting = true;

        isCrouching = false;
        animator.SetBool("IsCrouching", false);

        animator.SetTrigger("Vault");

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = vaultPoint.endPoint.position;
        endPosition = GetGroundedPosition(endPosition);

        Quaternion endRotation = vaultPoint.endPoint.rotation;

        float timer = 0f;

        while (timer < vaultMoveDuration)
        {
            timer += Time.deltaTime;

            float t = timer / vaultMoveDuration;

            Vector3 currentPosition = Vector3.Lerp(
                startPosition,
                endPosition,
                t
            );

            if (vaultArcHeight > 0f)
            {
                currentPosition.y += Mathf.Sin(t * Mathf.PI) * vaultArcHeight;
            }

            transform.position = currentPosition;

            transform.rotation = Quaternion.Slerp(
                startRotation,
                endRotation,
                t
            );

            yield return null;
        }

        transform.position = GetGroundedPosition(endPosition);
        transform.rotation = endRotation;

        isVaulting = false;
    }

    public void ForceCrouch(bool crouch)
    {
        isCrouching = crouch;
        animator.SetBool("IsCrouching", isCrouching);
    }

    private Vector2 lookInput;
    public Vector2 LookInput => lookInput;

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }


    public void DoFlashStun()
    {
        Collider[] hits = Physics.OverlapSphere(
            flashOrigin.position,
            stunRadius,
            enemyLayer);

        foreach (Collider hit in hits)
        {
            EnemyAI enemy =
                hit.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                enemy.Stun(stunDuration);

                Debug.Log("Flash stunned " + enemy.name);
            }
        }
    }

    void Update()
    {
        //Debug.Log("Sister(PlayerA) Holding = " + isHoldingHands);

        if (isHoldingHands)
            return;

        HandleRotation();
        HandleAnimation();

    }

    void HandleRotation()
    {
        if (isVaulting)
            return;

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

        animator.SetBool("IsCrouching", isCrouching);
        animator.SetFloat("Speed", animSpeed, 0.1f, Time.deltaTime);
        animator.SetBool("HasTorch", HasTorch);
    }


}