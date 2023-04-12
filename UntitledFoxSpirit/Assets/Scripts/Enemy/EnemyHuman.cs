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

    float distFromPlayer = 0;
    int optionCount = 0;
    int choice = 0;
    float moveSpeed = 3f;
    Rigidbody rb;
    public float speed;
    Animator animControl;

    //public GameObject weapon;
    //public GameObject bodyAnimObj;
    

    NavMeshAgent navAgent;

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

    }


    private void FixedUpdate()
    {
        if (AIState == State.Standing)
        {
            FindPlayer();
        }
        else if (AIState == State.Wander)
        {             
            if(rb.velocity == Vector3.zero)
            {
                rb.AddForce(transform.forward * moveSpeed, ForceMode.VelocityChange);
            }
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
            Wander();
        }
        
    }

    void Standing()
    {
        
     
        if (decisionTimer <= 0)
        {
            //Does the AI move?
            optionCount = 2;
            choice = decideRandomly(optionCount);
            if (choice == 0)
            {
                AIState = State.Wander;
            }
            else if (choice == 1)
            {
                //dont move
                rb.velocity = Vector3.zero;
                AIState = State.Standing;

                choice = decideRandomly(optionCount);
                //does AI turn around?
                if (choice == 0) //yes
                {
                    //turn around
                    Flip();
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

        navAgent.destination = playerPos;
        //bodyAnimControl.SetTrigger("chaseTrig");
        //Debug.Log("agent destination: " + navAgent.destination);
        //move towards player using force
        //Move(dirToPlayer);

        Debug.Log("Distance from Player" + distFromPlayer);
        //attack the player when within range
        if(distFromPlayer < 2.5)
        {
            //animControl.SetBool("isFighting", true);
        }
        else
        {
            animControl.SetBool("isFighting", false);
        }

        if (rb.velocity == Vector3.zero)
        {
            animControl.SetTrigger("Attack");
            
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
                if (choice == 0) //yes
                {
                    //turn around
                    Flip();
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
        //RaycastHit hit;
        //if (Physics.SphereCast(transform.position, 6, transform.forward,out hit, detectionLength))
        //{
        //    if (hit.collider.gameObject.CompareTag("Player"))
        //    {
        //        //Player detected
        //        Debug.Log("Detected");
        //        
        //        AIState = State.Combat;
        //    }
        //    
        //}
        
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
        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position, transform.position + transform.forward * detectionLength);
        //Gizmos.DrawSphere(transform.position, 6);
    }



 
}


