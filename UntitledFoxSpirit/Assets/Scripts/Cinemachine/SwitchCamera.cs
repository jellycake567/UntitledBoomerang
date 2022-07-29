using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwitchCamera : MonoBehaviour
{
    public CinemachineVirtualCamera activateCam;
    public CinemachineVirtualCamera deactivateCam;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activateCam.Priority = 10;
            deactivateCam.Priority = 0;
        }
    }
}
