using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLedgeClimb : MonoBehaviour
{
    public PlayerMovement script;

    void EndLedgeClimb()
    {
        script.canClimbLedge = false;
        script.ledgeDetected = false;
    }
}
