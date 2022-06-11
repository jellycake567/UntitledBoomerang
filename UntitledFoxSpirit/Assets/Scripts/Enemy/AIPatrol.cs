using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIPatrol : MonoBehaviour
{
    public float speed = 2.0f;
    public Transform groundCheck;
    public LayerMask groundLayer;

    bool mustPatrol = true;
    bool mustTurn;
    
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mustPatrol)
        {
            Patrol();
        }
    }

    void FixedUpdate()
    {
        if (mustPatrol)
        {
            Collider[] overlap = Physics.OverlapSphere(groundCheck.position, 0.1f, groundLayer);

            if (overlap.Length == 0)
                mustTurn = true;
            else
                mustTurn = false;
        }
    }

    void Patrol()
    {
        if (mustTurn)
        {
            Flip();
        }

        // Move AI
        Vector3 velocity = new Vector3(speed * Time.fixedDeltaTime, rb.velocity.y, rb.velocity.z);
        rb.velocity = transform.forward * velocity.magnitude;
    }

    void Flip()
    {
        mustPatrol = false;

        Vector3 rotation = transform.eulerAngles + Quaternion.Euler(new Vector3(0, 180f, 0)).eulerAngles;
        transform.rotation = Quaternion.Euler(rotation);
        speed *= -1;

        mustPatrol = true;
    }
}
