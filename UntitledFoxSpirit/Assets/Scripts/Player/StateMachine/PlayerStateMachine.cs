using Cinemachine;
using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateMachine : MonoBehaviour
{
    #region Movement / Rotation

    [Header("Movement")]
    [SerializeField] float humanSpeed = 5.0f;
    [SerializeField] float humanRunSpeed = 10.0f;
    [SerializeField] float accelTimeToMaxSpeed = 2.0f;
    [SerializeField] float decelTimeToZeroSpeed = 1.0f;
    [SerializeField] float animJogSpeed = 1.17f;
    [SerializeField] float animJogAccelSpeed = 0.8f;
    [SerializeField] float animJogDecelSpeed = 0.8f;
    private float accelRatePerSec;
    private float decelRatePerSec;
    private bool isAccel = false;
    private bool isDecel = false;
    private bool isRunning = false;
    private bool disableMovement;

    [SerializeField] float turnSmoothTime2D = 0.03f;
    [SerializeField] float turnSmoothTime3D = 0.1f;
    [SerializeField] float maxVelocityChange = 10f;
    [SerializeField] float frictionAmount = 0.2f;
    private float turnSmoothVelocity;
    private float currentSpeed;
    private float maxSpeed;

    [Header("Rotation")]
    [SerializeField] float timeToReachTargetRotation = 0.14f;
    private float dampedTargetRotationCurrentYVelocity;
    private float dampedTargetRotationPassedTime;
    private bool disableUpdateRotations = false;
    private bool disableInputRotations = false;
    private Quaternion previousRotation;
    private Quaternion targetRot2D;

    [Header("Gravity")]
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float gravityScale = 3f;
    [SerializeField] float fallGravityMultiplier = 0.2f;
    [SerializeField] float reduceVelocityPeak = 5f;
    [SerializeField] float reduceVelocity = 5f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] Vector3 groundCheckSize;
    [SerializeField] LayerMask ignorePlayerMask;
    private bool reduceVelocityOnce = false;
    private bool isGrounded;
    private float updateMaxHeight = 100000f;
    private float updateMaxHeight2 = 100000f;
    private bool disableGravity = false;

    #endregion

    #region Player Actions

    [Header("Jump")]
    [SerializeField] float humanJumpHeight = 5f;
    [SerializeField] float jumpRollVelocity = -5f;
    [SerializeField] float rootMotionJumpRollSpeed = 2f;
    private bool isLanding = false;
    private bool isLandRolling = false;
    private bool disableJumping = false;
    private bool isHoldingJump = false;

    [SerializeField] float jumpCooldown = 0.2f;
    [SerializeField] float jumpBufferTime = 0.1f;   // Detect jump input before touching the ground
    [SerializeField] float jumpCoyoteTime = 0.2f;   // Allow you to jump when you walk off platform
    [SerializeField] float jumpMultiplier = 1.0f;
    [SerializeField] float doubleJumpHeightPercent = 0.5f;
    private float jumpCounter;
    private float jumpBufferCounter;
    private float jumpCoyoteCounter;
    private bool canDoubleJump;
    private float newGroundY = 1000000f;

    [Header("Sneaking")]
    [SerializeField] float sneakSpeed = 2f;
    [SerializeField] Vector3 sneakCheckOffset;
    [SerializeField] Vector3 sneakCheckSize;
    private bool canUnsneak = true;
    private bool isSneaking
    {
        get { return animController.GetBool("isSneaking"); }
        set { animController.SetBool("isSneaking", value); }
    }

    [Header("Dash")]
    [SerializeField] float humanDashTime = 5.0f;
    [SerializeField] float humanDashDistance = 4.0f;
    [SerializeField] float dashCooldown = 1f;
    private float currentDashCooldown = 1f;
    private bool isDashing = false;
    private bool disableDashing = false;

    [Header("Ledge Climb")]
    [SerializeField] float wallCheckDistance = 3.0f;
    [SerializeField] float ledgeHangDistanceOffset;
    [SerializeField] float ledgeHangYOffset;
    [SerializeField] float distanceFromGround = 1f;
    [SerializeField] float ledgeHangCooldown = 1f;
    [SerializeField] Transform wallCheck;
    [SerializeField] Transform ledgeCheck;
    [SerializeField] Transform ledgeRootJntTransform;
    [SerializeField] LayerMask groundLayer;
    private float currentLedgeHangCooldown;
    private bool isClimbing = false;
    private bool isWallClimbing;
    private bool canClimbWall;

    private bool isTouchingWall;
    private bool isTouchingLedge;
    [HideInInspector] public bool canClimbLedge = false;
    [HideInInspector] public bool ledgeDetected;

    [Header("Attack")]
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] float resetComboDelay = 1f;
    [SerializeField] float rootMotionAtkSpeed = 2f;
    private bool isAttacking = false;
    private float currentAttackCooldown;
    private int comboCounter;
    private int lastAttackInt;
    private bool resetAttack = true;

    #endregion

    #region Other

    [Header("Stamina")]
    [SerializeField] float staminaConsumption = 20f;
    [SerializeField] float staminaRecovery = 5f;
    [SerializeField] float staminaCooldown = 1f;
    [SerializeField] float maxStamina = 100f;
    private float currentStaminaCooldown = 0f;
    private float currentStamina;


    [Header("Take Damage")]
    [SerializeField] float invulnerableTime = 1f;
    [SerializeField] float regainMovement = 0.5f;
    [SerializeField] float horizontalKnockback = 10f;
    [SerializeField] float verticalKnockback = 10f;
    [HideInInspector] public bool isInvulnerable = false;
    private float currentInvulnerableCooldown;

    [Header("Camera")]
    [SerializeField] float camRotationSpeed2D = 0.2f;
    [SerializeField] Transform mainCamera;

    [Header("Path")]
    [SerializeField] PathCreator pathCreator;
    [SerializeField] float maxDistancePath = 0.5f;
    [SerializeField] float distanceSpawn = 0f;
    [SerializeField] float spawnYOffset = 0f;
    [SerializeField] float adjustVelocity = 1.0f; //Velocity to push player towards the path
    private float distanceOnPath;

    [Header("References")]
    [SerializeField] Slider staminaBar;
    [SerializeField] CinemachineVirtualCamera virtualCam2D;
    [SerializeField] PhysicMaterial friction;
    private Rigidbody rb;
    private Animator animController;
    private CapsuleCollider tallCollider;
    private CapsuleCollider shortCollider;
    private BoxCollider boxCollider;
    protected PlayerInput input;

    #endregion

    #region Internal Variables

    private Vector3 prevInputDirection;

    private bool isHeavyLand = false;

    const float REDUCE_SPEED = 1.414214f;
    
    

    // Debug
    float currentMaxHeight = 0f;
    private Vector3 velocity;

    #endregion

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
        HandleRotation();
        CameraRotation();
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
