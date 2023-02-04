using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHuman : MonoBehaviour
{
    public GameManager GM;
    private Vector3 playerPos;
    float detectionLength = 5;
    float decisionTimer = 0;
    
    int optionCount = 0;
    int choice = 0;
    Vector3 moveSpeed = new Vector3(0,0,5);
    Rigidbody rb;
    public float speed;
    public enum State
    {
        Standing,
        Wander,
        Attack
    }

    public State AIState;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void FixedUpdate()
    {
        if (AIState == State.Standing)
        {
            FindPlayer();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (AIState == State.Standing)
        {
            Standing();
        }
        else if (AIState == State.Attack)
        {
            Attack();
        }
        else if (AIState == State.Wander)
        {
            Wander();
        }
    }
    void Standing()
    {
        
        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0)
        {
            //Does the AI move?
            optionCount = 2;
            choice = decideRandomly(optionCount);
            if (choice == 0)
            {
                //move
                //optionCount = 2;
                choice = decideRandomly(optionCount);
                //does AI turn around?
                if (choice == 0) //yes
                {
                    //turn around
                    Flip();
                }

                rb.AddForce(moveSpeed, ForceMode.VelocityChange);
            }
            else if (choice == 1)
            {
                //dont move
                rb.AddForce(-moveSpeed, ForceMode.VelocityChange);
            }

            decisionTimer = Random.Range(2, 6);
        }
       
       

    }

    void Attack()
    {
        playerPos = GM.playerPos;
        Vector3 dirToPlayer = (playerPos - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (angle > 100f)
        {
            Flip();
        }

        //move towards player using force
        Move(dirToPlayer);


    }
    void Wander()
    {

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
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward,out hit, detectionLength))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                //Player detected
                AIState = State.Attack;
            }
       
        }
    }

    IEnumerator DecisionTimerCountdown()
    {


        yield return 1;
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
}


