using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    CharacterController controller;

    public float speed = 5.0f;
    public float turnSmoothTime = 1.0f;
    private float turnSmoothVelocity;

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
        Movement();
        CheckPlayerOnPath();
    }

    void CheckPlayerOnPath()
    {
        Vector3 waypoint1 = Waypoints[currentPoint].position;
        Vector3 waypoint2 = Waypoints[currentPoint + 1].position;

        float maxDistance = Vector3.Distance(waypoint1, waypoint2);
        float leftDis = Vector3.Distance(waypoint1, transform.position);
        float rightDis = Vector3.Distance(waypoint2, transform.position);

        if (rightDis >= maxDistance)
        {
            // Going left
            if (currentPoint > 0)
            {
                currentPoint--;
            }
        }
        else if (leftDis >= maxDistance)
        {
            // Going Right
            if (currentPoint < Waypoints.Count - 2)
            {
                currentPoint++;
            }
        }
    }

    void Movement()
    {
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if (direction.magnitude >= 0.1f)
        {
            Vector3 pathDir = Waypoints[currentPoint + 1].position - Waypoints[currentPoint].position;

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg - 90f + Quaternion.LookRotation(pathDir).eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir * speed * Time.deltaTime);
        }
    }
}
