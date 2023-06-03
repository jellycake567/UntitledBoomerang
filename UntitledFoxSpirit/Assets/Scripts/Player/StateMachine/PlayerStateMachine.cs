using Cinemachine;
using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStateMachine : MonoBehaviour
{
    [HideInInspector]
    public VariableScriptObject vso;

    [HideInInspector]
    public PlayerBaseState currentState;
    PlayerStateFactory states;

    [Header("References")]
    public Transform mainCamera;
    public Transform wallCheck;
    public Transform ledgeCheck;
    public Transform ledgeRootJntTransform;
    public PathCreator pathCreator;
    public Slider staminaBar;
    public CinemachineVirtualCamera virtualCam2D;
    public PhysicMaterial friction;

    // Movement
    private float accelRatePerSec;
    private float decelRatePerSec;
    private bool isAccel = false;
    private bool isDecel = false;
    private bool isRunning = false;
    private bool disableMovement;
    private float turnSmoothVelocity;
    private float currentSpeed;
    private float maxSpeed;

    // Rotation
    private float dampedTargetRotationCurrentYVelocity;
    private float dampedTargetRotationPassedTime;
    private bool disableUpdateRotations = false;
    private bool disableInputRotations = false;
    private Quaternion previousRotation;
    private Quaternion targetRot2D;

    // Gravity
    private bool reduceVelocityOnce = false;
    private bool isGrounded;
    private float updateMaxHeight = 100000f;
    private float updateMaxHeight2 = 100000f;
    private bool disableGravity = false;

    // Sneaking
    private bool canUnsneak = true;
    private bool isSneaking
    {
        get { return animController.GetBool("isSneaking"); }
        set { animController.SetBool("isSneaking", value); }
    }

    // Jump
    private bool isLanding = false;
    private bool isLandRolling = false;
    private bool disableJumping = false;
    private bool isHoldingJump = false;
    private float jumpCounter;
    private float jumpBufferCounter;
    private float jumpCoyoteCounter;
    private bool canDoubleJump;
    private float newGroundY = 1000000f;

    // Dash
    private float currentDashCooldown = 1f;
    private bool isDashing = false;
    private bool disableDashing = false;

    // Ledge Climb
    private float currentLedgeHangCooldown;
    private bool isClimbing = false;
    private bool isWallClimbing;
    private bool canClimbWall;
    
    private bool isTouchingWall;
    private bool isTouchingLedge;
    private bool canClimbLedge = false;
    private bool ledgeDetected;

    // Attack
    private bool isAttacking = false;
    private float currentAttackCooldown;
    private int comboCounter;
    private int lastAttackInt;
    private bool resetAttack = true;

    // Stamina
    private float currentStaminaCooldown = 0f;
    private float currentStamina;

    // Take Damage
    private bool isInvulnerable = false;
    private float currentInvulnerableCooldown;

    // Path
    private float distanceOnPath;

    // References
    private Rigidbody rb;
    private Animator animController;
    private CapsuleCollider tallCollider;
    private CapsuleCollider shortCollider;
    private BoxCollider boxCollider;
    private PlayerInput input;

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

        currentStamina = vso.maxStamina;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;


        Vector3 spawnPos = pathCreator.path.GetPointAtDistance(vso.distanceSpawn);
        spawnPos.y += vso.spawnYOffset + 1.0f;
        transform.position = spawnPos;
        distanceOnPath = pathCreator.path.GetClosestDistanceAlongPath(transform.position);


        states = new PlayerStateFactory(this);
        currentState = new PlayerGroundedState(this, states);
        currentState.EnterState();
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

    void OnAnimatorMove()
    {
        if (isClimbing && !animController.IsInTransition(0))
        {
            rb.velocity = animController.deltaPosition * vso.rootMotionAtkSpeed / Time.deltaTime;
        }

        // Attacking root motion
        if (isAttacking && !disableDashing && !animController.IsInTransition(0))
        {
            float y = rb.velocity.y;

            rb.velocity = animController.deltaPosition * vso.rootMotionAtkSpeed / Time.deltaTime;

            rb.velocity = new Vector3(rb.velocity.x, y, rb.velocity.z);
        }

        // Jump Roll root motion
        if (isLandRolling && animController.GetBool("Grounded") && !isLanding)
        {
            isLanding = true;
            disableMovement = true;
            disableInputRotations = true;
            tallCollider.material = null;
        }
        if (isLanding)
        {
            AnimatorStateInfo jumpRollState = animController.GetCurrentAnimatorStateInfo(0);

            if (jumpRollState.IsName("JumpRoll") && jumpRollState.normalizedTime < 0.3f || animController.GetBool("Grounded") && animController.IsInTransition(0))
            {
                float y = rb.velocity.y;

                rb.velocity = animController.deltaPosition * vso.rootMotionJumpRollSpeed / Time.deltaTime;

                rb.velocity = new Vector3(rb.velocity.x, y, rb.velocity.z);

            }
            else if (isGrounded)
            {
                isLanding = false;
                disableMovement = false;
                disableJumping = false;
                disableInputRotations = false;
                tallCollider.material = friction;
                isLandRolling = false;
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.matrix = Matrix4x4.identity;

    //    // Spawn player position
    //    if (vso.distanceSpawn >= 0 && vso.distanceSpawn <= vso.pathCreator.path.length)
    //    {
    //        Vector3 spawnPosition = vso.pathCreator.path.GetPointAtDistance(vso.distanceSpawn);
    //        spawnPosition.y += vso.spawnYOffset;

    //        Gizmos.color = Color.cyan;
    //        Gizmos.DrawSphere(spawnPosition, 0.5f);
    //    }

    //    // Player ground check
    //    Vector3 point = new Vector3(transform.position.x + vso.groundCheckOffset.x, transform.position.y + vso.groundCheckOffset.y, transform.position.z + vso.groundCheckOffset.z) + Vector3.down;
    //    Gizmos.matrix = Matrix4x4.TRS(point, transform.rotation, transform.lossyScale);

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireCube(Vector3.zero, vso.groundCheckSize);

    //    // Player head check
    //    Vector3 centerPos = new Vector3(transform.position.x + vso.sneakCheckOffset.x, transform.position.y + vso.sneakCheckOffset.y, transform.position.z + vso.sneakCheckOffset.z) + Vector3.up;
    //    Gizmos.matrix = Matrix4x4.TRS(centerPos, transform.rotation, transform.lossyScale);

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireCube(Vector3.zero, vso.sneakCheckSize);
    //}

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

            jumpCoyoteCounter = vso.jumpCoyoteTime;
        }
        else
        {
            jumpCoyoteCounter -= Time.deltaTime;
        }
    }

    void GroundCheck()
    {
        Vector3 centerPos = new Vector3(transform.position.x + vso.groundCheckOffset.x, transform.position.y + vso.groundCheckOffset.y, transform.position.z + vso.groundCheckOffset.z) + Vector3.down;
        //Vector3 size = isFox ? new Vector3(0.9f, 0.1f, 1.9f) : new Vector3(0.8f, 0.1f, 0.8f);

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

                animController.speed = vso.animJogDecelSpeed;
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
            animController.speed = vso.animJogAccelSpeed; // Set to accel jogging speed
        }
        else if (currentSpeed >= maxSpeed && !isDecel) // If reached max speed, set anim speed to normal jogging speed
        {
            animController.speed = vso.animJogSpeed;
        }



        #endregion
    }

    IEnumerator Deceleration()
    {
        float time = 0f;
        float timeToZero = vso.decelTimeToZeroSpeed * currentSpeed;

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


    Quaternion GetPathRotation()
    {
        return pathCreator.path.GetRotationAtDistance(distanceOnPath, EndOfPathInstruction.Stop);
    }

}
