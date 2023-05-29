using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    enum State
    {
        Moving,
        Attack,
        Jump,
        Dash
    }


    StateMachine currentState;

    void Start()
    {
        currentState = new MovingState();
    }

    public virtual void Init() { }

    public virtual void Update() { Debug.Log("base"); }

    public virtual void Exit() { }
}
