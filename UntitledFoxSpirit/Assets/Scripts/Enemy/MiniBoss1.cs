using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MiniBoss1 : MonoBehaviour
{
    float detectionLength = 4;
    float decisionTimer = 0;
    public float decisionTimerMin = 0;
    public float decisionTimerMax = 0;

    float distFromPlayer = 0;
    int optionCount = 0;
    int choice = 0;
    float moveSpeed = 3f;
    public float speed;

    //attack variables
    float currentAtkCooldown = 0;
    public float atkCooldown1;
    public float atkCooldown2;
    public float atkCooldown3;
    bool isAttacking = false;

    AnimatorClipInfo[] currentClipArray;
    public AnimationClip[] attackAnimations;

    public GameManager GM;
    Rigidbody rb;
    Animator animControl;
    NavMeshAgent navAgent;
    private Vector3 playerPos;

    public enum State
    {
        Standing,
        Wander,
        Combat
    }

    public enum atk
    {
        shortMelee,
        antiAirMelee,
        chargeAttack,
        behindMelee,

    }

    public atk atkState;
    public State AIState;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>();
        animControl = GetComponent<Animator>();
        atkState = atk.shortMelee;
    }


    private void FixedUpdate()
    {
        if (AIState == State.Standing)
        {
            FindPlayer();
        }
        else if (AIState == State.Wander)
        {
            if (rb.velocity == Vector3.zero)
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
            animControl.SetBool("isFighting", true);
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

        //navAgent.destination = playerPos;
        //Debug.Log("agent destination: " + navAgent.destination);

        if(!isAttacking)
        {
            StartCoroutine(attackCycle());

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

        //Debug.DrawRay(transform.position, transform.forward * detectionLength, Color.red);

        playerPos = GM.playerPos;
        distFromPlayer = Vector3.Distance(transform.position, playerPos);
        Vector3 dirToPlayer = (playerPos - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle < 45 && distFromPlayer < detectionLength)
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


    IEnumerator attackCycle()
    {
        isAttacking = true;
        animControl.SetBool("shortAtk", false);
        animControl.SetBool("antiAirAtk", false);
        animControl.SetBool("charge", false);
        switch (atkState)
        {
            case atk.shortMelee:
                if (distFromPlayer < 2.5)
                {
                    animControl.SetTrigger("attack");
                    atkState = atk.antiAirMelee;
                    //currentClipArray = animControl.GetCurrentAnimatorClipInfo(0);

                    yield return new WaitForSeconds(attackAnimations[0].length);
                    
                }
                else
                {
                    navAgent.destination = playerPos;
                    animControl.SetTrigger("isChasing");
                }

                break;

            case atk.antiAirMelee:
                if (distFromPlayer < 2.5)
                {
                    animControl.SetTrigger("attack2");
                    atkState = atk.chargeAttack;
                    yield return new WaitForSeconds(attackAnimations[1].length);
                    
                }
                else
                {
                    navAgent.destination = playerPos;
                    animControl.SetTrigger("isChasing");
                }

                break;

            case atk.chargeAttack:              
                animControl.SetTrigger("attack3");
                atkState = atk.shortMelee;
                yield return new WaitForSeconds(attackAnimations[2].length);
                
                break;

        }
        isAttacking = false;
    }





}
