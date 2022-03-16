using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{

    [SerializeField] GameManager gM;
    [SerializeField] GameObject player;

    Rigidbody rb;

    Vector3 throwingDir;
    Vector3 returnPosition;
    Vector3 spearLocalHoldPos;
    Vector3 dirToReturn;
    Vector3 rotationAxis;

    bool isHeld = true;
    bool isReturning = false;
    float rotationSpeed = 100f;
    float rotationAmount;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        throwingDir = Vector3.zero;
        returnPosition = Vector3.zero;
        spearLocalHoldPos = Vector3.zero;
        dirToReturn = Vector3.zero;
        rotationAxis = Vector3.zero;
        throwingDir = Vector3.forward;
        rotationAmount = 0;
        spearLocalHoldPos = transform.localPosition;
    }

    private void FixedUpdate()
    {
        if (isReturning)
        {
            //update returnPosition            
            returnPosition = gM.playerPos + spearLocalHoldPos;

            dirToReturn = returnPosition - transform.position;

            if (Vector3.Magnitude(dirToReturn) < 0.5f) //magic number
            {
                isReturning = false;
                isHeld = true;
                //RB.AddForce(Vector3.zero, ForceMode.VelocityChange);
                rb.velocity = Vector3.zero;
                this.transform.parent = GameObject.FindGameObjectWithTag("Player").transform;
            }
            else
            {
                //do vector shit to rotate the spear
                dirToReturn = Vector3.Normalize(returnPosition - transform.position);

                rotationAmount = Vector3.Magnitude(Vector3.Cross(dirToReturn, transform.forward));

                rotationAxis = Vector3.Cross(dirToReturn, transform.forward);

                //apply force
                //RB.angularVelocity = rotationAxis * rotationAmount * rotationSpeed;

                if (rb.velocity.magnitude < 25)//more magic numbers
                {
                    rb.AddForce(dirToReturn * 3, ForceMode.Impulse);
                }
            }
            

            
        }
    }

    void Update()
    {
        if (isHeld)
        {
            //if player wants to throw spear
            if (Input.GetKeyUp(KeyCode.Space) && this.transform.parent != null)
            {
                //rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = true;
                this.transform.parent = null;
                rb.AddForce(throwingDir * 40, ForceMode.Impulse); //another magic number
                isHeld = !isHeld;
            }


        }
        else
        {

            //if player wants the spear to return
            if (Input.GetKeyUp(KeyCode.Space) && this.transform.parent == null)
            {
                rb.useGravity = false;
                isReturning = true;
            }

            //check if spear has returned

        }
    }

    void ReturnToHand()
    {

    }
}


