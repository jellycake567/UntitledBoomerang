using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class SwitchPath : MonoBehaviour
{
    public PathCreator path;
    private PathCreator prevPath;
    private float prevDistance;
    bool didChangePath = false;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement script = other.GetComponentInParent<PlayerMovement>();

            if (!didChangePath)
            {
                prevPath = script.pathCreator;
                script.ChangePath(path);
                didChangePath = true;
            }
            else
            {
                script.ChangePath(prevPath);
                didChangePath = false;
            }
        }
    }
}
