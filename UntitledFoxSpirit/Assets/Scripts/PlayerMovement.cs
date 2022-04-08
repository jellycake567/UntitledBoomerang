using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    CharacterController controller;

    [Header("Movement")]
    public float speed = 5.0f;
    public float turnSmoothTime2D = 0.03f;
    public float turnSmoothTime3D = 0.1f;
    private float turnSmoothVelocity;
    public bool camera3D = false;
    private Vector3 movementDirection;
    
    private float currentSpeed;

    [Header("Dash")]
    public float dashSpeed = 5.0f;
    public float dashLength = 4.0f;
    [Range(0f, 1f)] public float startDashAcc = 0.1f;   // Acceleration speed to dashspeed
    [Range(0f, 1f)] public float endDashAcc = 0.1f;     // Deceleration dashspeed to speed
    private float dashTimer;
    private float dashCounter;
    private bool isDashing;
    
    

    [Header("Jump")]
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

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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

        PlayerInput();

        CheckPlayerOnPath();
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

    void GroundCheck()
    {
        Vector3 point = transform.position + Vector3.down * 0.1f;
        Vector3 size = transform.localScale - new Vector3(0.5f, 0, 0.5f);
        Collider[] results = Physics.OverlapBox(point, size, Quaternion.identity, ~ignorePlayerMask);

        if (results.Length > 0)
        {
            isGrounded = true;

            hangCounter = hangTime;
        }
        else
        {
            isGrounded = false;

            hangCounter -= Time.deltaTime;
        }
    }

    float HeadCheck(float velocity)
    {
        Vector3 point = transform.position + Vector3.up * 0.1f;
        Vector3 size = transform.localScale - new Vector3(0.5f, 0, 0.5f);
        Collider[] results = Physics.OverlapBox(point, size, Quaternion.identity, ~ignorePlayerMask);

        if (results.Length > 0 && velocity > 0)
        {
            return 0f;
        }

        return velocity;
    }

    void PlayerInput()
    {
        Movement();
        Jump();
    }

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
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float camAngle = camera3D ? mainCamera.eulerAngles.y : pathAngle;
            float turnSmoothTime = camera3D ? turnSmoothTime3D : turnSmoothTime2D;

            // Player movement/rotation direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camAngle;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f); 

            // Move player
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            if (!isDashing)
                controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        Dash(pathAngle, direction);
    }

    void Jump()
    {
        velocity += gravity * gravityScale * Time.deltaTime;

        // Player is grounded
        if (isGrounded && velocity < 0)
        {
            velocity = 0f;

            canDoubleJump = true;
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
        if (jumpBufferCounter >= 0f && hangCounter > 0f || Input.GetKeyDown(KeyCode.Space) && canDoubleJump)
        {
            velocity = Mathf.Sqrt(-2 * (gravity * gravityScale) * jumpHeight);

            if (hangCounter <= 0f && !isGrounded && canDoubleJump)
            {
                canDoubleJump = false;
            }
            else if (hangCounter > 0f)
            {
                hangCounter = 0f; // So you don't triple jump
            }
        }

        // If we bump our head set velocity to 0 (so we don't float)
        velocity = HeadCheck(velocity);

        if (isDashing)
            velocity = 0f;

        // Player jump
        controller.Move(new Vector3(0, velocity, 0) * Time.deltaTime);
    }

    void Dash(float pathAngle, Vector3 movement)
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                movementDirection = movement;
                currentSpeed = speed;
                dashTimer = dashLength / dashSpeed;
                dashCounter = dashTimer;

                isDashing = true;
            }            
        }
        else if (isDashing)
        {
            dashCounter -= Time.deltaTime;

            float startAcc = dashTimer * (1f - startDashAcc);
            float endAcc = dashTimer * endDashAcc;

            if (dashCounter >= startAcc)
            {
                float acceleration = (dashCounter - startAcc) / (dashTimer - startAcc);

                currentSpeed = Mathf.Lerp(dashSpeed, speed, acceleration);
            }
            else if (dashCounter <= endAcc && dashCounter >= 0f)
            {
                float acceleration = dashCounter / endAcc;

                currentSpeed = Mathf.Lerp(speed, dashSpeed, acceleration);
            }
            else if (dashCounter > 0f && dashCounter <= dashTimer)
            {
                currentSpeed = dashSpeed;
            }
        }

        if (dashCounter > 0 && isDashing)
        {
            float targetAngle = Mathf.Atan2(movementDirection.x, movementDirection.z) * Mathf.Rad2Deg + pathAngle;

            // Move player
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }
        else
        {
            isDashing = false;
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
