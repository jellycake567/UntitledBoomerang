using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{

    Rigidbody RB;
    Vector3 throwingDir;
    Vector3 returnPosition;
    bool isHeld = true;
    bool isReturning = false;

    void Start()
    {
        throwingDir = Vector3.forward;
        

    }

    private void FixedUpdate()
    {
        if (isReturning)
        {
            //update returnPosition

            //do vector shit to rotate 

            //apply force
        }
    }

    void Update()
    {
        if (isHeld)
        {
            //if player wants to throw spear
            if (Input.GetKeyDown(KeyCode.Space) && this.transform.parent != null)
            {
                RB = gameObject.AddComponent<Rigidbody>();
                RB.useGravity = false;
                this.transform.parent = null;
                RB.AddForce(throwingDir * 30, ForceMode.Impulse);
                isHeld = !isHeld;
            }


        }
        else
        {

            //if player wants the spear to return
            if (Input.GetKeyDown(KeyCode.Space) && this.transform.parent == null)
            {
                isReturning = true;
            }

            //check if spear has returned

        }
    }

    void ReturnToHand()
    {

    }
}


