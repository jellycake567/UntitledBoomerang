using NUnit.Framework.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingState : StateMachine
{
    
    public override void Update()
    {
        Debug.Log("Moving");
    }
}
