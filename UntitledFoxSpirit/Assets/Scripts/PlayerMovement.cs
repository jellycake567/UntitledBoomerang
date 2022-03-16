using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    CharacterController controller;

    [Header("Movement")]
    public float speed = 5.0f;
    public float turnSmoothTime = 1.0f;
    private float turnSmoothVelocity;

    [Header("Jump")]
    public float jumpForce = 5f;
    public float gravityScale = 3f;
    public float gravity = -9.81f;
    public Transform groundCheck;
    public LayerMask ignorePlayerMask;
    private float velocity;
    private Vector3 surfacePosition;

    bool isGrounded;

    public Transform cam;
    public Transform path;

    private int currentPoint = 0;

    List<Transform> Waypoints = new List<Transform>();

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();

        // "path (transform)" magically supply "all of its children" when it is in a situation such as a foreach
        foreach (Transform child in path)
        {
            Waypoints.Add(child);
        }

        Vector3 waypoint = Waypoints[currentPoint].position;

        transform.position = waypoint;
    }

    // Update is called once per frame
    void Update()
    {
        GroundCheck();

        PlayerInput();

        CheckPlayerOnPath();
    }


    void GroundCheck()
    {
        Vector3 point = transform.position + Vector3.down * 0.2f;
        Vector3 size = transform.localScale;
        Collider[] results = Physics.OverlapBox(point, size, Quaternion.identity, ~ignorePlayerMask);

        if (results.Length > 0)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    void PlayerInput()
    {
        Movement();
        Jump();
    }

    void Movement()
    {
        // Get input
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Path direction
            Vector3 pathDir = Waypoints[currentPoint + 1].position - Waypoints[currentPoint].position;

            // Player movement/rotation direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg - 90f + Quaternion.LookRotation(pathDir).eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move player
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir * speed * Time.deltaTime);
        }
    }

    void Jump()
    {
        velocity += gravity * gravityScale * Time.deltaTime;

        if (isGrounded && velocity < 0)
        {
            velocity = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity = jumpForce;
        }

        controller.Move(new Vector3(0, velocity, 0) * Time.deltaTime);
    }

    void CheckPlayerOnPath()
    {
        Vector3 waypoint1 = new Vector3(Waypoints[currentPoint].position.x, 0f, Waypoints[currentPoint].position.z);
        Vector3 waypoint2 = new Vector3(Waypoints[currentPoint + 1].position.x, 0f, Waypoints[currentPoint + 1].position.z);
        Vector3 playerPos = new Vector3(transform.position.x, 0f, transform.position.z);

        float maxDistance = Vector3.Distance(waypoint1, waypoint2);

        // Going right
        float rightDis = Vector3.Distance(waypoint1, playerPos);

        // Going Left
        float leftDis = Vector3.Distance(waypoint2, playerPos);

        if (leftDis >= maxDistance && rightDis <= maxDistance)
        {
            // Going left
            if (currentPoint > 0)
            {
                currentPoint--;
            }
        }
        else if (rightDis >= maxDistance && leftDis <= maxDistance)
        {
            // Going Right
            if (currentPoint < Waypoints.Count - 2)
            {
                currentPoint++;
            }
        }
    }
}
