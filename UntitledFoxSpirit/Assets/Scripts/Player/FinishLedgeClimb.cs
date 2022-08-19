using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLedgeClimb : MonoBehaviour
{
    PlayerMovement script;

    private void Start()
    {
        script = GetComponentInParent<PlayerMovement>();
    }

    // Animation Event
    void EndLedgeClimb()
    {
        // Stop climbing animation
        script.climbAnim.Stop();

        // Reset ledge climb
        script.canClimbLedge = false;
        script.ledgeDetected = false;

        // Set players position to players body position and reset body position
        script.transform.position = transform.position;
        transform.localPosition = new Vector3(0, 0, 0);
    }
}
