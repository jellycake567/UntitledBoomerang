using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIPatrol : MonoBehaviour
{
    public enum State
    {
        Chase,
        Patrol
    }

    public State AIState;
    NavMeshAgent agent;

    [Header("Movement")]
    public float maxVelocityChange = 10f;
    public float speed = 2.0f;
    public float rotationAngle = 30.0f;
    public Transform groundCheck;
    public Transform target;
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float gravityScale = 3f;
    public float fallGravityMultiplier = 0.2f;

    bool mustPatrol = true;
    bool mustTurn;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (AIState == State.Chase)
        {
            Chase();
        }
        else if (AIState == State.Patrol)
        {
            if (mustPatrol)
            {
                Patrol();
            }
        }
    }

    void FixedUpdate()
    {
        //Gravity();

        if (mustPatrol)
        {
            Collider[] overlap = Physics.OverlapSphere(groundCheck.position, 0.1f, groundLayer);

            if (overlap.Length == 0)
                mustTurn = true;
            else
                mustTurn = false;
        }
    }

    void Chase()
    {
        //Vector3 dirToPlayer = (target.position - transform.position).normalized;
        //float angle = Vector3.Angle(transform.forward, dirToPlayer);

        //if (angle > 100f)
        //{
        //    Flip();
        //}

        // Create a path and set it based on a target position.
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);

        if (path.corners.Length > 1)
        {
            Debug.DrawLine(path.corners[0], path.corners[1], Color.red);

            Vector3 dir = path.corners[1] - path.corners[0];
            dir.y = 0f;

            // Calculate angle from AI to player
            float angle = Mathf.Acos(Vector3.Dot(transform.forward, dir.normalized)) * Mathf.Rad2Deg;

            // Rotation speed
            float rotSpeed = angle / rotationAngle;


            // Get Rotation
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotSpeed);

        }

        agent.SetDestination(target.position);
    }

    void Patrol()
    {
        if (mustTurn)
        {
            mustPatrol = false;
            Flip();
            mustPatrol = true;
        }

        Movement(transform.forward);
    }

    void Flip()
    {
        Vector3 rotation = transform.eulerAngles + Quaternion.Euler(new Vector3(0, 180f, 0)).eulerAngles;
        transform.rotation = Quaternion.Euler(rotation);
    }

    void Movement(Vector3 direction)
    {
        #region Calculate Velocity

        Vector3 targetVelocity = direction * speed;
        Vector3 rbVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Apply a force that attempts to reach our target velocity
        Vector3 velocity = rbVelocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;

        #endregion

        // Move AI
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
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
}
