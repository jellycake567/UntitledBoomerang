using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiftenAI : MonoBehaviour
{
    public enum State
    {
        Idle,
        Attack
    }

    public GameManager GM;
    public State AIState;

    private Vector3 playerPos;
    private Vector3 velocity;
    public float speed;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (AIState == State.Idle)
        {
            Idle();
        }
        else if (AIState == State.Attack)
        {
            Attack();          
        }
    }



    void Idle()
    {

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
