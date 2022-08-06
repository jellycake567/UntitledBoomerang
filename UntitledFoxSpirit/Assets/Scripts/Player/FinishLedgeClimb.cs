using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLedgeClimb : MonoBehaviour
{
    public PlayerMovement script;

    void EndLedgeClimb()
    {
        script.climbAnim.Stop();
        script.canClimbLedge = false;
        script.ledgeDetected = false;

        script.transform.position = transform.position;
        transform.localPosition = new Vector3(0, 0, 0);
        //StartCoroutine(ResetPos());

    }

    IEnumerator ResetPos()
    {
        yield return new WaitForEndOfFrame();
        
        yield return new WaitForEndOfFrame();

    }
}
