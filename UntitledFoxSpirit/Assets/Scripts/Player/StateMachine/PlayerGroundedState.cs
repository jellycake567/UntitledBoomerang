using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

    public override void EnterState() { }
    public override void UpdateState()
    {
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
        if (ctx.input.isInputJumpPressed || !ctx.isGrounded)
        {
            SwitchState(factory.Jump());
        }
    }

    void ApplyGravity()
    {
        if (ctx.rb.velocity.y <= 0f)
        {
            // Player Falling
            ctx.rb.AddForce(new Vector3(0, vso.gravity, 0) * ctx.rb.mass * vso.fallGravityMultiplier);
        
            if (!ctx.isGrounded)
                ctx.animController.SetBool("Fall", true);
        }
    }
}
