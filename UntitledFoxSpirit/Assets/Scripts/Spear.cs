using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : MonoBehaviour
{

    [SerializeField] GameManager gM;
    [SerializeField] GameObject player;
    [SerializeField] Camera cam;

    Rigidbody rb;


    Transform parent;
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
        //throwingDir = Vector3.zero;
        //returnPosition = Vector3.zero;
        //spearLocalHoldPos = Vector3.zero;
        //dirToReturn = Vector3.zero;
        //rotationAxis = Vector3.zero;
        rotationAmount = 0;
        //spearLocalHoldPos = transform.localPosition;
        parent = transform.parent;
    }

    private void FixedUpdate()
    {
        if (isReturning)
        {
            //update returnPosition            
            returnPosition = gM.playerPos + spearLocalHoldPos;

            dirToReturn = returnPosition - transform.position;

            if (Vector3.Magnitude(dirToReturn) < 1f) //magic number
            {
                isReturning = false;
                isHeld = true;
                //RB.AddForce(Vector3.zero, ForceMode.VelocityChange);
                rb.velocity = Vector3.zero;
                this.transform.parent = GameObject.FindGameObjectWithTag("Player").transform;
            }
            else
            {
                //do vector shit for rotating the spear
                dirToReturn = Vector3.Normalize(returnPosition - transform.position);

                rotationAmount = Vector3.Magnitude(Vector3.Cross(dirToReturn, transform.forward));

                rotationAxis = Vector3.Cross(dirToReturn, transform.forward);

                //apply force
                //RB.angularVelocity = rotationAxis * rotationAmount * rotationSpeed;

                if (rb.velocity.magnitude < 10)//more magic numbers
                {
                    rb.AddForce(dirToReturn * 45, ForceMode.VelocityChange);
                }
            }
            

            
        }
    }

    void Update()
    {
        if (isHeld)
        {
            //if player wants to throw spear
            if (Input.GetKeyDown(KeyCode.Mouse1) && this.transform.parent != null)
            {
                GetComponent<MeshCollider>().enabled = true;

                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.FreezeRotation;

                this.transform.parent = null;

                throwingDir = transform.up;
                rb.AddForce(throwingDir * 40, ForceMode.Impulse); //another magic number
                isHeld = false;
            }
            else
            {
                Vector3 input = Input.mousePosition;
                Vector3 screenPoint = cam.WorldToScreenPoint(player.transform.position);

                // Get Angle
                float angle = Mathf.Atan2(input.x - screenPoint.x, input.y - screenPoint.y) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
                transform.rotation = Quaternion.AngleAxis(angle + 180f, Vector3.forward);

            }


        }
        else
        {

            //if player wants the spear to return
            if (Input.GetKeyDown(KeyCode.Mouse1) && this.transform.parent == null)
            {
                Destroy(GetComponent<Rigidbody>());

                GetComponent<MeshCollider>().enabled = false;

                transform.parent = parent;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;

                isHeld = true;

                //isReturning = true;
            }

            //check if spear has returned

        }
    }

    void ReturnToHand()
    {

    }
}


