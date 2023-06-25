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
    //public float speed;
    public float maxSpeed = 2;
    public float walkAccel = 1.5f;
    public bool isStaggered = false;
    public float staggerTimer;
    public float staggerTimerMax;
    public float attackSpeed;
    public float meleeAttackRange = 2;
    bool stopMoving = false;
    bool hasAttacked = false;
    public bool in3Dmode = true;

    Vector2 velocityXZ;

    float distFromPlayer = 0;
    int optionCount = 0;
    int choice = 0;
    Vector3 totalForce;

    Rigidbody rb;
    Animator animControl;

    
    //nav agent variable
    NavMeshAgent navAgent;
    Vector3 targetPoint;
    float initialStopDist;
    public float smallerStopDist = 3f;
    float initialAccel;
    public float runAccel = 3;
    public float runSpeed = 8f;

    //Wandering 3D variables
    public float wanderRadius = 3;
    public float circleDistance = 5;
    public float wanderJitter = 3;
    Vector3 wanderPreviousTarget;
    Vector3 target;
    bool isGroundAhead= true;
    public LayerMask groundMask;
    public LayerMask obstacleMask;
    private Vector3 lastGroundPos;
    public float walkSpeed;
    bool isObstacleNear = false;

    //debug
    bool temp;
    //wandering 3D
    Vector3 debug_wanderDistancePoint;
    float x;
    float z;
    float angle;
    Vector3 initalPos;
    Vector3 currentPos;

    RaycastHit hitInfo;
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
        wanderPreviousTarget = wanderPreviousTarget + transform.position * wanderRadius + transform.forward * circleDistance;
        debug_wanderDistancePoint = new Vector3();
        target = new Vector3();

        totalForce = new Vector3();
        bool temp = Physics.Raycast(transform.position, -transform.up, out hitInfo, 5, groundMask);

        initalPos = transform.position;
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
                //AIState = State.Standing;
                //rb.velocity = Vector3.zero;

                rb.velocity *= 0.98f; //magic number
                return;
            }

            velocityXZ.x = rb.velocity.x;
            velocityXZ.y = rb.velocity.z;

            //2D
            if (!in3Dmode)
            {
                if (CheckForObstacle(hitInfo))
                {
                    
                }

                if (velocityXZ.magnitude < maxSpeed)
                {
                    //targetPoint = transform.forward * Random.Range(2,6);
                    //navAgent.destination = transform.position + targetPoint;

                    rb.AddForce(transform.forward * 3, ForceMode.Acceleration);
                    

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
        if (isStaggered) 
        {
            animControl.SetTrigger("isStaggeredTrig");
            staggerTimer -= Time.deltaTime;
            if(staggerTimer <= 0)
            {
                isStaggered = false;
            }
            return;
        }
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

        debug_wanderDistancePoint = debug_wanderDistancePoint + transform.forward * circleDistance;
        

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

        if(!stopMoving)
        {
            navAgent.acceleration = runAccel;
            navAgent.speed = runSpeed;
            navAgent.destination = playerPos;

        }


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

    void OnDrawGizmos()
    {
        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(transform.position, debug_wanderDistancePoint);        
        //Gizmos.DrawWireSphere(debug_wanderDistancePoint, wanderRadius);
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(transform.position, target);
        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(initalPos, currentPos);
        
    }

    IEnumerator AttackCycle()
    {
        hasAttacked = true;

        animControl.SetTrigger("attack");
        stopMoving = true;
        yield return new WaitForSeconds(attackSpeed);
        stopMoving = false;
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

        //Vector3 randomDir = new Vector3();
        //randomDir.x = Random.Range(-1.0f, 1.0f);
        //randomDir.y = transform.position.y;
        //randomDir.z = 1 - randomDir.x;
        

        //newTarget = new Vector3();
        //apply jitter
        target.x = wanderPreviousTarget.x + Random.Range(-wanderJitter, wanderJitter);
        target.y = wanderPreviousTarget.y;
        target.z = wanderPreviousTarget.z + Random.Range(-wanderJitter, wanderJitter);

        


        Vector3 dirToNewTarget = new Vector3();
        dirToNewTarget = target - (transform.position + transform.forward * circleDistance);

        dirToNewTarget.Normalize();

        //constrict new target onto circle
        target = transform.position + dirToNewTarget * wanderRadius + transform.forward * circleDistance;

        Vector3 seekDir = (target - transform.position).normalized;


        float dotResult = 0;
        float distance = 0;
 
        if (CheckForObstacle(hitInfo))
        {
            //totalForce += ApplyAvoidSteering(hitInfo);
            //Debug.Log("x " + totalForce.x + " z " + totalForce.z);

            //dotResult = Vector3.Dot(, transform.right);
            //angle = Vector3.Angle(transform.forward, totalForce);

            Vector3 hitDirection = Vector3.zero;
            hitDirection = (hitInfo.point - transform.position).normalized;
            
            dotResult = Vector3.Dot(hitDirection, transform.right);
            angle = Vector3.Angle(transform.forward, hitDirection);
        
            distance = (hitInfo.point - transform.position).magnitude;
            
        }
        else
        {
            dotResult = Vector3.Dot(seekDir, transform.right);
            angle = Vector3.Angle(transform.forward, seekDir);

            if (dotResult < 0)
            {
                angle = -angle;
            }

        }


        transform.localEulerAngles += new Vector3(0, angle, 0) * Time.deltaTime;
        
        
        totalForce = transform.forward * 5;

        rb.AddForce(totalForce);
        rb.velocity = rb.velocity.normalized * walkSpeed; 
        //rb.velocity = transform.forward * walkSpeed; 

        

        //transform.rotation = Quaternion.LookRotation(dirToNewTarget);
        currentPos = transform.position;
        wanderPreviousTarget = target;
        totalForce = Vector3.zero;
    }

    bool CheckGround()
    {
        
        Vector3 direction = new Vector3();
        direction = transform.forward;
        direction.y = -0.020f;//magic number

        //Debug.DrawLine(transform.position, transform.position + direction * 5, Color.green);
        RaycastHit hit;
        if(Physics.Raycast(transform.position, direction, out hit, 5, groundMask))
        {
            lastGroundPos = hit.point;
            return true;
        }
        return false;
    }

    bool CheckForObstacle(RaycastHit hitInfo)
    {

        Vector3 direction = new Vector3();
        direction = transform.forward + transform.right;
        direction.Normalize();

        float raycastLength = 4;

        //right forward diagonal raycast
        Debug.DrawLine(transform.position, transform.position + direction * raycastLength, Color.white);       
        if (Physics.Raycast(transform.position, direction, out hitInfo,  raycastLength, obstacleMask))
        {
            return true;
        }

        //left forward diagonal raycast
        direction = transform.forward - transform.right;
        direction.Normalize();
        Debug.DrawLine(transform.position, transform.position + direction * raycastLength, Color.white);      
        if (Physics.Raycast(transform.position, direction, out hitInfo, raycastLength, obstacleMask))
        {
            return true;
        }

        //forward raycast
        Debug.DrawRay(transform.position, transform.forward * raycastLength, Color.white);
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, raycastLength, obstacleMask))
        {
            return true;
        }
        return false;
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
            //rb.velocity = Vector3.zero;
            //AIState = State.Standing;

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

    void SlowDownNearCliff()
    {
        
    }

    Vector3 ApplyAvoidSteering(RaycastHit hitInfo)
    {
        Vector3 dirFromTarget =  transform.position - hitInfo.point;
        dirFromTarget.Normalize();

        return dirFromTarget * -4;
    }


}


