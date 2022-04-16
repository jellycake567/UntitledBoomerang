using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float playerSpeed = 5.0f;
    public float turnSmoothTime2D = 0.03f;
    public float turnSmoothTime3D = 0.1f;
    private float turnSmoothVelocity;
    public float maxVelocityChange = 10f;
    public bool camera3D = false;
    private Vector3 movementDirection;
    private float currentSpeed;

    [Header("Dash")]
    public float dashTime = 5.0f;
    public float dashDistance = 4.0f;
    [Range(0f, 1f)] public float startDashAcc = 0.1f;   // Acceleration speed to dashspeed
    [Range(0f, 1f)] public float endDashAcc = 0.1f;     // Deceleration dashspeed to speed
    private float dashTimer;
    private float dashCounter;
    private bool isDashing;

    [Header("Jump")]
    public float jumpCooldown = 0.2f;
    private float jumpCounter;
    public float jumpHeight = 5f;
    public float jumpBufferLength = 0.1f;   // Detect jump input before touching the ground
    public float hangTime = 0.2f;           // Allow you to jump when you walk off platform
    public float gravityScale = 3f;
    public float gravity = -9.81f;
    public LayerMask ignorePlayerMask;
    private float velocity;
    private bool isGrounded;
    private float hangCounter;
    private bool canDoubleJump;
    private float jumpBufferCounter;

    [Header("Camera")]
    public float camRotationSpeed2D = 0.2f;
    public Transform mainCamera;

    [Header("References")]
    public Transform path;
    public CinemachineVirtualCamera virtualCam2D;
    public CinemachineFreeLook virtualCam3D;

    private int currentPoint = 0;
    List<Transform> Waypoints = new List<Transform>();

    const float REDUCE_SPEED = 1.414214f;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        Physics.gravity *= gravityScale;


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
        CheckPlayerOnPath();
    }

    void FixedUpdate()
    {
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

            // Player is moving diagonally
            if (targetVelocity.z == 1 && targetVelocity.x == 1 || targetVelocity.z == 1 && targetVelocity.x == -1 || targetVelocity.z == -1 && targetVelocity.x == 1 || targetVelocity.z == -1 && targetVelocity.x == -1)
            {
                targetVelocity = -targetVelocity * playerSpeed / REDUCE_SPEED;
            }
            else
            {
                targetVelocity = -targetVelocity * playerSpeed;
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
        // Can jump when leaving the ground
        if (isGrounded && jumpCounter <= 0f)
        {
            canDoubleJump = true;

            hangCounter = hangTime;
        }
        else
        {
            hangCounter -= Time.deltaTime;

            // Jumping cooldown
            jumpCounter -= Time.deltaTime;
        }

        // Jump before you touch the ground
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferLength;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        
        // Player jump input
        if (jumpBufferCounter > 0f && hangCounter > 0f || Input.GetKeyDown(KeyCode.Space) && canDoubleJump)
        {
            // Calculate jump velocity
            if (rb.velocity.y >= 0)
            {
                velocity = Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight);
                velocity += -rb.velocity.y; // When double jumping cancel out your first jump force
            }
            else
            {
                velocity = Mathf.Sqrt(-2 * Physics.gravity.y * jumpHeight);
                velocity += Mathf.Abs(rb.velocity.y); // When falling cancel out gravity force
            }

            // Jump
            rb.AddForce(new Vector3(0, velocity, 0), ForceMode.Impulse);

            // Set jump cooldown
            jumpCounter = jumpCooldown;

            if (hangCounter <= 0f && !isGrounded && canDoubleJump)
            {
                canDoubleJump = false;
            }
            if (hangCounter > 0f)
            {
                hangCounter = 0f; // So you don't triple jump
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

                //currentSpeed = playerSpeed;
                //dashTimer = dashLength / dashSpeed;
                //dashCounter = dashTimer;

                //isDashing = true;
            }
        }

        //if (isDashing)
        //{
        //    dashCounter -= Time.deltaTime;

        //    float startAcc = dashTimer * (1f - startDashAcc);
        //    float endAcc = dashTimer * endDashAcc;

        //    if (dashCounter >= startAcc)
        //    {
        //        // Accelerate to dash speed 

        //        float acceleration = (dashCounter - startAcc) / (dashTimer - startAcc);

        //        currentSpeed = Mathf.Lerp(dashSpeed, playerSpeed, acceleration);
        //    }
        //    else if (dashCounter <= endAcc && dashCounter >= 0f)
        //    {
        //        // Decelerate to normal speed

        //        float acceleration = dashCounter / endAcc;

        //        currentSpeed = Mathf.Lerp(playerSpeed, dashSpeed, acceleration);
        //    }
        //    else if (dashCounter > 0f && dashCounter <= dashTimer)
        //    {
        //        // Maintain dash speed

        //        currentSpeed = dashSpeed;
        //    }
        //}

        //// If dash is active
        //if (dashCounter > 0 && isDashing)
        //{
        //    rb.useGravity = false;


        //    rb.velocity = targetVelocity * currentSpeed;
            
        //}
        //else
        //{
        //    rb.useGravity = true;
        //    isDashing = false;
        //}
    }

    IEnumerator Dash(Vector3 direction)
    {
        isDashing = true;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        float speed = dashDistance / dashTime;

        rb.AddForce(direction * speed, ForceMode.Impulse);
        rb.useGravity = false;

        yield return new WaitForSeconds(dashTime);

        isDashing = false;
        rb.useGravity = true;
    }

    #endregion

    void GroundCheck()
    {
        Vector3 point = transform.position + Vector3.down;
        Vector3 size = new Vector3(0.6f, 0.1f, 0.6f);
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
