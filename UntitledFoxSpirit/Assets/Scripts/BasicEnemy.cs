using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    public enum behaviour
    {
        patrol,
        chase,
        
    }

    Rigidbody RB;

    float topSpeed;
    float acceleration;
    float patrolSpeed;

    [SerializeField] GameObject nodeList;
    //[SerializeField] List<Transform> nodePosList;
  
    void Start()
    {
        RB = GetComponent<Rigidbody>();

    }

    
    void Update()
    {
        //RB.AddForce()
            //rb.AddForce(agentHeading.normalized * moveSpeed, ForceMode.VelocityChange);
    }
}
