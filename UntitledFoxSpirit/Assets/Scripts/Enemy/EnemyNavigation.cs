using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using PathCreation;
using System.ComponentModel;
using NUnit.Framework.Internal;

enum State
{
    Chase,
    Attack
}

public class EnemyNavigation : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] float rotationAngle = 30.0f;
    [SerializeField] State state = State.Chase;

    [Header("Attack")]
    [SerializeField] float attackRadius = 3f;
    [SerializeField] float attackCooldown = 3f;
    private float currentAttackDuration;
    private bool isAttacking = false;
    private BoxCollider hitbox;

    [Header("Path")]
    [SerializeField] float maxDistancePath = 30.0f;
    [SerializeField] float adjustVelocity = 1.0f;
    [SerializeField] bool followPath = false;

    [Header("References")]
    [SerializeField] Transform target;
    [SerializeField] PathCreator pathCreator;
    [SerializeField] LayerMask groundMask;
    private NavMeshAgent agent;
    private Animator animController;

    #region Internal Variables

    private bool canMove = true;
    

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        animController = GetComponent<Animator>();
        hitbox = GetComponentInChildren<BoxCollider>();

        ChangeState(state);
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Chase)
            Chase();

        if (state == State.Attack)
            Attack();
    }

    void Chase()
    {
        #region Behaviour

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

        #endregion

        #region Condition

        // If Ai is near player stop chasing
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > attackRadius)
        {
            animController.SetBool("isMoving", true);
            agent.SetDestination(target.position);
        }
        else if (distance <= attackRadius)
        {
            
            ChangeState(State.Attack);
            return;
        }

        currentAttackDuration -= Time.deltaTime;

        #endregion
    }

    void Attack()
    {
        if (animController.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            if (!isAttacking)
                isAttacking = true;
        }
        else
        {
            if (isAttacking)
                ChangeState(State.Chase);
        }
    }

    void ChangeState(State stateToChange)
    {
        // Initialize state
        if (stateToChange == State.Chase)
        {
            
        }
        else if (stateToChange == State.Attack)
        {
            agent.enabled = false;
            animController.SetTrigger("Attack");
            hitbox.enabled = true;
        }

        // Reset state
        if (state == State.Chase)
        {
            agent.ResetPath(); // Stop moving
            agent.velocity = Vector3.zero; // Reset velocity
            animController.SetBool("isMoving", false);
        }
        else if (state == State.Attack)
        {
            agent.enabled = true;
            isAttacking = false;
            hitbox.enabled = false;
        }

        state = stateToChange;
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
