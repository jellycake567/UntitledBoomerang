using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStates : MonoBehaviour
{
    public Animator controller;


    // Start is called before the first frame update
    protected virtual void Start()
    {
        controller = GetComponent<Animator>();
    }
}
