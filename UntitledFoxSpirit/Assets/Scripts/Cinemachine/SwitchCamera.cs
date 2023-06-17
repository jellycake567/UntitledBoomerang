using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using NUnit.Framework.Constraints;

public class SwitchCamera : MonoBehaviour
{
    protected enum States
    {
        Path,
        DoorReveal
    }

    CameraStates script;
    [SerializeField] States stateToChange;

    void Start()
    {
        script = FindObjectOfType<CameraStates>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            script.controller.Play(stateToChange.ToString());
            Debug.Log(stateToChange.ToString());
        }
    }
}
