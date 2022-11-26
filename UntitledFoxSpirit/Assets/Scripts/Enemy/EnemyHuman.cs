using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHuman : MonoBehaviour
{
    float detectionLength = 5;
    float decisionTimer = 0;
    
    int optionCount = 0;
    int choice = 0;

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
        decisionTimer = Random.Range(2, 6);
        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0)
        {
            //Does the AI move?
            optionCount = 2;
            choice = decideRandomly(optionCount);
            if (choice == 0)
            {
            //move
            }
            else if (choice == 1)
            {
            //dont move

            }
        }
       

    }

    void Attack()
    {

    }

    void Wander()
    {

    }

    int decideRandomly(int optionCount)
    {
        int decision = 0;
        decision = Random.Range(0, optionCount - 1);

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

    
}


