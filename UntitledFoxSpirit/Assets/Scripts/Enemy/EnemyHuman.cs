using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHuman : MonoBehaviour
{
    public GameManager GM;
    private Vector3 playerPos;
    float detectionLength = 4;
    float decisionTimer = 0;
    public float decisionTimerMin = 0;
    public float decisionTimerMax = 0;
    public float speed;
    //public float velocity;
    public float meleeAttackRange = 2;
    float initialStopDist;
    float smallerStopDist = 1.40f;
    bool hasAttacked = false;
    bool in3Dmode = true;

    float initialAccel;
    public float runAccel = 3;

    float distFromPlayer = 0;
    int optionCount = 0;
    int choice = 0;
    float moveSpeed = 3f;
    Rigidbody rb;
    Animator animControl;

    NavMeshAgent navAgent;
    Vector3 targetPoint;

    //Wandering 3D variables
    float wanderRadius = 3;
    float wanderDistance = 5;
    float wanderJitter = 4;
    Vector3 wanderPreviousTarget;
    Vector3 newTarget;

    //debug wandering 3D
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

        wanderPreviousTarget.x = Random.Range(-wanderRadius, wanderRadius);
        wanderPreviousTarget.y = transform.position.y;
        wanderPreviousTarget.z = Random.Range(-wanderRadius, wanderRadius);
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
            //2D
            if(!in3Dmode)
            {
                if(navAgent.velocity == Vector3.zero)
                {
                    targetPoint = transform.forward * Random.Range(2,6);
                    navAgent.destination = transform.position + targetPoint;
                    FindPlayer();
                }
                return;
            }

            WanderIn3D();

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
            Standing();
        }
        else if (AIState == State.Combat)
        {
            animControl.SetBool("isChasing", true);
            Attack();
        }
        else if (AIState == State.Wander)
        {
            decisionTimer -= Time.deltaTime;
            animControl.SetBool("isWalking", true);
            animControl.SetBool("isChasing", false);
            AdjustForwardBlend(AIState);
            Wander();
        }

        //Debug.Log("nav agent " + navAgent.velocity);

        debug_wanderDistancePoint.x = transform.position.x;
        debug_wanderDistancePoint.y = transform.position.y;
        debug_wanderDistancePoint.z = transform.position.z;

        debug_wanderDistancePoint = debug_wanderDistancePoint + transform.forward * wanderDistance;

        //sin(theta) = o/h

        //Debug.Log(newTarget.y);
    }

    void Standing()
    {
        
     
        if (decisionTimer <= 0)
        {
            //Does the AI move?
            optionCount = 2;
            choice = decideRandomly(optionCount);
            if (choice == 0) //yes
            {
                AIState = State.Wander;
            }
            else if (choice == 1) //no
            {
                //dont move
                rb.velocity = Vector3.zero;
                AIState = State.Standing;

                choice = decideRandomly(optionCount);

                //does AI turn around?
                if (!in3Dmode)
                {
                    if (choice == 0) //yes
                    {
                        //turn around
                        Flip();
                    }

                }
            }


            decisionTimer = Random.Range(decisionTimerMin, decisionTimerMax);
        }


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
            Flip();
        }

        navAgent.acceleration = runAccel;
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

        if (!hasAttacked)
        {
            

        }

    }
    void Wander()
    {
        if (decisionTimer <= 0)
        {
            //move
            //optionCount = 2;
            choice = decideRandomly(optionCount);
            //Debug.Log(choice);

            if (choice == 0)
            {
                AIState = State.Wander;

                choice = decideRandomly(optionCount);
                
            }
            else if (choice == 1)
            {
                //dont move
                rb.velocity = Vector3.zero;
                AIState = State.Standing;

                //does AI turn around?
                if(!in3Dmode)
                {
                    if (choice == 0) //yes
                    {
                        //turn around
                        Flip();
                    }

                }
            }


            decisionTimer = Random.Range(decisionTimerMin, decisionTimerMax);
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
        
        Debug.DrawRay(transform.position, transform.forward * detectionLength, Color.red);

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

    /// <summary>
    /// Rotates the object
    /// </summary>
    void Flip()
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
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, debug_wanderDistancePoint);        
        Gizmos.DrawWireSphere(debug_wanderDistancePoint, wanderRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, newTarget);        
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

            case State.Standing:
                velocity.x = rb.velocity.x;
                velocity.y = 0;
                velocity.z = rb.velocity.z;
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
        //newTarget = new Vector3();
        newTarget.x = wanderPreviousTarget.x + Random.Range(-wanderJitter, wanderJitter);
        newTarget.y = wanderPreviousTarget.y;
        newTarget.z = wanderPreviousTarget.z + Random.Range(-wanderJitter, wanderJitter);

        //funky math 
        
        

        //move new target onto circle
        newTarget.Normalize();
        newTarget.x = newTarget.x * wanderRadius;
        newTarget.y = wanderPreviousTarget.y;
        newTarget.z = newTarget.z * wanderRadius;

        newTarget = newTarget + transform.forward * wanderDistance;

        Vector3 newTargetDirection = newTarget.normalized; 
        

        rb.velocity = transform.forward * 3;

        Vector3 dotProduct = new Vector3();
        dotProduct.x = transform.right.x * newTarget.x;
        angle = Vector3.Angle(transform.forward, newTarget);

        transform.Rotate(new Vector3(0, angle, 0) * Time.deltaTime, Space.Self);


        wanderPreviousTarget = newTarget;
    }
}


