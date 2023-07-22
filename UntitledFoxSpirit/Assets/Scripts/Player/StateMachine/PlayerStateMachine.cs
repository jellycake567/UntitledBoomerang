using Cinemachine;
using NUnit.Framework.Internal;
using PathCreation;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
public class PlayerStateMachine : MonoBehaviour
{
    public string debugState;
    public string subState;

    [HideInInspector]
    public VariableScriptObject vso;

    public PlayerBaseState currentState;
    private PlayerStateFactory states;

    [Header("References")]
    public Transform mainCamera;
    public Transform wallCheck;
    public Transform ledgeCheck;
    public Transform ledgeRootJntTransform;
    public PathCreator pathCreator;
    public Slider staminaBar;
    public CinemachineVirtualCamera virtualCam2D;
    public PhysicMaterial friction;

    #region Movement

    // Movement
    [HideInInspector] public float accelRatePerSec, decelRatePerSec, turnSmoothVelocity, currentSpeed, maxSpeed;
    [HideInInspector] public bool isAccel = false, isDecel = false, isRunning = false, disableMovement;


    // Rotation
    [HideInInspector] public float dampedTargetRotationCurrentYVelocity, dampedTargetRotationPassedTime;
    [HideInInspector] public bool disableUpdateRotations = false, disableInputRotations = false;
    [HideInInspector] public Quaternion previousRotation, targetRot2D;

    // Gravity
    [HideInInspector] public bool reduceVelocityOnce = false, disableGravity = false, isGrounded;
    [HideInInspector] public float updateMaxHeight = 100000f, updateMaxHeight2 = 100000f;

    #endregion

    #region Actions

    // Sneaking
    [HideInInspector] public bool canUnsneak = true;
    [HideInInspector] public bool animIsSneaking
    {
        get { return animController.GetBool("isSneaking"); }
        set { animController.SetBool("isSneaking", value); }
    }

    // Jump
    [HideInInspector] public bool isLanding = false, isLandRolling = false, disableJumping = false, canDoubleJump;
    [HideInInspector] public float newGroundY = 1000000f, jumpCounter, jumpBufferCounter, jumpCoyoteCounter;

    // Dash
    [HideInInspector] public float currentDashCooldown = 1f;
    [HideInInspector] public bool disableDashing = false, isDashing = false;
    [HideInInspector] public bool animIsRunning
    {
        get { return animController.GetBool("isRunning"); }
    }

    // BackStep
    [HideInInspector] public bool isBackStep;

    // Ledge Climb
    [HideInInspector] public float currentLedgeHangCooldown;
    [HideInInspector] public bool isClimbing = false, isWallClimbing, canClimbWall, isTouchingWall, isTouchingLedge, canClimbLedge, ledgeDetected;

    // Attack
    [HideInInspector] public bool resetAttack = true, attackAgain = false;
    [HideInInspector] public float currentAttackCooldown;
    [HideInInspector] public int comboCounter, lastAttackInt;
    [HideInInspector] public bool animIsTagAttack
    {
        get { return animController.GetNextAnimatorStateInfo(0).IsTag("Attack"); }
    }
    [HideInInspector] public bool animIsAttacking
    {
        get { return animController.GetBool("isAttacking"); }
        set { animController.SetBool("isAttacking", value); }
    }
    [HideInInspector] public bool animIsAtkTriggered
    {
        get { return animController.GetBool("Attack"); }
    }

    #endregion

    #region Other

    // Stamina
    [HideInInspector] public float currentStaminaCooldown, currentStamina;

    // Take Damage
    [HideInInspector] public bool isInvulnerable = false;
    [HideInInspector] public float currentInvulnerableCooldown;

    // Path
    [HideInInspector] public float distanceOnPath;

    // References
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator animController;
    [HideInInspector] public CapsuleCollider tallCollider;
    [HideInInspector] public CapsuleCollider shortCollider;
    [HideInInspector] public BoxCollider boxCollider;
    [HideInInspector] public PlayerInput input;

    #endregion

    #region Internal Variables

     public Vector3 prevInputDirection;
    [HideInInspector] public bool isHeavyLand = false;

    // Debug
    [HideInInspector] public float currentMaxHeight = 0f;
    [HideInInspector] public Vector3 velocity;

    #endregion

    #region Unity Functions

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animController = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        input = GetComponent<PlayerInput>();

        CapsuleCollider[] colliderArr = GetComponentsInChildren<CapsuleCollider>();
        tallCollider = colliderArr[0];
        shortCollider = colliderArr[1];

        currentStamina = vso.maxStamina;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;


        Vector3 spawnPos = pathCreator.path.GetPointAtDistance(vso.distanceSpawn);
        spawnPos.y += vso.spawnYOffset + 1.0f;
        transform.position = spawnPos;
        distanceOnPath = pathCreator.path.GetClosestDistanceAlongPath(transform.position);


        states = new PlayerStateFactory(this, vso);
        currentState = new PlayerGroundedState(this, states, vso);
        currentState.EnterState();
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.velocity.y > 0)
        {
            currentMaxHeight = transform.position.y;
        }

        currentState.UpdateStates();
        if (currentState.currentSubState != null)
        {
            subState = currentState.currentSubState.ToString();
        }
        debugState = currentState.ToString();

        //Debug.Log(subState);

        StoreInputMovement();

        // Rotation
        HandleRotation();
        CameraRotation();

        // Jump
        JumpCooldownTimer();
        CoytoteTime();
        JumpBuffer();

        // Attack
        AttackCooldown();

        // Dash
        DashCooldown();

        // Ledge Hang
        LedgeClimbCooldown();

        // Ground
        GroundCheck();
    }

    void FixedUpdate()
    {
        currentState.FixedUpdateStates();
    }

    void OnAnimatorMove()
    {
        
        
        // Attacking root motion
        if (animIsAttacking && !disableDashing && !animController.IsInTransition(0))
        {
            float y = rb.velocity.y;
        
            rb.velocity = animController.deltaPosition * vso.rootMotionAtkSpeed / Time.deltaTime;
        
            rb.velocity = new Vector3(rb.velocity.x, y, rb.velocity.z);
        }

        currentState.OnAnimatorMoveStates();
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.identity;

        // Spawn player position
        if (vso.distanceSpawn >= 0 && vso.distanceSpawn <= pathCreator.path.length)
        {
            Vector3 spawnPosition = pathCreator.path.GetPointAtDistance(vso.distanceSpawn);
            spawnPosition.y += vso.spawnYOffset;

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(spawnPosition, 0.5f);
        }

        // Player ground check
        Vector3 point = new Vector3(transform.position.x + vso.groundCheckOffset.x, transform.position.y + vso.groundCheckOffset.y, transform.position.z + vso.groundCheckOffset.z) + Vector3.down;
        Gizmos.matrix = Matrix4x4.TRS(point, transform.rotation, transform.lossyScale);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, vso.groundCheckSize);

        // Player head check
        Vector3 centerPos = new Vector3(transform.position.x + vso.sneakCheckOffset.x, transform.position.y + vso.sneakCheckOffset.y, transform.position.z + vso.sneakCheckOffset.z) + Vector3.up;
        Gizmos.matrix = Matrix4x4.TRS(centerPos, transform.rotation, transform.lossyScale);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, vso.sneakCheckSize);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hitbox"))
        {
            EnemyNavigation enemyNav = other.GetComponentInParent<EnemyNavigation>();
            Vector3 enemyPos = enemyNav.transform.position;

            // Get dir from AI to player
            Vector3 facingDir = (other.ClosestPointOnBounds(transform.position) - enemyPos).IgnoreYAxis();
            Vector3 dir = enemyNav.CalculatePathFacingDir(enemyPos, facingDir);

            //TakeDamage(dir);
        }
    }

    #endregion

    
    void StoreInputMovement()
    {
        // Store when player presses left or right when moving
        Vector3 direction = input.GetMovementInput.normalized;
        if (prevInputDirection != direction && input.isMovementHeld)
        {
            // Reset speed when turning around in the air
            if (!isGrounded)
                currentSpeed = vso.reduceSpeed;

            prevInputDirection = direction;
        }
    }

    #region Rotation

    void HandleRotation()
    {
        // Calculate player 2D rotation
        distanceOnPath = pathCreator.path.GetClosestDistanceAlongPath(transform.position);

        targetRot2D = Rotation2D(GetPathRotation(), input.GetMovementInput.normalized);
    }

    // Determine to update current or last input
    Quaternion Rotation2D(Quaternion targetRot2D, Vector3 direction)
    {
        if (disableUpdateRotations)
            return targetRot2D;


        // Flipping direction
        if (prevInputDirection.x < 0f)
        {
            targetRot2D = Flip(targetRot2D);
        }

        // Don't allow new inputs, but allow last input to update rotation
        if (disableInputRotations)
            UpdateRotation(previousRotation);
        else
            UpdateRotation(targetRot2D);

        Debug.DrawRay(transform.position, previousRotation * Vector3.forward);

        if (input.isMovementHeld)
        {
            if (previousRotation != targetRot2D)
            {
                dampedTargetRotationPassedTime = 0f;
            }

            if (disableInputRotations)
                return targetRot2D;

            // Saved for deceleration
            previousRotation = targetRot2D;
        }

        return targetRot2D;
    }

    public Quaternion Flip(Quaternion targetRot)
    {
        Vector3 rot = targetRot.eulerAngles;
        return Quaternion.Euler(rot.x, rot.y + 180f, rot.z);
    }

    void UpdateRotation(Quaternion targetRot2D)
    {
        float currentYAngle = rb.rotation.eulerAngles.y;
        if (currentYAngle == previousRotation.eulerAngles.y)
        {
            return;
        }

        float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, targetRot2D.eulerAngles.y, ref dampedTargetRotationCurrentYVelocity, vso.timeToReachTargetRotation - dampedTargetRotationPassedTime);
        dampedTargetRotationPassedTime += Time.deltaTime;

        Quaternion targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);
        rb.MoveRotation(targetRotation);
    }

    void CameraRotation()
    {
        // Rotate camera 2d
        Vector3 camEulerAngle = mainCamera.rotation.eulerAngles;
        virtualCam2D.transform.rotation = Quaternion.Slerp(mainCamera.rotation, Quaternion.Euler(camEulerAngle.x, GetPathRotation().eulerAngles.y - 90f, camEulerAngle.z), vso.camRotationSpeed2D);
    }

    #endregion

    #region Jump

    void JumpCooldownTimer()
    {
        if (jumpCounter > 0f)
        {
            jumpCounter -= Time.deltaTime;
        }
        else
        {
            animController.SetBool("Jump", false);
        }
    }

    void CoytoteTime()
    {
        // Coyote Time
        if (isGrounded)
        {
            canDoubleJump = true;

            jumpCoyoteCounter = vso.jumpCoyoteTime;
        }
        else
        {
            jumpCoyoteCounter -= Time.deltaTime;
        }
    }

    void JumpBuffer()
    {
        if (input.isInputJumpPressed)
        {
            jumpBufferCounter = vso.jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    #endregion

    void AttackCooldown()
    {
        if (currentAttackCooldown > 0f)
        {
            currentAttackCooldown -= Time.deltaTime;
        }
    }

    void DashCooldown()
    {
        if (currentDashCooldown > 0f)
        {
            currentDashCooldown -= Time.deltaTime;
        }
    }

    void LedgeClimbCooldown()
    {
        if (currentLedgeHangCooldown > 0f)
        {
            currentLedgeHangCooldown -= Time.deltaTime;
        }
    }

    void GroundCheck()
    {
        Vector3 centerPos = new Vector3(transform.position.x + vso.groundCheckOffset.x, transform.position.y + vso.groundCheckOffset.y, transform.position.z + vso.groundCheckOffset.z) + Vector3.down;

        bool overlap = Physics.CheckBox(centerPos, vso.groundCheckSize / 2, transform.rotation, ~vso.ignorePlayerMask);

        RaycastHit hit;
        if (Physics.Raycast(centerPos, Vector3.down, out hit, 100f, ~vso.ignorePlayerMask))
        {
            newGroundY = hit.point.y;
        }

        if (overlap)
        {
            isGrounded = true;
            animController.SetBool("Grounded", true);

            if (rb.velocity.y <= 1f)
                animController.SetBool("Fall", false);
        }
        else
        {
            isGrounded = false;
            animController.SetBool("Grounded", false);
        }
    }

    

    public Quaternion GetPathRotation()
    {
        return pathCreator.path.GetRotationAtDistance(distanceOnPath, EndOfPathInstruction.Stop);
    }

    public void HeavyLandFinish()
    {
        isHeavyLand = false;
    }

    void FinishLedgeClimb()
    {
        Transform rootJointTransform = ledgeRootJntTransform;
        rootJointTransform.position = new Vector3(rootJointTransform.position.x, rootJointTransform.position.y - 0.65f, rootJointTransform.position.z);
        transform.position = rootJointTransform.position;
        ledgeRootJntTransform.localPosition = ledgeRootJntTransform.localPosition;

        canClimbLedge = false;
    }

    void FinishBackStep()
    {
        isBackStep = false;
    }

}
