using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HandHoldSystem : MonoBehaviour
{
    [Header("Players")]
    public PlayerController playerA;       // Sister
    public PlayerController_B playerB;     // Brother

    [Header("Distance")]
    public float startHoldDistance = 1.5f;
    public float breakDistance = 2.2f;
    public float holdSpacing = 0.85f;

    [Header("Movement")]
    public float holdWalkSpeed = 1.8f;
    public float holdCrouchSpeed = 0.9f;
    public float rotationSpeed = 10f;
    public float deadZone = 0.1f;

    [Header("Direction Match")]
    public float sameDirectionDot = 0.6f;
    public float breakDirectionDot = -0.3f;

    [Header("Snap Align")]
    public bool snapOnStart = true;
    public float snapDuration = 0.08f;

    [Header("IK Targets")]
    public Transform playerAHandTarget;
    public Transform playerBHandTarget;

    [Header("IK Constraints")]
    public TwoBoneIKConstraint playerAHandIK;
    public TwoBoneIKConstraint playerBHandIK;

    [Header("IK Settings")]
    public float standingHandHeight = 1.05f;
    public float crouchHandHeight = 0.75f;
    public float handOffset = 0.025f;
    public float ikBlendSpeed = 8f;

    [Header("Tutorial")]
    [SerializeField]
    private TutorialManager tutorialManager;

    private bool isHoldingHands;
    private bool isSnapping;
    private float snapTimer;
    private float ikWeight;
    private Vector3 holdCenter;
    private Vector3 holdForwardDir;
    private Vector3 holdSideDir;

    private Vector3 snapAStart;
    private Vector3 snapBStart;
    private Vector3 snapAEnd;
    private Vector3 snapBEnd;

    private Quaternion snapARotStart;
    private Quaternion snapBRotStart;
    private Quaternion snapRotEnd;

    public bool IsHoldingHands => isHoldingHands;

    [Header("Shared Camera Movement")]
    public Transform sharedCameraTarget;
    public float minHoldMoveAmount = 0.6f;

    [Header("Hold Speed Fix")]
    public float holdSpeedMultiplier = 2.5f;

    void Update()
    {
        CheckHoldInput();

        if (isSnapping)
        {
            UpdateSnapAlign();
        }
        else if (isHoldingHands)
        {
            HandleSharedCrouch();
            HandleHoldingMovement();
            MaintainHandSpacing();
            UpdateHandTargets();
        }

        UpdateIKWeight();
    }

    void CheckHoldInput()
    {
        float distance = Vector3.Distance(
            playerA.transform.position,
            playerB.transform.position
        );

        bool bothPressing =
            playerA.HoldHandPressed &&
            playerB.HoldHandPressed;

        if (!isHoldingHands)
        {
            if (bothPressing && distance <= startHoldDistance)
            {
                StartHoldingHands();
            }
        }
        else
        {
            if (!bothPressing || distance > breakDistance)
            {
                StopHoldingHands();
            }
        }
    }

    void StartHoldingHands()
    {
        isHoldingHands = true;

        playerA.SetHoldingHands(true);
        playerB.SetHoldingHands(true);

        if (snapOnStart)
            StartSnapAlign();
        else
            SnapAlignInstant();

        if (tutorialManager != null)
            tutorialManager.CompleteHandHoldObjective();

        Debug.Log("Start Holding Hands");
    }

    void StopHoldingHands()
    {
        isHoldingHands = false;
        isSnapping = false;

        playerA.SetHoldingHands(false);
        playerB.SetHoldingHands(false);

        playerA.SetExternalMove(Vector2.zero, 0f);
        playerB.SetExternalMove(Vector2.zero, 0f);

        Debug.Log("Stop Holding Hands");
    }

    void StartSnapAlign()
    {
        isSnapping = true;
        snapTimer = 0f;

        CalculateSnapTargets();

        snapAStart = playerA.transform.position;
        snapBStart = playerB.transform.position;

        snapARotStart = playerA.transform.rotation;
        snapBRotStart = playerB.transform.rotation;
    }

    void UpdateSnapAlign()
    {
        snapTimer += Time.deltaTime;
        float t = Mathf.Clamp01(snapTimer / snapDuration);

        playerA.transform.position = Vector3.Lerp(snapAStart, snapAEnd, t);
        playerB.transform.position = Vector3.Lerp(snapBStart, snapBEnd, t);

        playerA.transform.rotation = Quaternion.Slerp(snapARotStart, snapRotEnd, t);
        playerB.transform.rotation = Quaternion.Slerp(snapBRotStart, snapRotEnd, t);

        UpdateHandTargets();

        if (t >= 1f)
        {
            isSnapping = false;
        }
    }

    void SnapAlignInstant()
    {
        CalculateSnapTargets();

        playerA.transform.position = snapAEnd;
        playerB.transform.position = snapBEnd;

        playerA.transform.rotation = snapRotEnd;
        playerB.transform.rotation = snapRotEnd;

        UpdateHandTargets();
    }

    void CalculateSnapTargets()
    {
        Vector3 aPos = playerA.transform.position;
        Vector3 bPos = playerB.transform.position;

        holdCenter = (aPos + bPos) * 0.5f;

        holdSideDir = bPos - aPos;
        holdSideDir.y = 0f;

        if (holdSideDir.sqrMagnitude < 0.001f)
            holdSideDir = playerA.transform.right;
        else
            holdSideDir.Normalize();

        holdForwardDir = playerA.transform.forward + playerB.transform.forward;
        holdForwardDir.y = 0f;

        if (holdForwardDir.sqrMagnitude < 0.001f)
            holdForwardDir = Vector3.Cross(holdSideDir, Vector3.up);

        holdForwardDir.Normalize();

        snapAEnd = holdCenter - holdSideDir * holdSpacing * 0.5f;
        snapBEnd = holdCenter + holdSideDir * holdSpacing * 0.5f;

        snapRotEnd = Quaternion.LookRotation(holdForwardDir, Vector3.up);
    }

    void HandleSharedCrouch()
    {
        bool sharedCrouch = playerA.IsCrouching || playerB.IsCrouching;

        playerA.ForceCrouch(sharedCrouch);
        playerB.ForceCrouch(sharedCrouch);
    }

    void HandleHoldingMovement()
    {
        Vector2 inputA = playerA.MoveInput;
        Vector2 inputB = playerB.MoveInput;

        float amountA = Mathf.Clamp01(inputA.magnitude);
        float amountB = Mathf.Clamp01(inputB.magnitude);

        bool aMoving = amountA >= deadZone;
        bool bMoving = amountB >= deadZone;

        //playerA.SetExternalMove(Vector2.zero, 0f);
        //playerB.SetExternalMove(Vector2.zero, 0f);

        // both stay
        if (!aMoving && !bMoving)
        {
            playerA.SetExternalMove(Vector2.zero, 0f);
            playerB.SetExternalMove(Vector2.zero, 0f);
            return;
        }

        // one move
        if (aMoving != bMoving)
        {
            playerA.SetExternalMove(Vector2.zero, 0f);
            playerB.SetExternalMove(Vector2.zero, 0f);
            return;
        }

        Vector2 dirA = inputA.normalized;
        Vector2 dirB = inputB.normalized;

        float dot = Vector2.Dot(dirA, dirB);

        // dif
        if (dot < breakDirectionDot)
        {
            StopHoldingHands();
            return;
        }

        // gap
        if (dot < sameDirectionDot)
        {
            playerA.SetExternalMove(Vector2.zero, 0f);
            playerB.SetExternalMove(Vector2.zero, 0f);
            return;
        }


        Vector2 sharedInput = (dirA + dirB).normalized;

        float inputAmount =
            Mathf.Clamp01((amountA + amountB) * 0.5f);

        inputAmount =
            Mathf.Max(inputAmount, minHoldMoveAmount);


        Vector3 moveDirection = GetCameraRelativeMoveDirection(sharedInput);

        bool isCrouching = playerA.IsCrouching || playerB.IsCrouching;
        float speed = isCrouching ? holdCrouchSpeed : holdWalkSpeed;

        Vector3 movement = moveDirection * speed * holdSpeedMultiplier * inputAmount * Time.deltaTime;

        holdCenter += movement;

        /*holdForwardDir = Vector3.Slerp(
            holdForwardDir,
            moveDirection,
            rotationSpeed * Time.deltaTime
        );

        holdForwardDir.y = 0f;
        holdForwardDir.Normalize();

        holdSideDir = Vector3.Cross(Vector3.up, holdForwardDir).normalized;*/

        ApplyHoldFormation();

        float animSpeed = isCrouching ? 1f : inputAmount;

        playerA.SetExternalMove(sharedInput, animSpeed);
        playerB.SetExternalMove(sharedInput, animSpeed);
    }

    Vector3 GetCameraRelativeMoveDirection(Vector2 input)
    {
        Transform reference = sharedCameraTarget;

        Vector3 forward = reference.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = reference.right;
        right.y = 0f;
        right.Normalize();

        Vector3 dir =
            forward * input.y +
            right * input.x;

        if (dir.sqrMagnitude < 0.001f)
            return holdForwardDir;

        return dir.normalized;
    }

    void ApplyHoldFormation()
    {
        Vector3 aTarget =
            holdCenter - holdSideDir * holdSpacing * 0.5f;

        Vector3 bTarget =
            holdCenter + holdSideDir * holdSpacing * 0.5f;

        playerA.transform.position = aTarget;
        playerB.transform.position = bTarget;

        Quaternion targetRotation =
            Quaternion.LookRotation(holdForwardDir, Vector3.up);

        playerA.transform.rotation = targetRotation;
        playerB.transform.rotation = targetRotation;
    }

    void RotateBoth(Vector3 moveDirection)
    {
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        playerA.transform.rotation = Quaternion.Slerp(
            playerA.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        playerB.transform.rotation = Quaternion.Slerp(
            playerB.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void MaintainHandSpacing()
    {
        ApplyHoldFormation();
        /*Vector3 aPos = playerA.transform.position;
        Vector3 bPos = playerB.transform.position;

        Vector3 center = (aPos + bPos) * 0.5f;

        Vector3 sideDir = bPos - aPos;
        sideDir.y = 0f;

        if (sideDir.sqrMagnitude < 0.001f)
            sideDir = playerA.transform.right;
        else
            sideDir.Normalize();

        playerA.transform.position =
            center - sideDir * holdSpacing * 0.5f;

        playerB.transform.position =
            center + sideDir * holdSpacing * 0.5f;*/
    }

    void UpdateHandTargets()
    {
        if (playerAHandTarget == null || playerBHandTarget == null)
            return;

        bool isCrouching = playerA.IsCrouching || playerB.IsCrouching;
        float height = isCrouching ? crouchHandHeight : standingHandHeight;

        Vector3 handCenter =
            holdCenter + Vector3.up * height;

        playerAHandTarget.position =
            handCenter + holdSideDir * handOffset;

        playerBHandTarget.position =
            handCenter - holdSideDir * handOffset;
        /*if (playerAHandTarget == null || playerBHandTarget == null)
            return;

        Vector3 aPos = playerA.transform.position;
        Vector3 bPos = playerB.transform.position;

        Vector3 center = (aPos + bPos) * 0.5f;

        Vector3 sideDir = bPos - aPos;
        sideDir.y = 0f;

        if (sideDir.sqrMagnitude < 0.001f)
            sideDir = playerA.transform.right;
        else
            sideDir.Normalize();

        bool isCrouching = playerA.IsCrouching || playerB.IsCrouching;
        float height = isCrouching ? crouchHandHeight : standingHandHeight;

        Vector3 handCenter = center + Vector3.up * height;

        playerAHandTarget.position =
            handCenter + sideDir * handOffset;

        playerBHandTarget.position =
            handCenter - sideDir * handOffset;*/
    }

    void UpdateIKWeight()
    {
        float targetWeight = isHoldingHands ? 1f : 0f;

        ikWeight = Mathf.MoveTowards(
            ikWeight,
            targetWeight,
            ikBlendSpeed * Time.deltaTime
        );

        if (playerAHandIK != null)
            playerAHandIK.weight = ikWeight;

        if (playerBHandIK != null)
            playerBHandIK.weight = ikWeight;
    }
}