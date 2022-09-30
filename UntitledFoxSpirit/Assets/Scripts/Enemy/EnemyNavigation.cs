using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using PathCreation;

public class EnemyNavigation : MonoBehaviour
{
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
                //GetComponentInChildren<BoxCollider>().isTrigger = true;
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


    void OnTriggerStay(Collider other)
    {
        float distance = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        Vector3 pathDir = pathCreator.path.GetDirectionAtDistance(distance);
        pathDir.y = 0f;

        Vector3 dir = other.ClosestPointOnBounds(target.position) - transform.position;
        dir.y = 0f;

        float angle = Vector3.Angle(pathDir, dir);

        dir = angle > 90f ? -pathDir : pathDir;
        

        target.gameObject.GetComponent<PlayerMovement>().TakeDamage(dir.normalized);
    }
}
