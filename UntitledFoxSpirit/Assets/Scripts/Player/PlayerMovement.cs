using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using PathCreation;

public class PlayerMovement : MonoBehaviour
{
    #region Human Settings

    [Header("Movement")]
    public float humanSpeed = 5.0f;

    [Header("Jump")]
    public float humanJumpHeight = 5f;

    [Header("Dash")]
    public float humanDashTime = 5.0f;
    public float humanDashDistance = 4.0f;

    [Header("Stamina")]
    public float staminaConsumption = 20f;
    public float staminaRecovery = 5f;
    public float staminaCooldown = 1f;
    private float currentStaminaCooldown = 0f;
    public float maxStamina = 100f;
    private float currentStamina;

    [Header("Wall Climb")]
    public float wallCheckDistance = 3.0f;
    public Transform wallCheck;
    public Transform ledgeCheck;
    public LayerMask groundLayer;
    public Animation climbAnim;

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

    [Header("Movement")]
    public float turnSmoothTime2D = 0.03f;
    public float turnSmoothTime3D = 0.1f;
    private float turnSmoothVelocity;
    public float maxVelocityChange = 10f;
    public float frictionAmount = 0.2f;
    public bool camera3D = false;
    
    [Header("Jump")]
    public float jumpCooldown = 0.2f;
    private float jumpCounter;
    public float jumpBufferTime = 0.1f;   // Detect jump input before touching the ground
    public float jumpCoyoteTime = 0.2f;   // Allow you to jump when you walk off platform
    private float jumpCoyoteCounter;
    private float jumpBufferCounter;
    private bool canDoubleJump;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float gravityScale = 3f;
    public float fallGravityMultiplier = 0.2f;
    public LayerMask ignorePlayerMask;
    private bool isGrounded;

    [Header("Camera")]
    public float camRotationSpeed2D = 0.2f;
    public Transform mainCamera;

    [Header("Path")]
    public PathCreator pathCreator;
    public float maxDistancePath = 0.5f;
    [Tooltip("Velocity to push player towards the path")] public float adjustVelocity = 1.0f;

    [Header("References")]
    public Transform path;
    public Slider staminaBar;
    public CinemachineVirtualCamera virtualCam2D;
    public CinemachineFreeLook virtualCam3D;
    public GameObject human;
    public GameObject fox;
    public Animation attack;

    #endregion

    #region Internal Variables

    private bool isFox;
    private bool isDashing;
    private bool isWallClimbing;
    private bool canClimbWall;

    // Ledge Climbing
    private bool isTouchingWall;
    private bool isTouchingLedge;
    [HideInInspector] public bool canClimbLedge = false;
    [HideInInspector] public bool ledgeDetected;

    // Direction
    private Vector3 currentPathFacingDir;
    private Vector3 rightDir;
    private Vector3 leftDir;

    // Waypoint
    private int currentPoint = 0;
    List<Transform> Waypoints = new List<Transform>();

    const float REDUCE_SPEED = 1.414214f;
    private float distanceOnPath;

    // References
    Rigidbody rb;

    #endregion

    #region Unity Functions

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        currentStamina = maxStamina;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        #region Waypoint

        // "path (transform)" magically supply "all of its children" when it is in a situation such as a foreach
        foreach (Transform child in path)
        {
            Waypoints.Add(child);
        }

        //Vector3 waypoint = Waypoints[currentPoint].position;
        //transform.position = waypoint;

        transform.position = pathCreator.path.localPoints[0] + pathCreator.transform.position + new Vector3(0, 1f, 0);

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        VirtualCamUpdate();
        CalculatePlayerDirection();
        CheckPlayerOnPath();

        GroundCheck();
        Stamina();
        WallClimb();
        LedgeClimb();
        ShowLedgeRaycast();
        
        if (!isWallClimbing && !canClimbLedge)
        {
            // Player Input Functions
            Jump();
            DashInput();
            ChangeForm();
            Attack();
        }
    }

    void FixedUpdate()
    {
        if (!isWallClimbing && !canClimbLedge)
        {
            Gravity();
            Movement();
        }
    }

    #endregion


    #region Player Movement

    void Movement()
    {
        // Get Path direction
        Quaternion targetRot = GetPathRotation();
        float pathAngle = targetRot.eulerAngles.y - 90f;

        // Is 2d camera on
        if (!camera3D)
        {
            // Rotate camera
            Vector3 camEulerAngle = mainCamera.rotation.eulerAngles;
            virtualCam2D.transform.rotation = Quaternion.Slerp(mainCamera.rotation, Quaternion.Euler(camEulerAngle.x, pathAngle, camEulerAngle.z), camRotationSpeed2D);
        }

        // Get input
        Vector3 targetVelocity3D = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 targetVelocity2D = new Vector3(Input.GetAxisRaw("Horizontal"), 0, 0);

        Vector3 targetVelocity = camera3D ? targetVelocity3D : targetVelocity2D;
        Vector3 direction = targetVelocity.normalized;

        if (!isDashing)
        {
            // Calculate player rotation
            float camAngle = camera3D ? mainCamera.eulerAngles.y : pathAngle;
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camAngle;

            float speed = isFox ? foxSpeed : humanSpeed;

            // If player is moving
            if (direction.magnitude > 0.1f)
            {
                if (camera3D)
                {
                    // Player movement/rotation direction
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime3D);
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);
                }
                else
                {
                    #region Path Position
                    //returns if its positive = 1, negative = -1
                    if (Mathf.Sign(direction.x) < 0f)
                    {
                        distanceOnPath -= speed * Time.deltaTime;
                    }
                    else
                    {
                        distanceOnPath += speed * Time.deltaTime;
                    }
                    #endregion

                    #region Player Rotation
                    if (direction.x < 0f)
                    {
                        Vector3 rot = targetRot.eulerAngles;
                        targetRot = Quaternion.Euler(rot.x, rot.y + 180f, rot.z);
                    }
                    transform.rotation = targetRot;
                    #endregion
                }
            }

            #region Calculate Velocity

            // Where we want to player to face/walk towards
            //Vector3 desiredDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Vector3 desiredDir = targetRot * Vector3.forward;

            //Debug.DrawLine(transform.position, transform.position + targetRot * Vector3.forward * 3f, Color.red);

            Vector3 pathPos = pathCreator.path.GetPointAtDistance(distanceOnPath, EndOfPathInstruction.Stop);
            Debug.DrawLine(pathPos + new Vector3(0f, 1f, 0f), pathPos + Vector3.up * 3f, Color.green);

            // Player is moving diagonally
            if (targetVelocity.z == 1 && targetVelocity.x == 1 || targetVelocity.z == 1 && targetVelocity.x == -1 || targetVelocity.z == -1 && targetVelocity.x == 1 || targetVelocity.z == -1 && targetVelocity.x == -1)
            {
                targetVelocity = desiredDir * targetVelocity.magnitude * speed / REDUCE_SPEED;
            }
            else
            {
                targetVelocity = desiredDir * targetVelocity.magnitude * speed;
            }

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

            #region Adjust Player on Path

            // Distance between path and player
            float distance = Vector3.Distance(pathPos.IgnoreYAxis(), transform.position.IgnoreYAxis());

            // Direction from path towards player
            Vector3 dirTowardPlayer = transform.position.IgnoreYAxis() - pathPos.IgnoreYAxis();
            Debug.DrawLine(pathPos, pathPos + dirTowardPlayer * maxDistancePath, Color.blue);

            // Keeps player on the path
            if (distance > maxDistancePath)
            {
                Vector3 dirTowardPath = (pathPos - transform.position.IgnoreYAxis()).normalized;
                rb.AddForce(dirTowardPath * adjustVelocity, ForceMode.Impulse);
            }

            #endregion
        }

    }

    void Jump()
    {
        #region Coyote and Jump Buffer Timers

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

        // Jump Buffer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        #endregion

        // Player jump input
        if (jumpBufferCounter > 0f && jumpCoyoteCounter > 0f || Input.GetKeyDown(KeyCode.Space) && canDoubleJump)
        {
            float jumpHeight = isFox ? foxJumpHeight : humanJumpHeight;

            // Calculate Velocity
            float velocity = Mathf.Sqrt(-2 * gravity * gravityScale * jumpHeight);
            velocity += -rb.velocity.y; // Cancel out current velocity

            // Jump
            rb.AddForce(new Vector3(0, velocity, 0), ForceMode.Impulse);

            // Set jump cooldown
            jumpCounter = jumpCooldown;

            if (jumpCoyoteCounter <= 0f && !isGrounded && canDoubleJump)
            {
                canDoubleJump = false;
            }
            if (jumpCoyoteCounter > 0f)
            {
                jumpCoyoteCounter = 0f; // So you don't triple jump
            }
        }
    }

    #endregion

    #region Dash

    void DashInput()
    {
        if (!isFox && currentStamina >= staminaConsumption || isFox)
        {
            // Dash input
            if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
            {
                // If player is moving left or right
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                {
                    //currentPathFacingDir = Input.GetAxisRaw("Horizontal") > 0 ? rightDir : leftDir;
                    bool isFacingRight = Input.GetAxisRaw("Horizontal") > 0 ? true : false;

                    StartCoroutine(Dash(isFacingRight));
                }
            }
        }
    }

    IEnumerator Dash(bool isFacingRight)
    {
        isDashing = true;

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
            currentDashTime -= Time.deltaTime;

            #region Rotation

            Quaternion targetRot = GetPathRotation();
            if (isFacingRight)
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

        

        isDashing = false;

        if (!isFox)
        {
            // Stamina
            currentStamina -= staminaConsumption;
            currentStaminaCooldown = staminaCooldown;
        }
    }

    #endregion

    #region Gravity

    void Gravity()
    {
        if (!isDashing)
        {
            if (rb.velocity.y < 0f)
            {
                // Player Falling
                rb.AddForce(new Vector3(0, gravity, 0) * rb.mass * gravityScale * fallGravityMultiplier);
            }
            else
            {
                rb.AddForce(new Vector3(0, gravity, 0) * rb.mass * gravityScale);
            }
        }
    }

    void GroundCheck()
    {
        Vector3 point = transform.position + Vector3.down;
        Vector3 size = isFox ? new Vector3(0.6f, 0.1f, 0.6f) : new Vector3(0.9f, 0.1f, 1.9f);

        bool overlap = Physics.CheckBox(point, size, Quaternion.identity, ~ignorePlayerMask);

        if (overlap && rb.velocity.y <= 0f)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WallClimb"))
        {
            canClimbWall = true;
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

    void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            attack.Play();
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

    void LedgeClimb()
    {
        // Raycasts
        isTouchingWall = Physics.Raycast(wallCheck.position, transform.forward, wallCheckDistance, groundLayer);
        isTouchingLedge = Physics.Raycast(ledgeCheck.position, transform.forward, wallCheckDistance, groundLayer);

        Vector3 ledgeCheckEndPoint = ledgeCheck.position + transform.forward * wallCheckDistance;

        // Check if there is floor
        if (Physics.Raycast(ledgeCheckEndPoint, -transform.up, wallCheckDistance, groundLayer))
        {
            if (isTouchingWall && !isTouchingLedge && !canClimbLedge)
            {
                canClimbLedge = true;
                climbAnim.Play();
                rb.velocity = Vector3.zero;
            }
        }
    }

    #endregion

    #region Other Functions

    void VirtualCamUpdate()
    {
        if (camera3D)
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

    void CheckPlayerOnPath()
    {
        if (!camera3D)
        {
            Vector3 waypoint1 = new Vector3(Waypoints[currentPoint].position.x, 0f, Waypoints[currentPoint].position.z);
            Vector3 waypoint2 = new Vector3(Waypoints[currentPoint + 1].position.x, 0f, Waypoints[currentPoint + 1].position.z);
            Vector3 playerPos = new Vector3(transform.position.x, 0f, transform.position.z);

            float maxDistance = Vector3.Distance(waypoint1, waypoint2);

            // Going right
            float rightDis = Vector3.Distance(waypoint1, playerPos);

            // Going Left
            float leftDis = Vector3.Distance(waypoint2, playerPos);

            if (leftDis > maxDistance && rightDis < maxDistance)
            {
                // Going left
                if (currentPoint > 0)
                {
                    currentPoint--;
                }
            }
            else if (rightDis > maxDistance && leftDis < maxDistance)
            {
                // Going Right
                if (currentPoint < Waypoints.Count - 2)
                {
                    currentPoint++;
                }
            }
        }
    }

    void ShowLedgeRaycast()
    {
        Debug.DrawLine(wallCheck.position, wallCheck.position + transform.forward * wallCheckDistance);
        Debug.DrawLine(ledgeCheck.position, ledgeCheck.position + transform.forward * wallCheckDistance);
    }

    Quaternion GetPathRotation()
    {
        return pathCreator.path.GetRotationAtDistance(distanceOnPath, EndOfPathInstruction.Stop);
    }

    void CalculatePlayerDirection()
    {
        rightDir = (Waypoints[currentPoint + 1].position - Waypoints[currentPoint].position).normalized;
        leftDir = (Waypoints[currentPoint].position - Waypoints[currentPoint + 1].position).normalized;
    }

    #endregion
}
