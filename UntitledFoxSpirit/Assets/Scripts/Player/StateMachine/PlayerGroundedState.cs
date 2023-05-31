using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory) { }

    public override void EnterState() { }
    public override void UpdateState() 
    {
        //ApplyGravity();
        CheckSwitchState();
    }

    public override void FixedUpdateState() 
    {
        ApplyGravity();
    }
    public override void ExitState() { }
    public override void InitializeSubState() { }
    public override void CheckSwitchState() 
    {
        if (input.isInputJumpPressed && jumpCounter <= 0f)
        {
            SwitchState(factory.Jump());
        }    
    }

    void ApplyGravity()
    {
        if (rb.velocity.y <= 0f)
        {
            // Player Falling
            rb.AddForce(new Vector3(0, gravity, 0) * rb.mass * fallGravityMultiplier);

            if (!isGrounded)
                animController.SetBool("Fall", true);
        }
    }

}
