using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVariables : MonoBehaviour
{
    StateMachine currentState;


    void Start()
    {
        currentState = new MovingState();
    }

    void Update()
    {
        currentState.Update();
    }
}
