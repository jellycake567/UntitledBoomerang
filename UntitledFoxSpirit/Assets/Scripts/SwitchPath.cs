using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class SwitchPath : MonoBehaviour
{
    public PathCreator path;
    bool didChangePath = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !didChangePath)
        {
            other.GetComponentInParent<PlayerMovement>().ChangePath(path);
            didChangePath = true;
        }
    }
}
