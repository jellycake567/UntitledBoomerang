using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class projectile : MonoBehaviour
{

    GameObject player;

    Vector3 destination;

    Vector3 projectileHeading;
    Vector3 projectileHeadingDebug;
    Vector3 initialPos;

    Vector3 projectileHeadNorm;

    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        destination = player.transform.position;
        projectileHeading.x = destination.x - this.transform.position.x;
        projectileHeading.y = destination.y - this.transform.position.y;
        projectileHeading.z = destination.z - this.transform.position.z;

        projectileHeadingDebug.x = projectileHeading.x;
        projectileHeadingDebug.y = projectileHeading.y;
        projectileHeadingDebug.z = projectileHeading.z;

        projectileHeadNorm = projectileHeading.normalized;
        speed = 3;
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(projectileHeadNorm * Time.deltaTime * speed, Space.World);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(this.transform.position, this.transform.position + projectileHeading);
        //Gizmos.DrawLine(wanderPointDebug, targetDebug);
    }
}
