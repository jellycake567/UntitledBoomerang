using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    int wanderCount;
    int chargeCount;
    int shootCount;
    int random;
    int maxChargeCount;
    int maxShootCount;
    int maxSpinCount;
    int ammo;

    float timer;
    public float targetTimer;
    public float chargeTimer;

    public float moveSpeed;
    float angle;
    public float wanderPointRadius;
    public float wanderPointDistance;
    float wanderJitter;
    public float chargeSpeed;


    Vector2 wanderPoint;
    Vector2 target;
    Vector2 agentHeading;
    Vector2 shootHeading;

    Rigidbody rb;

    [SerializeField] GameManager GM;
    [SerializeField] GameObject ProjectilePrefab;


    //Debug
    Vector3 wanderPointDebug;
    Vector3 targetDebug;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        wanderPointDistance = 0;
        wanderPointRadius = 3;
        timer = 0;
        chargeTimer = 0;
        wanderCount = 0;
        chargeCount = 0;

        //maxChargeCount = Random.Range(1, 4);
        //maxShootCount = Random.Range(1, 4);
        //maxSpinCount = Random.Range(1, 3);
        maxChargeCount = 1;
        maxShootCount = 3;
    
    }


    void Update()
    {
        //charge for 3 seconds
        if(chargeTimer > 3)
        {
            chargeTimer = 0;
            wanderCount = 0;
            chargeCount += 1;
        }

        
        
        if (wanderCount == 5 && chargeCount >= maxChargeCount && shootCount < maxShootCount)
        {
            //shoot
            Instantiate(ProjectilePrefab, this.transform.position, this.transform.rotation);
            //shootHeading.x = GM.playerPos.x - this.transform.position.x;
            //shootHeading.y = GM.playerPos.y - this.transform.position.y;

            
            shootCount += 1;
        }
        else  if (wanderCount == 5 && chargeTimer < 3 && chargeCount < maxChargeCount)        
        {          
            ChargeAtPlayer();
     
            chargeTimer += Time.deltaTime;
            //if(chargeTimer >= 3)
            //{
            //    chargeCount += 1;
            //}

        }
        else if (timer < 1)                                         //
        {                                                           //
            timer += Time.deltaTime;                                //
        }                                                           //    
        else if (timer >= targetTimer)                              //  Chooses a point to move to every second
        {                                                           //        
            Wander();                                               //    
            timer = 0;                                              //
            wanderCount += 1;                                       //
        }                                                           //

        



    }

    void Wander()
    {
        wanderPoint.x = this.transform.position.x;
        wanderPoint.y = this.transform.localPosition.y + wanderPointDistance;

        angle = Random.Range(0, 360);

        target.x = wanderPoint.x + Mathf.Cos(angle) * wanderPointRadius;
        target.y = wanderPoint.y + Mathf.Sin(angle) * wanderPointRadius;

        agentHeading.x = target.x - this.transform.position.x;
        agentHeading.y = target.y - this.transform.position.y;

        rb.velocity = Vector3.zero;
        rb.AddForce(agentHeading.normalized * moveSpeed, ForceMode.VelocityChange);

        //For debugging
        wanderPointDebug.x = wanderPoint.x;
        wanderPointDebug.y = wanderPoint.y;
        wanderPointDebug.z = 0.0f;

        targetDebug.x = target.x;
        targetDebug.y = target.y;
        targetDebug.z = 0;

    }

    void ChargeAtPlayer()
    {
        agentHeading.x = GM.playerPos.x - this.transform.position.x;
        agentHeading.y = GM.playerPos.y - this.transform.position.y;

        rb.AddForce(agentHeading.normalized * chargeSpeed, ForceMode.VelocityChange);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(this.transform.position, wanderPointDebug);
        Gizmos.DrawLine(wanderPointDebug, targetDebug);
    }
}
