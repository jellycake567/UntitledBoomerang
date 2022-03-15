using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Path : MonoBehaviour
{
    List<Transform> Waypoints;


    public Vector3 size = new Vector3(0.5f, 0.5f, 0.5f);
    public bool enableDebug = false;

    // Update is called once per frame
    void Update()
    {
        Waypoints = new List<Transform>();

        // "transform" magically supply "all of its children" when it is in a situation such as a foreach
        foreach (Transform child in transform)
        {
            Waypoints.Add(child);
        }
    }

    private void OnDrawGizmos()
    {
        if (enableDebug)
        {
            for (int i = 0; i < Waypoints.Count; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(Waypoints[i].position, size);

                if (i < Waypoints.Count - 1)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(Waypoints[i].position, Waypoints[i + 1].position);
                }
            }
        }
    }
}