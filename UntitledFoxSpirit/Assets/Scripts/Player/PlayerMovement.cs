using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")] public float humanSpeed = 5.0f;
    [Header("Movement")] public float foxSpeed = 5.0f;

    [Header("Movement")]
    public float turnSmoothTime2D = 0.03f;
    public float turnSmoothTime3D = 0.1f;
    private float turnSmoothVelocity;
    public float maxVelocityChange = 10f;
    public bool camera3D = false;
    private bool isFox;

    [Header("Dash")]
    public float humanDashTime = 5.0f;
    public float humanDashDistance = 4.0f;

    [Header("Dash")]
    public float foxDashTime = 5.0f;
    public float foxDashDistance = 4.0f;
    private bool isDashing;

    [Header("Jump")] public float humanJumpHeight = 5f;
    [Header("Jump")] public float foxJumpHeight = 5f;

    [Header("Jump")]
    public float jumpCooldown = 0.2f;
    private float jumpCounter;
    public float jumpBufferTime = 0.1f;   // Detect jump input before touching the ground
    public float jumpCoyoteTime = 0.2f;   // Allow you to jump when you walk off platform
    private float jumpHangCounter;
    private float jumpBufferCounter;
    private bool canDoubleJump;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float gravityScale = 3f;
    public float fallGravityMultiplier = 0.2f;
    public LayerMask ignorePlayerMask;
    private float velocity;
    private bool isGrounded;

    [Header("Camera")]
    public float camRotationSpeed2D = 0.2f;
    public Transform mainCamera;

    [Header("References")]
    public Transform path;
    public CinemachineVirtualCamera virtualCam2D;
    public CinemachineFreeLook virtualCam3D;
    public GameObject human;
    public GameObject fox;
    public Animation attack;

    private int currentPoint = 0;
    List<Transform> Waypoints = new List<Transform>();

    const float REDUCE_SPEED = 1.414214f;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        #region Waypoint

        // "path (transform)" magically supply "all of its children" when it is in a situation such as a foreach
        foreach (Transform child in path)
        {
            Waypoints.Add(child);
        }

        Vector3 waypoint = Waypoints[currentPoint].position;
        transform.position = waypoint;

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        VirtualCamUpdate();

        GroundCheck();

        Jump();
        DashInput();
        ChangeForm();
        Attack();

        CheckPlayerOnPath();
    }

    void FixedUpdate()
    {
        Gravity();

        Movement();
    }

    #region Player Movement

    void Movement()
    {
        // Path direction
        Vector3 pathDir = Waypoints[currentPoint + 1].position - Waypoints[currentPoint].position;
        float pathAngle = Quaternion.LookRotation(pathDir).eulerAngles.y - 90f;

        // Is 2d cam on
        if (!camera3D)
        {
            // Rotate camera
            Vector3 camEulerAngle = mainCamera.rotation.eulerAngles;
            virtualCam2D.transform.rotation = Quaternion.Lerp(mainCamera.rotation, Quaternion.Euler(camEulerAngle.x, pathAngle, camEulerAngle.z), camRotationSpeed2D);
        }

        // Get input
        Vector3 targetVelocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 direction = targetVelocity.normalized;

        if (direction.magnitude >= 0.1f)
        {
            #region Player Rotation

            float camAngle = camera3D ? mainCamera.eulerAngles.y : pathAngle;
            float turnSmoothTime = camera3D ? turnSmoothTime3D : turnSmoothTime2D;

            // Player movement/rotation direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camAngle;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            #endregion

            #region Calculate Velocity

            float speed = isFox ? foxSpeed : humanSpeed;

            // Player is moving diagonally
            if (targetVelocity.z == 1 && targetVelocity.x == 1 || targetVelocity.z == 1 && targetVelocity.x == -1 || targetVelocity.z == -1 && targetVelocity.x == 1 || targetVelocity.z == -1 && targetVelocity.x == -1)
            {
                targetVelocity = -targetVelocity * speed / REDUCE_SPEED;
            }
            else
            {
                targetVelocity = -targetVelocity * speed;
            }

            
            Vector3 rbVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rbVelocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;

            #endregion

            if (!isDashing)
                rb.AddForce(velocityChange, ForceMode.VelocityChange);

        }
        
    }

    void Jump()
    {

        #region Coyote and Jump Buffer

        // Can jump when leaving the ground
        if (isGrounded && jumpCounter <= 0f)
        {
            canDoubleJump = true;

            jumpHangCounter = jumpCoyoteTime;
        }
        else
        {
            jumpHangCounter -= Time.deltaTime;

            // Jumping cooldown
            jumpCounter -= Time.deltaTime;
        }

        // Jump before you touch the ground
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
        if (jumpBufferCounter > 0f && jumpHangCounter > 0f || Input.GetKeyDown(KeyCode.Space) && canDoubleJump)
        {
            float jumpHeight = isFox ? foxJumpHeight : humanJumpHeight;

            #region Velocity

            // Calculate jump velocity
            if (rb.velocity.y >= 0)
            {
                velocity = Mathf.Sqrt(-2 * gravity * gravityScale * jumpHeight);
                velocity += -rb.velocity.y; // When double jumping cancel out your first jump force
            }
            else
            {
                velocity = Mathf.Sqrt(-2 * gravity * gravityScale * jumpHeight);
                velocity += Mathf.Abs(rb.velocity.y); // When falling cancel out gravity force
            }

            #endregion

            // Jump
            rb.AddForce(new Vector3(0, velocity, 0), ForceMode.Impulse);

            // Set jump cooldown
            jumpCounter = jumpCooldown;

            if (jumpHangCounter <= 0f && !isGrounded && canDoubleJump)
            {
                canDoubleJump = false;
            }
            if (jumpHangCounter > 0f)
            {
                jumpHangCounter = 0f; // So you don't triple jump
            }
        }

        if (isDashing)
            velocity = 0f;
    }

    void DashInput()
    {
        // Dash input
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            // If player is moving left or right
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

                StartCoroutine(Dash(-direction));
            }
        }
    }

    IEnumerator Dash(Vector3 direction)
    {
        isDashing = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        float dashDistance = isFox ? foxDashDistance : humanDashDistance;
        float dashTime = isFox ? foxDashTime : humanDashTime;

        float speed = dashDistance / dashTime;

        rb.AddForce(direction * speed, ForceMode.Impulse);
        rb.useGravity = false;

        yield return new WaitForSeconds(dashTime);

        isDashing = false;
        rb.useGravity = true;
    }

    void Gravity()
    {
        if (rb.velocity.y < 0f)
        {
            rb.AddForce(new Vector3(0, gravity, 0) * rb.mass * gravityScale * fallGravityMultiplier);
        }
        else
        {
            rb.AddForce(new Vector3(0, gravity, 0) * rb.mass * gravityScale);
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
}
