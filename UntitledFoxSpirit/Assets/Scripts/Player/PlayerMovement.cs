using Cinemachine;
using PathCreation;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    #region Human Settings

    [Header("Attack")]
    [SerializeField] float attackCooldown = 2f;
    [SerializeField] float resetComboDelay = 1f;
    [SerializeField] float rootMotionAtkSpeed = 2f;
    private bool isAttacking = false;
    private float currentAttackCooldown;
    private int comboCounter;

    #region Movement

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

    [Header("Rotation")]
    [SerializeField] float timeToReachTargetRotation = 0.14f;
    private float dampedTargetRotationCurrentYVelocity;
    private float dampedTargetRotationPassedTime;
    private bool disableUpdateRotations = false;
    private bool disableInputRotations = false;

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

    [Header("Jump")]
    public float humanJumpHeight = 5f;
    [SerializeField] float jumpRollVelocity = -5f;
    [SerializeField] float rootMotionJumpRollSpeed = 2f;
    private bool isLanding = false;
    private bool isLandRolling = false;
    private bool disableJumping = false;

    [Header("Dash")]
    public float humanDashTime = 5.0f;
    public float humanDashDistance = 4.0f;
    [SerializeField] float dashCooldown = 1f;
    private float currentDashCooldown = 1f;
    private bool isDashing = false;
    private bool disableDashing = false;

    #endregion

    #region Other

    [Header("Stamina")]
    public float staminaConsumption = 20f;
    public float staminaRecovery = 5f;
    public float staminaCooldown = 1f;
    public float maxStamina = 100f;
    private float currentStaminaCooldown = 0f;
    private float currentStamina;

    [Header("Ledge Climb")]
    public float wallCheckDistance = 3.0f;
    public float ledgeHangDistanceOffset;
    public float ledgeHangYOffset;
    [SerializeField] float distanceFromGround = 1f;
    [SerializeField] float ledgeHangCooldown = 1f;
    private float currentLedgeHangCooldown;
    public Transform wallCheck;
    public Transform ledgeCheck;
    public LayerMask groundLayer;
    public Animation climbAnim;
    [SerializeField] Transform ledgeRootJntTransform;
    private bool isClimbing = false;

    #endregion

    #endregion

    #region Fox Settings

    [Header("Movement")]
    public float foxSpeed = 5.0f;

    
    [Header("Jump")]
    public float foxJumpHeight = 5f;

    [Header("Dash")]
    public float foxDashTime = 5.0f;
    public float foxDashDistance = 4.0f;

    #endregion

    #region Other Settings

    #region Movement

    [Header("Movement")]
    public float turnSmoothTime2D = 0.03f;
    public float turnSmoothTime3D = 0.1f;
    private float turnSmoothVelocity;
    public float maxVelocityChange = 10f;
    public float frictionAmount = 0.2f;
    public bool mode3D = false;
    private float currentSpeed;
    private float maxSpeed;


    [Header("Step Climb")]
    [SerializeField] GameObject stepRayUpper;
    [SerializeField] float stepRayUpperDistance = 0.7f;
    [SerializeField] GameObject stepRayLower;
    [SerializeField] float stepRayLowerDistance = 0.5f;
    [SerializeField] float stepHeight = 0.7f;
    [SerializeField] float stepSmooth = 1.5f;
    [SerializeField] bool stepDebug = false;

    [Header("Jump")]
    public float jumpCooldown = 0.2f;
    private float jumpCounter;
    public float jumpBufferTime = 0.1f;   // Detect jump input before touching the ground
    private float jumpBufferCounter;
    public float jumpCoyoteTime = 0.2f;   // Allow you to jump when you walk off platform
    private float jumpCoyoteCounter;
    private bool canDoubleJump;
    public float jumpMultiplier = 1.0f;
    private float newGroundY = 1000000f;
    public float doubleJumpHeightPercent = 0.5f;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float gravityScale = 3f;
    public float fallGravityMultiplier = 0.2f;
    public float reduceVelocity = 5f;
    [SerializeField] Vector3 groundCheckOffset;
    [SerializeField] Vector3 groundCheckSize;
    public LayerMask ignorePlayerMask;
    private bool isGrounded;
    private float updateMaxHeight = 100000f;
    private float updateMaxHeight2 = 100000f;
    private bool disableGravity = false;

    #endregion

    #region Other

    [Header("Damage")]
    public float invulnerableTime = 1f;
    public float regainMovement = 0.5f;
    public float horizontalKnockback = 10f;
    public float verticalKnockback = 10f;
    private float currentInvulnerableCooldown;
    [HideInInspector] public bool isInvulnerable = false;

    [Header("Camera")]
    public float camRotationSpeed2D = 0.2f;
    public Transform mainCamera;

    [Header("Path")]
    public PathCreator pathCreator;
    public float maxDistancePath = 0.5f;
    public float distanceSpawn = 0f;
    public float spawnYOffset = 0f;
    [Tooltip("Velocity to push player towards the path")] public float adjustVelocity = 1.0f;

    [Header("References")]
    public Slider staminaBar;
    public CinemachineVirtualCamera virtualCam2D;
    public CinemachineFreeLook virtualCam3D;
    public GameObject human;
    public GameObject fox;
    [SerializeField] PhysicMaterial friction;
    [SerializeField] Transform rootJnt;

    #endregion

    #endregion

    #region Internal Variables

    private bool isFox;
    private bool isWallClimbing;
    private bool canClimbWall;
    private bool isHoldingJump = false;
    private Vector3 prevInputDirection;

    private bool isHeavyLand = false;

    private bool isParrying = false;
    private int lastAttackInt;
    private bool resetAttack = true;

    // Ledge Climbing
    private bool isTouchingWall;
    private bool isTouchingLedge;
    [HideInInspector] public bool canClimbLedge = false;
    [HideInInspector] public bool ledgeDetected;

    // Save current rotation when input is pressed
    private Quaternion previousRotation;

    const float REDUCE_SPEED = 1.414214f;
    private float distanceOnPath;

    // References
    Rigidbody rb;
    Animator animController;
    CapsuleCollider tallCollider;
    CapsuleCollider shortCollider;
    BoxCollider boxCollider;

    // Debug
    float currentMaxHeight = 0f;
    private Vector3 velocity;

    #endregion

    #region Unity Functions

    // Start is called before the first frame update
    void Start()
    {

        rb = GetComponent<Rigidbody>();
        animController = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider>();


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
        stepRayUpper.transform.position = new Vector3(stepRayUpper.transform.position.x, stepRayLower.transform.position.y + stepHeight, stepRayUpper.transform.position.z);

        // Acceleration / Deceleration Calculation
        float maxSpeed = isFox ? foxSpeed : humanSpeed;
        accelRatePerSec = maxSpeed / accelTimeToMaxSpeed;
        decelRatePerSec = -maxSpeed / decelTimeToZeroSpeed;

        if (rb.velocity.y > 0)
        {
            currentMaxHeight = transform.position.y;
        }

        VirtualCamUpdate();
        GroundCheck();
        Stamina();
        WallClimb();
        CheckInvulnerableTime();
        HardLand();

        if (Input.GetKey(KeyCode.Space)) //if space is held
        {
            isHoldingJump = true;

        }
        else
        {
            isHoldingJump = false;
        }

        if (!isWallClimbing && !canClimbLedge && !disableMovement)
        {
            // Player Input Functions
            
            ChangeForm();
            
            Sneak();
            
        }

        LedgeClimb();
        Jump();

        //Parry();
        DashInput();
        Attack();

        

        
    }

    void FixedUpdate()
    {
        ApplyGravity();
        Movement();
    }

    void OnAnimatorMove()
    {
        if (isClimbing)
        {
            rb.velocity = animController.deltaPosition * rootMotionAtkSpeed / Time.deltaTime;

            Debug.DrawRay(transform.position, animController.deltaPosition.normalized);
        }

        // Attacking root motion
        if (isAttacking && !disableDashing && !animController.IsInTransition(0))
        {
            float y = rb.velocity.y;

            rb.velocity = animController.deltaPosition * rootMotionAtkSpeed / Time.deltaTime;

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

                rb.velocity = animController.deltaPosition * rootMotionJumpRollSpeed / Time.deltaTime;

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

    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.identity;

        // Spawn player position
        if (distanceSpawn >= 0 && distanceSpawn <= pathCreator.path.length)
        {
            Vector3 spawnPosition = pathCreator.path.GetPointAtDistance(distanceSpawn);
            spawnPosition.y += spawnYOffset;

            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(spawnPosition, 0.5f);
        }

        // Player ground check
        Vector3 point = new Vector3(transform.position.x + groundCheckOffset.x, transform.position.y + groundCheckOffset.y, transform.position.z + groundCheckOffset.z) + Vector3.down;
        Gizmos.matrix = Matrix4x4.TRS(point, transform.rotation, transform.lossyScale);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, groundCheckSize);

        // Player head check
        Vector3 centerPos = new Vector3(transform.position.x + sneakCheckOffset.x, transform.position.y + sneakCheckOffset.y, transform.position.z + sneakCheckOffset.z) + Vector3.up;
        Gizmos.matrix = Matrix4x4.TRS(centerPos, transform.rotation, transform.lossyScale);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, sneakCheckSize);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WallClimb"))
        {
            canClimbWall = true;
        }

        if (other.CompareTag("Hitbox"))
        {
            EnemyNavigation enemyNav = other.GetComponentInParent<EnemyNavigation>();
            Vector3 enemyPos = enemyNav.transform.position;

            // Get dir from AI to player
            Vector3 facingDir = (other.ClosestPointOnBounds(transform.position) - enemyPos).IgnoreYAxis();
            Vector3 dir = enemyNav.CalculatePathFacingDir(enemyPos, facingDir);

            TakeDamage(dir);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("WallClimb"))
        {
            canClimbWall = false;
        }
    }

    #endregion

    #region Player Movement

    #region Movement Other Functions

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

    #region Rotation

    void Rotation3D(float targetAngle3D, Vector3 direction)
    {
        // Player movement/rotation direction
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle3D, ref turnSmoothVelocity, turnSmoothTime3D);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

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

        //transform.rotation = targetRot2D;
        
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

    #endregion

    void AdjustPlayerOnPath()
    {
        Vector3 pathPos = pathCreator.path.GetPointAtDistance(distanceOnPath, EndOfPathInstruction.Stop);
        //Debug.DrawLine(pathPos + new Vector3(0f, 1f, 0f), pathPos + Vector3.up * 3f, Color.green);

        // Distance between path and player
        float distance = Vector3.Distance(pathPos.IgnoreYAxis(), transform.position.IgnoreYAxis());

        // Direction from path towards player
        Vector3 dirTowardPlayer = transform.position.IgnoreYAxis() - pathPos.IgnoreYAxis();
        Debug.DrawLine(pathPos, pathPos + dirTowardPlayer * maxDistancePath, Color.blue);

        // Keeps player on the path
        if (distance > maxDistancePath)
        {
            Vector3 dirTowardPath = (pathPos.IgnoreYAxis() - transform.position.IgnoreYAxis()).normalized;
            rb.AddForce(dirTowardPath * adjustVelocity, ForceMode.Impulse);
        }
    }

    #endregion

    #region Movement functions

    void Movement()
    {
        if (!mode3D)
        {
            // Rotate camera 2d
            Vector3 camEulerAngle = mainCamera.rotation.eulerAngles;
            virtualCam2D.transform.rotation = Quaternion.Slerp(mainCamera.rotation, Quaternion.Euler(camEulerAngle.x, GetPathRotation().eulerAngles.y - 90f, camEulerAngle.z), camRotationSpeed2D);
        }

        Vector3 targetVelocity3D = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 targetVelocity2D = new Vector3(Input.GetAxisRaw("Horizontal"), 0, 0);

        Vector3 targetVelocity = mode3D ? targetVelocity3D : targetVelocity2D;
        Vector3 direction = targetVelocity.normalized;


        maxSpeed = isFox ? foxSpeed : humanSpeed;
        
        if (isSneaking)
            maxSpeed = sneakSpeed;

        DetectAnimAcceleration(targetVelocity, direction); // uses maxSpeed

        #region Rotation

        // Calculate player 3D rotation
        float targetAngle3D = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;

        // Calculate player 2D rotation
        distanceOnPath = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        Quaternion targetRot2D = GetPathRotation();

        if (mode3D)
            Rotation3D(targetAngle3D, direction);
        else
            targetRot2D = Rotation2D(targetRot2D, direction);

        #endregion

        if (disableMovement)
            return;

        #region Is player moving?

        // If player is moving
        if (direction.magnitude > 0.01f)
        {
            #region Reached end of path
            if (direction.x < 0f)
            {
                if (distanceOnPath <= 0)
                {
                    maxSpeed = 0f;
                }
            }
            else if (direction.x > 0f)
            {
                if (distanceOnPath >= pathCreator.path.length)
                {
                    maxSpeed = 0f;
                }
            }
            #endregion

            // Acceleration
            currentSpeed += accelRatePerSec * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            tallCollider.material = null;
            shortCollider.material = null;

            if (animController.GetCurrentAnimatorStateInfo(0).IsName("Running") || isRunning || isDashing)
            {
                currentSpeed = humanRunSpeed;
                isRunning = true;
            }

            if (animController.GetCurrentAnimatorStateInfo(0).IsTag("Land"))
            {
                isRunning = false;
            }
        }
        else
        {
            // Deceleration
            currentSpeed += decelRatePerSec * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, 0);

            tallCollider.material = friction;
            shortCollider.material = friction;

            isRunning = false;
        }

        animController.SetFloat("ForwardSpeed", currentSpeed);

        #endregion



        #region Calculate Velocity

        // Where we want to player to face/walk towards
        Vector3 desiredDir = (mode3D ? Quaternion.Euler(0f, targetAngle3D, 0f) : targetRot2D) * Vector3.forward;

        // Rotation Debug Line for path
        //Debug.DrawLine(transform.position, transform.position + targetRot2D * Vector3.forward * 3f, Color.red);


        if (targetVelocity.z == 1 && targetVelocity.x == 1 || targetVelocity.z == 1 && targetVelocity.x == -1 || targetVelocity.z == -1 && targetVelocity.x == 1 || targetVelocity.z == -1 && targetVelocity.x == -1)
        {
            // Player is moving diagonally
            targetVelocity = desiredDir * targetVelocity.magnitude * currentSpeed / REDUCE_SPEED;
        }
        else if (targetVelocity.magnitude > 0f)
        {
            // Player currently apply input
            targetVelocity = desiredDir * targetVelocity.magnitude * currentSpeed;
        }
        else
        {
            // No input
            targetVelocity = previousRotation * Vector3.forward * currentSpeed;
        }

        Vector3 rbVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Apply a force that attempts to reach our target velocity
        Vector3 velocity = rbVelocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        #endregion

        //if (isGrounded)
        //  StepClimb(desiredDir); // After movement

        if (!mode3D)
        {
            AdjustPlayerOnPath();
        }

    }

    void Jump()
    {
        if (animController.GetCurrentAnimatorStateInfo(0).IsName("DoubleJump"))
        {
            if (animController.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f)
            {
                animController.SetBool("DoubleJump", false);
            }
        }

        #region Coyote and Jump Buffer Timers

        if (jumpCounter <= 0f)
        {
            animController.SetBool("Jump", false);
        }

        // Coyote Time
        if (isGrounded && jumpCounter <= 0f)
        {
            canDoubleJump = true;
            

            jumpCoyoteCounter = jumpCoyoteTime;
        }
        else
        {
            jumpCoyoteCounter -= Time.deltaTime;

            // Jumping cooldown
            jumpCounter -= Time.deltaTime;
        }

        //Jump Buffer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        #endregion

        if (isLanding || isSneaking || isParrying || disableJumping && canDoubleJump || isClimbing || canClimbLedge)
            return;

        // Player jump input
        if (jumpBufferCounter > 0f && jumpCoyoteCounter > 0f || isClimbing && jumpBufferCounter > 0f)
        {

            float jumpHeight = isFox ? foxJumpHeight : humanJumpHeight;

            // Calculate Velocity
            float velocity = Mathf.Sqrt(-2 * gravity * jumpHeight * gravityScale);
            velocity += -rb.velocity.y; // Cancel out current velocity
            
            //temporary
            updateMaxHeight = newGroundY + humanJumpHeight;
            Debug.Log("updated height 1: " + updateMaxHeight);

            //updateMaxHeight = transform.position.y + jumpHeight;

            // Jump
            rb.AddForce(new Vector3(0, velocity, 0), ForceMode.Impulse);

            animController.SetBool("Jump", true);

            // Set jump cooldown
            jumpCounter = jumpCooldown;

            if (jumpCoyoteCounter > 0f)
            {
                jumpCoyoteCounter = 0f; // So you don't triple jump
            }

        } //2nd jump
        else if (Input.GetKeyDown(KeyCode.Space) && canDoubleJump && !isGrounded)
        {
            //doubleJumpHeight = humanJumpHeight / 3;//the three is a magic number 
            float jumpHeight = isFox ? foxJumpHeight : humanJumpHeight;

            // Calculate Velocity
            float velocity = Mathf.Sqrt(-2 * gravity * (jumpHeight * doubleJumpHeightPercent) * gravityScale);
            velocity += -rb.velocity.y; // Cancel out current velocity

            //temporary
            updateMaxHeight2 = transform.position.y + humanJumpHeight * doubleJumpHeightPercent;
            Debug.Log("updated height 1+2=: " + (updateMaxHeight2));
            Debug.Log("updated height 2 only: " + (updateMaxHeight2 - updateMaxHeight));

            //updateMaxHeight = transform.position.y + jumpHeight;

            // Jump
            rb.AddForce(new Vector3(0, velocity, 0), ForceMode.Impulse);

            animController.SetBool("DoubleJump", true);

            // Set jump cooldown
            jumpCounter = jumpCooldown;

            if (!isGrounded && canDoubleJump)
            {
                canDoubleJump = false;
            }
        }
    }

    void DashInput()
    {
        if (animController.GetCurrentAnimatorStateInfo(0).IsTag("Dash"))
        {
            // Wait for animation transition
            if (!isDashing)
            {
                isDashing = true;
            }
        }
        else
        {
            if (isDashing)
            {
                animController.SetBool("Dash", false);
                isDashing = false;
            }

            if (currentDashCooldown > 0f)
            {
                currentDashCooldown -= Time.deltaTime;
                return;
            }

            if (!isFox && currentStamina < staminaConsumption || isFox || !disableDashing && !isGrounded || isSneaking)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                animController.SetBool("Dash", true);
                disableMovement = true;
                disableDashing = true;
                disableInputRotations = true;
                animController.speed = 1f;

                currentDashCooldown = dashCooldown;
                
                if (prevInputDirection.x < 0.1f)
                {
                    StartCoroutine(Dash(false));
                }
                else
                {
                    StartCoroutine(Dash(true));
                }
            }
        }
    }

    IEnumerator Dash(bool usePathRotation)
    {
        currentSpeed = humanRunSpeed;
        animController.SetFloat("ForwardSpeed", currentSpeed);

        if (!isFox)
        {
            // Stamina
            currentStamina -= staminaConsumption;
            currentStaminaCooldown = staminaCooldown;
        }

        disableMovement = true;

        // Set Y velocity to 0
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Is in fox form?
        float dashDistance = isFox ? foxDashDistance : humanDashDistance;
        float dashTime = isFox ? foxDashTime : humanDashTime;

        // Calculate speed
        float speed = dashDistance / dashTime;

        float currentDashTime = dashTime;

        while (currentDashTime > 0f)
        {
            if (!isGrounded)
                //break;

            Debug.Log("dashing");

            currentDashTime -= Time.deltaTime;

            #region Rotation

            Quaternion targetRot = GetPathRotation();
            if (usePathRotation)
            {
                distanceOnPath += speed * Time.deltaTime;
            }
            else
            {
                Vector3 rot = targetRot.eulerAngles;
                targetRot = Quaternion.Euler(rot.x, rot.y + 180f, rot.z);

                distanceOnPath -= speed * Time.deltaTime;
            }

            #endregion

            #region Velocity

            // Where we want to player to face/walk towards
            Vector3 desiredDir = targetRot * Vector3.forward;

            Vector3 targetVelocity = desiredDir * speed;

            // Get rigidbody x and y velocity
            Vector3 rbVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rbVelocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;

            rb.AddForce(velocityChange, ForceMode.VelocityChange);

            #endregion

            yield return new WaitForEndOfFrame();
        }

        disableMovement = false;
        disableDashing = false;
        disableInputRotations = false;


        if (isGrounded)
            animController.SetBool("isSprinting", true);
    }

    void Sneak()
    {
        if (!isGrounded || isAttacking || disableJumping /* TODO: make own bool for sneaking*/)
            return;

        if (isSneaking)
        {
            Vector3 centerPos = new Vector3(transform.position.x + sneakCheckOffset.x, transform.position.y + sneakCheckOffset.y, transform.position.z + sneakCheckOffset.z) + Vector3.up;

            canUnsneak = !Physics.CheckBox(centerPos, sneakCheckSize / 2, Quaternion.identity, ~ignorePlayerMask);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (isSneaking && canUnsneak)
            {
                tallCollider.enabled = true;
                shortCollider.enabled = false;
                isSneaking = false;
            }
            else
            {
                tallCollider.enabled = false;
                shortCollider.enabled = true;
                isSneaking = true;
            }
        }
    }

    void HardLand()
    {
        if (rb.velocity.y <= jumpRollVelocity)
        {
            disableJumping = true;

            animController.SetTrigger("HeavyLand");

            if (animController.GetBool("isMoving"))
                isLandRolling = true;
        }
        

        if (animController.GetCurrentAnimatorStateInfo(0).IsName("HardLanding"))
        {
            if (!isHeavyLand)
            {
                isHeavyLand = true;
                disableMovement = true;
                disableInputRotations = true;
                tallCollider.material = friction;
            }

            if (animController.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f)
            {
                disableMovement = false;
                disableInputRotations = false;
                disableJumping = false;
            }
        }
        else
        {
            if (isHeavyLand)
            {
                isHeavyLand = false;
                disableMovement = false;
                disableInputRotations = false;
                disableJumping = false;
            }
        }
    }

    void StepClimb(Vector3 direction)
    {
        if (!animController.GetBool("isMoving"))
            return;

        if (stepDebug)
        {
            Debug.DrawRay(stepRayLower.transform.position, direction * stepRayLowerDistance, Color.red);

            Debug.DrawRay(stepRayUpper.transform.position, direction * stepRayUpperDistance, Color.blue);
        }

        // Forward
        if (Physics.Raycast(stepRayLower.transform.position, direction, stepRayLowerDistance, ~ignorePlayerMask))
        {
            if (!Physics.Raycast(stepRayUpper.transform.position, direction, stepRayUpperDistance, ~ignorePlayerMask))
            {
                rb.position += new Vector3(0f, stepSmooth * Time.deltaTime, 0f);

                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

                Vector3 velocity = new Vector3(0f, stepSmooth, 0f);

                rb.AddForce(velocity);
            }
        }
    }

    #endregion

    #endregion

    #region Gravity

    void ApplyGravity()
    {
        if (!disableGravity)
        {
            // Reached max height, positive velocity
            if (transform.position.y >= updateMaxHeight && rb.velocity.y > 0f && !isGrounded)
            {
                rb.AddForce(new Vector3(0, gravity * reduceVelocity, 0));
            }
            
            if (rb.velocity.y <= 0f)
            {
                // Player Falling
                rb.AddForce(new Vector3(0, gravity, 0) * rb.mass * fallGravityMultiplier);

                if (!isGrounded)
                    animController.SetBool("Fall", true);
            }
            else if (rb.velocity.y > 0f && !isHoldingJump) // while jumping and not holding jump
            {
                rb.AddForce(new Vector3(0, -50, 0));
                //rb.AddForce(new Vector3(0, gravity * reduceVelocity, 0));
            }   
            else
            {
                // Jumping while holding jump input
                rb.AddForce(new Vector3(0, gravity, 0));
            }
        }
    }

    void GroundCheck()
    {
        Vector3 centerPos = new Vector3(transform.position.x + groundCheckOffset.x, transform.position.y + groundCheckOffset.y, transform.position.z + groundCheckOffset.z) + Vector3.down;
        //Vector3 size = isFox ? new Vector3(0.9f, 0.1f, 1.9f) : new Vector3(0.8f, 0.1f, 0.8f);

        bool overlap = Physics.CheckBox(centerPos, groundCheckSize / 2, transform.rotation, ~ignorePlayerMask);

        RaycastHit hit;
        if(Physics.Raycast(centerPos, Vector3.down, out hit, 100f, ~ignorePlayerMask))
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

    #endregion

    #region Take Damage
    public void TakeDamage(Vector3 direction)
    {
        if (!isInvulnerable)
        {
            isInvulnerable = true;
            disableMovement = true;
            currentInvulnerableCooldown = invulnerableTime;
             
            rb.velocity = new Vector3(direction.x * horizontalKnockback, verticalKnockback, direction.z * horizontalKnockback);
        }
    }

    void CheckInvulnerableTime()
    {
        if (isInvulnerable)
        {
            // Player regain control
            if (currentInvulnerableCooldown <= invulnerableTime - regainMovement && disableMovement)
            {
                disableMovement = false;
            }

            // Invulnerable Timer
            if (currentInvulnerableCooldown <= 0f)
            {
                isInvulnerable = false;
            }
            else
            {
                currentInvulnerableCooldown -= Time.deltaTime;
            }
        }
    }

    #endregion

    #region Wall Climb

    void WallClimb()
    {
        if (canClimbWall && !isWallClimbing)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isWallClimbing = true;
            }
        }
        else if (isWallClimbing)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                isWallClimbing = false;
            }

            // Get input
            Vector3 targetVelocity = new Vector3(0, Input.GetAxisRaw("Vertical"), 0f);

            #region Calculate Velocity

            float speed = humanSpeed;


            if (targetVelocity.y < 0f)
            {
                // Down
                targetVelocity = -transform.up * targetVelocity.magnitude * speed;
            }
            else
            {
                // Up
                targetVelocity = transform.up * targetVelocity.magnitude * speed;
            }
            

            Vector3 rbVelocity = new Vector3(0f, rb.velocity.y, 0f);

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rbVelocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = 0f;
            velocityChange.z = 0f;
            velocityChange.y = Mathf.Clamp(velocityChange.y, -maxVelocityChange, maxVelocityChange);

            #endregion

            rb.AddForce(velocityChange, ForceMode.VelocityChange);

        }
    }

    #endregion

    #region Player Controls

    void Stamina()
    {
        if (!isFox)
        {
            // How long until stamina starts recovering
            if (currentStaminaCooldown > 0)
            {
                currentStaminaCooldown -= Time.deltaTime;
            }

            // If stamina hasn't been used recover stamina
            if (currentStamina < maxStamina && currentStaminaCooldown <= 0f)
            {
                currentStamina += staminaRecovery;
            }

            // Set stamina ui
            staminaBar.value = currentStamina / maxStamina;
        }
    }

    // Parry not being used atm
    void Parry()
    {
        if (isDashing || isSneaking || !isGrounded || isAttacking)
            return;

        if (animController.GetCurrentAnimatorStateInfo(0).IsName("Parry"))
        {
            if (!isParrying)
                isParrying = true;
        }
        else
        {
            if (isParrying)
            {
                isParrying = false;
                disableMovement = false;
                disableInputRotations = false;
                currentSpeed = 1f;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                animController.SetTrigger("Parry");
                disableMovement = true;
                rb.velocity = Vector3.zero;
                disableInputRotations = true;
                currentSpeed = 0f;
                tallCollider.material = friction;
                animController.SetBool("isMoving", false);
            }
        }
    }

    void Attack()
    {
        // Is currently playing attack animation
        if (animController.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            // Attack animation has started!
            if (!isAttacking)
            {
                rb.velocity = Vector3.zero;
                disableMovement = true;
                disableInputRotations = true;
                isAttacking = true;
                currentSpeed = 0f;
            }

            // Move after attacking
            if (animController.IsInTransition(0) && animController.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f)
            {
                disableMovement = false;
            }

        }
        else
        { // Not playing attacking animation
            if (isAttacking)
            {
                disableMovement = false;
                disableInputRotations = false;
                isAttacking = false;
                comboCounter = 0;

                animController.SetBool("Attack1", false);
                animController.SetBool("Attack2", false);
                animController.SetBool("Attack3", false);
            }
        }

        // End animation combo
        if (animController.GetCurrentAnimatorStateInfo(0).normalizedTime > animController.GetFloat("resetComboTime") && animController.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            if (!animController.IsInTransition(0) && resetAttack) // Check if not in transiton, so it doesn't reset run during transition
            {
                animController.SetBool("Attack1", false);
                disableMovement = false;
                disableInputRotations = false;
                resetAttack = false;
                comboCounter = 0;

                int atkNum = Random.Range(1, 5);
                if (lastAttackInt == atkNum)
                {
                    atkNum++;

                    if (atkNum > 4)
                        atkNum = 1;
                }
                lastAttackInt = atkNum;

                animController.SetInteger("RngAttack", atkNum);
            }
        }
        else
        {
            resetAttack = true;
        }

        if (animController.GetCurrentAnimatorStateInfo(0).normalizedTime > animController.GetFloat("resetComboTime") && animController.GetCurrentAnimatorStateInfo(0).IsName("Attack2") && !animController.IsInTransition(0))
        {
            animController.SetBool("Attack2", false);
            disableMovement = false;
            comboCounter = 0;
        }
        if (animController.GetCurrentAnimatorStateInfo(0).normalizedTime > animController.GetFloat("resetComboTime") && animController.GetCurrentAnimatorStateInfo(0).IsName("Attack3") && !animController.IsInTransition(0))
        {
            animController.SetBool("Attack3", false);
            disableMovement = false;
            comboCounter = 0;
        }

        // Cooldown to click again
        if (currentAttackCooldown <= 0f)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && isGrounded && !isSneaking && !animController.IsInTransition(0))
            {
                OnClick();
            }
        }

        if (currentAttackCooldown > 0f)
            currentAttackCooldown -= Time.deltaTime;
    }

    void OnClick()
    {
        if (comboCounter == 0)
        {
            disableInputRotations = true;
            isAttacking = true;
        }

        animController.speed = 1f;

        // Set time
        currentAttackCooldown = attackCooldown;

        // Increase combo count
        comboCounter++;
        // Clamp combo
        comboCounter = Mathf.Clamp(comboCounter, 0, 2);
        


        if (comboCounter == 1 && !animController.GetBool("Attack1"))
        {
            animController.SetTrigger("Attack");
            animController.SetBool("Attack1", true);
        }
        
        // Transitions to next combo animation
        if (comboCounter >= 2 && animController.GetCurrentAnimatorStateInfo(0).normalizedTime > animController.GetFloat("attackInputTime") && animController.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
        {
            animController.SetBool("Attack1", false);
            animController.SetBool("Attack2", true);
        }
        if (comboCounter >= 3 && animController.GetCurrentAnimatorStateInfo(0).normalizedTime > animController.GetFloat("attackInputTime") && animController.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
        {
            animController.SetBool("Attack2", false);
            animController.SetBool("Attack3", true);
        }
    }

    void ChangeForm()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isFox)
            {
                human.SetActive(true);
                fox.SetActive(false);

                isFox = false;
            }
            else
            {
                human.SetActive(false);
                fox.SetActive(true);

                isFox = true;
            }
        }
    }


    void FinishLedgeClimb()
    {
        Transform rootJointTransform = ledgeRootJntTransform;
        rootJointTransform.position = new Vector3(rootJointTransform.position.x, rootJointTransform.position.y - 0.65f, rootJointTransform.position.z);
        transform.position = rootJointTransform.position;
        Debug.Log(ledgeRootJntTransform.localPosition);
        ledgeRootJntTransform.localPosition = ledgeRootJntTransform.localPosition;

        Debug.Log("run");
        isClimbing = false;
        canClimbLedge = false;
        rb.useGravity = true;
        disableMovement = false;
        disableGravity = false;
        disableInputRotations = false;
        tallCollider.enabled = true;
        animController.SetBool("LedgeHang", false);

        currentLedgeHangCooldown = ledgeHangCooldown;
    }


    void LedgeClimb()
    {
        if (animController.GetCurrentAnimatorStateInfo(0).IsTag("Hang"))
        {
            if (!isClimbing && canClimbLedge)
            {
                isClimbing = true;
            }

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            {
                animController.SetTrigger("ClimbUp");

            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                animController.SetTrigger("Drop");
                animController.SetBool("LedgeHang", false);
                canClimbLedge = false;
                rb.useGravity = true;
                disableMovement = false;
                disableGravity = false;
                disableInputRotations = false;
                tallCollider.enabled = true;
                isClimbing = false;

                currentLedgeHangCooldown = ledgeHangCooldown;
            }
        }
        else
        {
            if (currentLedgeHangCooldown > 0f)
            {
                currentLedgeHangCooldown -= Time.deltaTime;
                return;
            }

            if (rb.velocity.y >= 0f || isGrounded || isClimbing)
                return;


            // Raycasts
            isTouchingLedge = Physics.Raycast(ledgeCheck.position, -prevInputDirection, wallCheckDistance, groundLayer);
            isTouchingWall = Physics.Raycast(wallCheck.position, transform.forward, wallCheckDistance, groundLayer);
            Vector3 ledgeCheckEndPoint = ledgeCheck.position + -prevInputDirection * wallCheckDistance;
            
            Debug.DrawRay(ledgeCheckEndPoint, Vector3.down * wallCheckDistance);
            

            RaycastHit verticalHit;
            // Check if there is floor
            if (Physics.Raycast(ledgeCheckEndPoint, Vector3.down, out verticalHit, wallCheckDistance, groundLayer))
            {
                Vector3 wallCheckPos = ledgeCheck.position;
                wallCheckPos.y = verticalHit.point.y - 0.01f;

                Debug.DrawRay(wallCheckPos, -prevInputDirection * wallCheckDistance);

                RaycastHit horizontalHit;
                if (Physics.Raycast(wallCheckPos, -prevInputDirection, out horizontalHit, wallCheckDistance, groundLayer))
                {
                    if (Physics.Raycast(transform.position, Vector3.down, distanceFromGround, groundLayer))
                        return;

                    if (canClimbLedge)
                        return;

                    canClimbLedge = true;
                    animController.SetBool("LedgeHang", true);
                    rb.velocity = Vector3.zero;
                    rb.useGravity = false;
                    disableMovement = true;
                    disableGravity = true;
                    disableInputRotations = true;
                    tallCollider.enabled = false;

                    Vector3 hangPos;
                    hangPos = horizontalHit.point + -prevInputDirection * ledgeHangDistanceOffset;
                    hangPos.y = verticalHit.point.y - ledgeHangYOffset;

                    transform.position = hangPos;
                }
            }
        }
    }

    #endregion

    #region Other Functions

    void VirtualCamUpdate()
    {
        if (mode3D)
        {
            virtualCam3D.Priority = 10;
            virtualCam2D.Priority = 0;
        }
        else
        {
            virtualCam3D.Priority = 0;
            virtualCam2D.Priority = 10;
        }
    }

    Quaternion GetPathRotation()
    {
        return pathCreator.path.GetRotationAtDistance(distanceOnPath, EndOfPathInstruction.Stop);
    }

    public void ChangePath(PathCreator path)
    {
        pathCreator = path;
        distanceOnPath = 0f;
    }

    #endregion

    
}
