using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeekBehaviourScript : MonoBehaviour
{
    public GameObject seekTarget;
    Vector3 V;
    Vector3 currentVelocity;
    Vector3 force;
    Vector3 totalForce;
    public float wanderRadius = 3;
    public float wanderDistance = 5;
    public float wanderJitter = 3;
    Vector3 wanderPreviousTarget;
    Vector3 newTarget;


    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        V = Vector3.zero;
        currentVelocity = Vector3.zero;
        force = Vector3.zero;
        rb = GetComponent<Rigidbody>();

        wanderPreviousTarget.x = transform.position.x + Random.Range(-wanderRadius, wanderRadius);
        wanderPreviousTarget.y = transform.position.y;
        wanderPreviousTarget.z = transform.position.z + Random.Range(-wanderRadius, wanderRadius);
        wanderPreviousTarget = wanderPreviousTarget + transform.forward * wanderDistance;
        newTarget = new Vector3();
        totalForce = new Vector3();
    }

    // Update is called once per frame
    void Update()
    {


        Vector3 eulerAngle = Quaternion.Euler(rb.velocity.normalized).eulerAngles;
        //eulerAngle.x = 0;
        //eulerAngle.z = 0;
        //transform.Rotate(eulerAngle, Space.Self);

        Quaternion rotation = Quaternion.LookRotation(seekTarget.transform.position - transform.position);
        //Quaternion current = transform.localRotation;


        //transform.rotation = Quaternion.Slerp(Quaternion.identity, rotation, Time.deltaTime);
        transform.rotation = rotation;
        

        
        //newTarget.x = wanderPreviousTarget.x + Random.Range(-wanderJitter, wanderJitter);
        //newTarget.y = wanderPreviousTarget.y;
        //newTarget.z = wanderPreviousTarget.z + Random.Range(-wanderJitter, wanderJitter);
        //
        //Vector3 dirFromObjtoNewTarget = new Vector3();
        //dirFromObjtoNewTarget = newTarget - transform.position;
        //
        //dirFromObjtoNewTarget.Normalize();

        //move new target onto circle

        //newTarget = transform.position + dirFromObjtoNewTarget * wanderRadius + transform.forward * wanderDistance;


        V = seekTarget.transform.position - transform.position;
        V = V.normalized;
        V *= 10;
        currentVelocity = rb.velocity;
        force = V - currentVelocity;
        force.Normalize();
        wanderPreviousTarget = newTarget;

        rb.velocity += force * 1;
    }
}
