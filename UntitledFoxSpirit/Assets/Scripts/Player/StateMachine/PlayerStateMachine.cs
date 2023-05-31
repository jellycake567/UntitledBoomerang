using Cinemachine;
using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateMachine : MonoBehaviour
{
    protected PlayerBaseState currentState;
    PlayerStateFactory states;


    #region Movement / Rotation

    [Header("Movement")]
    public float humanSpeed = 5.0f;
    public float humanRunSpeed = 10.0f;
    public float accelTimeToMaxSpeed = 2.0f;
    public float decelTimeToZeroSpeed = 1.0f;
    public float animJogSpeed = 1.17f;
    public float animJogAccelSpeed = 0.8f;
    public float animJogDecelSpeed = 0.8f;
    protected float accelRatePerSec;
    protected float decelRatePerSec;
    protected bool isAccel = false;
    protected bool isDecel = false;
    protected bool isRunning = false;
    protected bool disableMovement;

    public float turnSmoothTime2D = 0.03f;
    public float turnSmoothTime3D = 0.1f;
    public float maxVelocityChange = 10f;
    public float frictionAmount = 0.2f;
    protected float turnSmoothVelocity;
    protected float currentSpeed;
    protected float maxSpeed;

    [Header("Rotation")]
    public float timeToReachTargetRotation = 0.14f;
    protected float dampedTargetRotationCurrentYVelocity;
    protected float dampedTargetRotationPassedTime;
    protected bool disableUpdateRotations = false;
    protected bool disableInputRotations = false;
    protected Quaternion previousRotation;
    protected Quaternion targetRot2D;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float gravityScale = 3f;
    public float fallGravityMultiplier = 0.2f;
    public float reduceVelocityPeak = 5f;
    public float reduceVelocity = 5f;
    public Vector3 groundCheckOffset;
    public Vector3 groundCheckSize;
    public LayerMask ignorePlayerMask;
    protected bool reduceVelocityOnce = false;
    protected bool isGrounded;
    protected float updateMaxHeight = 100000f;
    protected float updateMaxHeight2 = 100000f;
    protected bool disableGravity = false;

    #endregion

    #region Player Actions

    [Header("Jump")]
    public float humanJumpHeight = 5f;
    public float jumpRollVelocity = -5f;
    public float rootMotionJumpRollSpeed = 2f;
    protected bool isLanding = false;
    protected bool isLandRolling = false;
    protected bool disableJumping = false;
    protected bool isHoldingJump = false;

    public float jumpCooldown = 0.2f;
    public float jumpBufferTime = 0.1f;   // Detect jump input before touching the ground
    public float jumpCoyoteTime = 0.2f;   // Allow you to jump when you walk off platform
    public float jumpMultiplier = 1.0f;
    public float doubleJumpHeightPercent = 0.5f;
    protected float jumpCounter;
    protected float jumpBufferCounter;
    protected float jumpCoyoteCounter;
    protected bool canDoubleJump;
    protected float newGroundY = 1000000f;

    [Header("Sneaking")]
    public float sneakSpeed = 2f;
    public Vector3 sneakCheckOffset;
    public Vector3 sneakCheckSize;
    protected bool canUnsneak = true;
    protected bool isSneaking
    {
        get { return animController.GetBool("isSneaking"); }
        set { animController.SetBool("isSneaking", value); }
    }

    [Header("Dash")]
    public float humanDashTime = 5.0f;
    public float humanDashDistance = 4.0f;
    public float dashCooldown = 1f;
    protected float currentDashCooldown = 1f;
    protected bool isDashing = false;
    protected bool disableDashing = false;

    [Header("Ledge Climb")]
    public float wallCheckDistance = 3.0f;
    public float ledgeHangDistanceOffset;
    public float ledgeHangYOffset;
    public float distanceFromGround = 1f;
    public float ledgeHangCooldown = 1f;
    public Transform wallCheck;
    public Transform ledgeCheck;
    public Transform ledgeRootJntTransform;
    public LayerMask groundLayer;
    protected float currentLedgeHangCooldown;
    protected bool isClimbing = false;
    protected bool isWallClimbing;
    protected bool canClimbWall;

    protected bool isTouchingWall;
    protected bool isTouchingLedge;
    [HideInInspector] public bool canClimbLedge = false;
    [HideInInspector] public bool ledgeDetected;

    [Header("Attack")]
    public float attackCooldown = 2f;
    public float resetComboDelay = 1f;
    public float rootMotionAtkSpeed = 2f;
    protected bool isAttacking = false;
    protected float currentAttackCooldown;
    protected int comboCounter;
    protected int lastAttackInt;
    protected bool resetAttack = true;

    #endregion

    #region Other

    [Header("Stamina")]
    public float staminaConsumption = 20f;
    public float staminaRecovery = 5f;
    public float staminaCooldown = 1f;
    public float maxStamina = 100f;
    protected float currentStaminaCooldown = 0f;
    protected float currentStamina;


    [Header("Take Damage")]
    public float invulnerableTime = 1f;
    public float regainMovement = 0.5f;
    public float horizontalKnockback = 10f;
    public float verticalKnockback = 10f;
    [HideInInspector] public bool isInvulnerable = false;
    protected float currentInvulnerableCooldown;

    [Header("Camera")]
    public float camRotationSpeed2D = 0.2f;
    public Transform mainCamera;

    [Header("Path")]
    public PathCreator pathCreator;
    public float maxDistancePath = 0.5f;
    public float distanceSpawn = 0f;
    public float spawnYOffset = 0f;
    public float adjustVelocity = 1.0f; //Velocity to push player towards the path
    protected float distanceOnPath;

    [Header("References")]
    public Slider staminaBar;
    public CinemachineVirtualCamera virtualCam2D;
    public PhysicMaterial friction;
    protected Rigidbody rb;
    protected Animator animController;
    protected CapsuleCollider tallCollider;
    protected CapsuleCollider shortCollider;
    protected BoxCollider boxCollider;
    protected PlayerInput input;

    #endregion

    #region Internal Variables

    protected Vector3 prevInputDirection;

    protected bool isHeavyLand = false;

    const float REDUCE_SPEED = 1.414214f;
    
    

    // Debug
    float currentMaxHeight = 0f;
    protected Vector3 velocity;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        states = new PlayerStateFactory(this);
        currentState = states.Grounded();
        currentState.EnterState();

        rb = GetComponent<Rigidbody>();
        animController = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();
        input = GetComponent<PlayerInput>();

        CapsuleCollider[] colliderArr = GetComponentsInChildren<CapsuleCollider>();
        tallCollider = colliderArr[0];
        shortCollider = colliderArr[1];

        currentStamina = maxStamina;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;


        Vector3 spawnPos = pathCreator.path.GetPointAtDistance(distanceSpawn);
        spawnPos.y += spawnYOffset + 1.0f;
        transform.position = spawnPos;

        distanceOnPath = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        currentState.UpdateState();

        // Rotation
        HandleRotation();
        CameraRotation();

        // Jump
        JumpCooldownTimer();
        CoytoteTime();

        GroundCheck();
    }

    void FixedUpdate()
    {
        currentState.FixedUpdateState();
    }

    void JumpCooldownTimer()
    {
        if (jumpCounter <= 0f)
        {
            animController.SetBool("Jump", false);

            jumpCounter -= Time.deltaTime;
        }
    }

    void CoytoteTime()
    {
        // Coyote Time
        if (isGrounded)
        {
            canDoubleJump = true;

            jumpCoyoteCounter = jumpCoyoteTime;
        }
        else
        {
            jumpCoyoteCounter -= Time.deltaTime;
        }
    }


    void GroundCheck()
    {
        Vector3 centerPos = new Vector3(transform.position.x + groundCheckOffset.x, transform.position.y + groundCheckOffset.y, transform.position.z + groundCheckOffset.z) + Vector3.down;
        //Vector3 size = isFox ? new Vector3(0.9f, 0.1f, 1.9f) : new Vector3(0.8f, 0.1f, 0.8f);

        bool overlap = Physics.CheckBox(centerPos, groundCheckSize / 2, transform.rotation, ~ignorePlayerMask);

        RaycastHit hit;
        if (Physics.Raycast(centerPos, Vector3.down, out hit, 100f, ~ignorePlayerMask))
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


    #region Animation Jog Speed

    void DetectAnimAcceleration(Vector3 targetVelocity, Vector3 direction)
    {
        #region Detect animation player input
        if (direction.magnitude > 0.1f)
        {
            // Currently decelerating, but if input is pressed, stop decel and start accel
            if (!isAccel && isDecel)
                isDecel = false;

            if (!disableMovement)
                animController.SetBool("isMoving", true);

            // Store when player presses left or right
            if (prevInputDirection != direction)
            {
                // Reset speed when turning around
                currentSpeed = 2f;
                prevInputDirection = direction;
            }
        }
        else
        {
            if (isDashing || animController.GetBool("isSneaking"))
            {
                animController.SetBool("isMoving", false);
            }
            animController.SetBool("isSprinting", false);

            // If currently accelerating, but input is released, stop accel and start decel
            if (isAccel)
            {
                isAccel = false;
                isDecel = true;

                animController.speed = animJogDecelSpeed;
                StartCoroutine(Deceleration());
            }
        }
        #endregion

        #region Acceleraction / Deceleration Animation

        // Is player currently in jogging state
        if (!animController.GetCurrentAnimatorStateInfo(0).IsTag("Run"))
        {
            animController.speed = 1f;
            return;
        }

        // Start acceleration when entering state
        if (!isAccel && !isDecel)
        {
            isAccel = true;
            animController.speed = animJogAccelSpeed; // Set to accel jogging speed
        }
        else if (currentSpeed >= maxSpeed && !isDecel) // If reached max speed, set anim speed to normal jogging speed
        {
            animController.speed = animJogSpeed;
        }



        #endregion
    }

    IEnumerator Deceleration()
    {
        float time = 0f;
        float timeToZero = decelTimeToZeroSpeed * currentSpeed;

        // Waiting for deceleration to reach zero (Match decel anim with player movement)
        while (time < timeToZero)
        {
            time += -decelRatePerSec * Time.deltaTime;

            if (!isDecel)
                break;

            yield return new WaitForEndOfFrame();
        }

        if (isDecel)
            animController.SetBool("isMoving", false);
    }

    #endregion

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
            Vector3 rot = targetRot2D.eulerAngles;
            targetRot2D = Quaternion.Euler(rot.x, rot.y + 180f, rot.z);
        }

        // Don't allow new inputs, but allow last input to update rotation
        if (disableInputRotations)
            UpdateRotation(previousRotation);
        else
            UpdateRotation(targetRot2D);


        if (direction.magnitude > 0.01f)
        {
            if (previousRotation != targetRot2D)
            {
                dampedTargetRotationPassedTime = 0f;
            }

            if (disableInputRotations)
                return targetRot2D;

            // Saved for deceleration
            previousRotation = targetRot2D;
            prevInputDirection = direction;
        }

        return targetRot2D;
    }

    void UpdateRotation(Quaternion targetRot2D)
    {
        float currentYAngle = rb.rotation.eulerAngles.y;
        if (currentYAngle == previousRotation.eulerAngles.y)
        {
            return;
        }

        float smoothedYAngle = Mathf.SmoothDampAngle(currentYAngle, targetRot2D.eulerAngles.y, ref dampedTargetRotationCurrentYVelocity, timeToReachTargetRotation - dampedTargetRotationPassedTime);
        dampedTargetRotationPassedTime += Time.deltaTime;

        Quaternion targetRotation = Quaternion.Euler(0f, smoothedYAngle, 0f);
        rb.MoveRotation(targetRotation);
    }

    void CameraRotation()
    {
        // Rotate camera 2d
        Vector3 camEulerAngle = mainCamera.rotation.eulerAngles;
        virtualCam2D.transform.rotation = Quaternion.Slerp(mainCamera.rotation, Quaternion.Euler(camEulerAngle.x, GetPathRotation().eulerAngles.y - 90f, camEulerAngle.z), camRotationSpeed2D);
    }

    #endregion


    Quaternion GetPathRotation()
    {
        return pathCreator.path.GetRotationAtDistance(distanceOnPath, EndOfPathInstruction.Stop);
    }

}
