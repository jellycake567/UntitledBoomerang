using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHuman : MonoBehaviour
{
    public GameManager GM;
    private Vector3 playerPos;
    float detectionLength = 7;
    float decisionTimer = 0;
    public float decisionTimerMin = 0;
    public float decisionTimerMax = 0;
    public float speed;
    //public float velocity;
    public float meleeAttackRange = 2;
    float initialStopDist;
    float smallerStopDist = 1.40f;
    bool hasAttacked = false;
    bool in3Dmode = false;

    float initialAccel;
    public float runAccel = 3;

    float distFromPlayer = 0;
    int optionCount = 0;
    int choice = 0;
    public float runSpeed = 8f;

    Rigidbody rb;
    Animator animControl;


    NavMeshAgent navAgent;
    Vector3 targetPoint;

    //Wandering 3D variables
    float wanderRadius = 3;
    float wanderDistance = 2;
    float wanderJitter = 4;
    Vector3 wanderPreviousTarget;
    Vector3 newTarget;
    bool isGroundAhead= true;
    public LayerMask groundMask;
    public LayerMask obstacleMask;


    //debug
    bool temp;
    //wandering 3D
    Vector3 debug_wanderDistancePoint;
    float x;
    float z;
    float angle;

    public enum State
    {
        Standing,
        Wander,
        Combat
    }

    public State AIState;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();
        animControl = GetComponent<Animator>();
        initialStopDist = navAgent.stoppingDistance;
        initialAccel = navAgent.acceleration;

        wanderPreviousTarget.x = transform.position.x + Random.Range(-wanderRadius, wanderRadius);
        wanderPreviousTarget.y = transform.position.y;
        wanderPreviousTarget.z = transform.position.z + Random.Range(-wanderRadius, wanderRadius);
        wanderPreviousTarget = wanderPreviousTarget + transform.forward * wanderDistance;
        debug_wanderDistancePoint = new Vector3();
        newTarget = new Vector3();
    }


    private void FixedUpdate()
    {
        if (AIState == State.Standing)
        {
            FindPlayer();
        }
        else if (AIState == State.Wander)
        {

            if (!isGroundAhead)
            {
                //temporary code
                AIState = State.Standing;
                rb.velocity = Vector3.zero;
                return;
            }

            //2D
            if (!in3Dmode)
            {
                if(rb.velocity.z == 0 && rb.velocity.x == 0)
                {
                    //targetPoint = transform.forward * Random.Range(2,6);
                    //navAgent.destination = transform.position + targetPoint;

                    rb.AddForce(transform.forward * 3, ForceMode.Impulse);

                }
                FindPlayer();
                return;
            }

            WanderIn3D();
            FindPlayer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (AIState == State.Standing)
        {
            decisionTimer -= Time.deltaTime;
            animControl.SetBool("isWalking", false);
            animControl.SetBool("isChasing", false);
            AdjustForwardBlend(AIState);
            StandOrMove();
        }
        else if (AIState == State.Combat)
        {
            AdjustForwardBlend(AIState);
            animControl.SetBool("isChasing", true);
            Attack();
        }
        else if (AIState == State.Wander)
        {
            decisionTimer -= Time.deltaTime;
            animControl.SetBool("isWalking", true);
            animControl.SetBool("isChasing", false);
            AdjustForwardBlend(AIState);
            isGroundAhead = CheckGround();
            StandOrMove();
        }

        //Debug.Log("nav agent " + navAgent.velocity);
        //debuging stuff
        debug_wanderDistancePoint.x = transform.position.x;
        debug_wanderDistancePoint.y = transform.position.y;
        debug_wanderDistancePoint.z = transform.position.z;

        debug_wanderDistancePoint = debug_wanderDistancePoint + transform.forward * wanderDistance;

    }


    void Attack()
    {
        rb.velocity = Vector3.zero;
        playerPos = GM.playerPos;
        distFromPlayer = Vector3.Distance(transform.position, playerPos);
        Vector3 dirToPlayer = (playerPos - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle > 100f)
        {
            TurnAround();
        }

        navAgent.acceleration = runAccel;
        navAgent.speed = runSpeed;
        navAgent.destination = playerPos;


        //Debug.Log("Distance from Player" + distFromPlayer);
        //attack the player when within range
        if (distFromPlayer < meleeAttackRange && !hasAttacked)
        {
            
            StartCoroutine(AttackCycle());
        }
        else if (distFromPlayer > navAgent.stoppingDistance)//outside of close proximity
        {
            animControl.SetTrigger("chaseTrig");
            //animControl.SetFloat("forwardBlend", Mathf.Clamp01(navAgent.speed));
        }
        else if (distFromPlayer < navAgent.stoppingDistance) //within close proximity
        {
            navAgent.stoppingDistance = smallerStopDist;
            animControl.SetTrigger("combatIdleTrig");
        }

        if(distFromPlayer > navAgent.stoppingDistance + 3.0f)
        {
            navAgent.stoppingDistance = initialStopDist;
        }

    }
    
    int decideRandomly(int optionCount)
    {
        int decision = 0;
        decision = Random.Range(0, optionCount);
        //Debug.Log(decision);
        return decision;  
    }

    void FindPlayer()
    { 
        
        //Debug.DrawRay(transform.position, transform.forward * detectionLength, Color.red);

        playerPos = GM.playerPos;
        distFromPlayer = Vector3.Distance(transform.position, playerPos);
        Vector3 dirToPlayer = (playerPos - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if(angle < 45 && distFromPlayer < detectionLength)
        {
            //Player detected
            Debug.Log("Detected");
                    
            AIState = State.Combat;
        }
    }

    void TurnAround()
    {
        Vector3 rotation = transform.eulerAngles + Quaternion.Euler(new Vector3(0, 180f, 0)).eulerAngles;
        transform.rotation = Quaternion.Euler(rotation);
    }

    void Move(Vector3 direction)
    {
        Vector3 targetVelocity = direction * speed;

        // Move AI
        rb.AddForce(targetVelocity, ForceMode.VelocityChange);
    }


    void OnDrawGizmos()
    {
        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(transform.position, debug_wanderDistancePoint);        
        //Gizmos.DrawWireSphere(debug_wanderDistancePoint, wanderRadius);
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(transform.position, newTarget);
  
    }

    IEnumerator AttackCycle()
    {
        hasAttacked = true;

        animControl.SetTrigger("attack");
        yield return new WaitForSeconds(5f); 

        hasAttacked = false;
    }

    void AdjustForwardBlend(State AIstate)
    {
        Vector3 velocity = new Vector3();
        float velocityMag = 0;
        switch (AIState)
        {
            case State.Wander:
                velocity.x = rb.velocity.x;
                velocity.y = 0;
                velocity.z = rb.velocity.z;
                velocityMag = Vector3.Magnitude(velocity);
                animControl.SetFloat("forwardBlend", Mathf.Clamp(velocityMag, 0, 1));
                break;

            case State.Standing:
                velocity.x = 0;
                velocity.y = 0;
                velocity.z = 0;
                velocityMag = Vector3.Magnitude(velocity);
                animControl.SetFloat("forwardBlend", Mathf.Clamp(velocityMag, 0, 1));
                break;

            case State.Combat:
                velocity.x = navAgent.velocity.x;
                velocity.y = 0;
                velocity.z = navAgent.velocity.z;
                velocityMag = Vector3.Magnitude(velocity);
                animControl.SetFloat("forwardBlend", Mathf.Clamp(velocityMag, 0, 1));
                break;

        }

        

    }
 

    void WanderIn3D()
    {

        if(!isGroundAhead)
        {
            //temporary code
            AIState = State.Standing;
            rb.velocity = Vector3.zero;
            return;
        }

        //newTarget = new Vector3();
        newTarget.x = wanderPreviousTarget.x + Random.Range(-wanderJitter, wanderJitter);
        newTarget.y = wanderPreviousTarget.y;
        newTarget.z = wanderPreviousTarget.z + Random.Range(-wanderJitter, wanderJitter);

        Vector3 dirFromObjtoNewTarget = new Vector3();
        dirFromObjtoNewTarget = newTarget - transform.position;
        dirFromObjtoNewTarget.Normalize();

        //move new target onto circle
       
        newTarget = transform.position  + dirFromObjtoNewTarget * wanderRadius + transform.forward * wanderDistance;

        if(CheckForObstacle())
        {

        }

        rb.velocity = transform.forward * 3;

        float dotResult = Vector3.Dot(dirFromObjtoNewTarget, transform.right);
        angle = Vector3.Angle(transform.forward, dirFromObjtoNewTarget);
        if(dotResult < 0)
        {
            angle = -angle;
        }
        transform.Rotate(new Vector3(0, angle, 0) * Time.deltaTime, Space.Self);

        wanderPreviousTarget = newTarget;
    }

    bool CheckGround()
    {
        
        Vector3 direction = new Vector3();
        direction = transform.forward;
        direction.y = -0.080f;

        Debug.DrawLine(transform.position, transform.position + direction * 5, Color.green);
        RaycastHit hit;
        if(Physics.Raycast(transform.position, direction, out hit, 5, groundMask))
        {
            return true;
        }
        return false;
    }

    bool CheckForObstacle()
    {

        Vector3 direction = new Vector3();
        direction = transform.forward + transform.right;
        direction.Normalize();

        float raycastLength = 4;

        //right forward diagonal raycast
        Debug.DrawLine(transform.position, transform.position + direction * raycastLength, Color.white);
        if (Physics.Raycast(transform.position, direction, raycastLength, obstacleMask))
        {
            return true;
        }

        //left forward diagonal raycast
        direction.x = -direction.x;
        Debug.DrawLine(transform.position, transform.position + direction * raycastLength, Color.white);
        if (Physics.Raycast(transform.position, direction, raycastLength, obstacleMask))
        {
            return true;
        }

        //forward raycast
        Debug.DrawRay(transform.position, transform.forward * raycastLength, Color.white);
        if (Physics.Raycast(transform.position, transform.forward, raycastLength, obstacleMask))
        {
            return true;
        }
        return true;
    }

    void StandOrMove()
    {

        if (decisionTimer <= 0)
        {
            //set number of options
            optionCount = 2;

            //Does the AI move?
            choice = decideRandomly(optionCount);

            if (choice == 0) //yes
            {
                AIState = State.Wander;
                decisionTimer = Random.Range(decisionTimerMin, decisionTimerMax);
                return;
            }
            
            //no, dont move  
            rb.velocity = Vector3.zero;
            AIState = State.Standing;

            //does AI turn around?
            if (!in3Dmode)
            {
                choice = decideRandomly(optionCount);

                if (choice == 0) //yes
                {                    
                    TurnAround();
                }
            }

            decisionTimer = Random.Range(decisionTimerMin, decisionTimerMax);
        }
    }
}


