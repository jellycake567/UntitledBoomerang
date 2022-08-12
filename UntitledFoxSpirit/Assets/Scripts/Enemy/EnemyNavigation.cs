using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{
    NavMeshAgent agent;

    public float rotationAngle = 30.0f;
    public Transform target;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
    }

    // Update is called once per frame
    void Update()
    {
        Chase();
    }

    void Chase()
    {
        // Create a path and set it based on a target position.
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);

        if (path.corners.Length > 1)
        {
            Debug.DrawLine(path.corners[0], path.corners[1], Color.red);

            Vector3 dir = path.corners[1] - path.corners[0];
            dir.y = 0f;

            // Calculate angle from AI to player
            float angle = Mathf.Acos(Vector3.Dot(transform.forward, dir.normalized)) * Mathf.Rad2Deg;

            // Rotation speed
            float rotSpeed = angle / rotationAngle;


            // Get Rotation
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotSpeed);

        }

        agent.SetDestination(target.position);
    }

}
