using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using PathCreation;

public class EnemyNavigation : MonoBehaviour
{
    // Path
    public float maxDistancePath = 30.0f;
    public float adjustVelocity = 1.0f;
    public bool followPath = false;

    // Rotation
    public float rotationAngle = 30.0f;

    // Attack
    public float attackRadius = 3f;
    public float attackCooldown = 3f;
    float currentAttackCooldown;
    [HideInInspector]public bool isDashing = false;

    // References
    NavMeshAgent agent;
    public Transform target;
    public PathCreator pathCreator;
    public LayerMask groundMask;
    [HideInInspector] public Animation attackAnim;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        attackAnim = GetComponentInChildren<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDashing)
            Chase();

        Attack();
    }

    void Chase()
    {
        RaycastHit hit;
        Physics.Raycast(target.position, Vector3.down, out hit, 100f, groundMask);

        // Create a path and set it based on a target position.
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, hit.point, NavMesh.AllAreas, path);
        

        if (path.corners.Length > 1)
        {
            Debug.DrawLine(path.corners[0], path.corners[1], Color.red);

            // Get Target Direction
            Vector3 dir = path.corners[1] - path.corners[0];

            if (followPath)
            {
                Vector3 targetDir = CalculatePathFacingDir(transform.position, dir);
                Quaternion targetRot = Quaternion.LookRotation(targetDir);

                transform.rotation = targetRot;
            }
            else
            {
                // Calculate angle from AI to player
                float angle = Mathf.Acos(Vector3.Dot(transform.forward, dir.normalized)) * Mathf.Rad2Deg;

                // Rotation speed
                float rotSpeed = angle / rotationAngle;

                // Get Rotation
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotSpeed);
            }
        }

        // If Ai is near player stop chasing
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > attackRadius)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            agent.ResetPath();
        }
    }

    void Attack()
    {
        if (currentAttackCooldown <= 0f && !isDashing)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < attackRadius)
            {
                isDashing = true;

                currentAttackCooldown = attackCooldown;
                attackAnim.Play();
            }
        }
        else
        {
            currentAttackCooldown -= Time.deltaTime;
        }
    }




    public Vector3 CalculatePathFacingDir(Vector3 posOnPath, Vector3 directionToFace)
    {
        // Get Path dir AI is on
        float distance = pathCreator.path.GetClosestDistanceAlongPath(posOnPath);
        Vector3 pathDir = pathCreator.path.GetDirectionAtDistance(distance);

        // Angle to calculate knockback dir
        float angle = Vector3.Angle(pathDir.IgnoreYAxis(), directionToFace.IgnoreYAxis());
        return angle > 90f ? -pathDir : pathDir;
    }
}
