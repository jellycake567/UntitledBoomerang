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
    public override void ExitState() { }
    public override void InitializeSubState() { }
    public override void CheckSwitchState() 
    {
        if (context.IsJumpPressed && context.JumpCounter <= 0f)
        {
            SwitchState(factory.Jump());
        }    
    }

    //void ApplyGravity()
    //{
    //    if (context.Rb.velocity.y <= 0f)
    //    {
    //        // Player Falling
    //        context.Rb.AddForce(new Vector3(0, context.Gravity, 0) * context.Rb.mass * fallGravityMultiplier);

    //        if (!context.IsGrounded)
    //            context.AnimController.SetBool("Fall", true);
    //    }
    //}

}
