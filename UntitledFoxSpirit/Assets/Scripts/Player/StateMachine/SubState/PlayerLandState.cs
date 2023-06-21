using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLandState : PlayerBaseState
{
    public PlayerLandState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

    public override void EnterState()
    {
        Debug.Log("Land State");

        if (ctx.isLandRolling) // is set in jumpstate RecordVelocity()
        {
            // Rolling
            ctx.tallCollider.material = null;
            ctx.isLanding = true;


            // Set player rotation to roll direction
            Quaternion rotation = ctx.GetPathRotation();

            // Flipping direction
            if (ctx.prevInputDirection.x < 0f)
            {
                rotation = ctx.Flip(rotation);
            }

            ctx.rb.rotation = rotation;
        }
        else
        {
            // Heavy Land
            ctx.tallCollider.material = ctx.friction;
        }


        ctx.disableInputRotations = true;
    }
    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void FixedUpdateState() { }
    public override void OnAnimatorMoveState() 
    {
        if (ctx.isLanding)
        {
            AnimatorStateInfo jumpRollState = ctx.animController.GetCurrentAnimatorStateInfo(0);

            // Roll when in state
            if (jumpRollState.IsName("JumpRoll") && jumpRollState.normalizedTime < 0.3f || ctx.isGrounded && ctx.animController.IsInTransition(0))
            {
                float y = ctx.rb.velocity.y;

                ctx.rb.velocity = ctx.animController.deltaPosition * vso.rootMotionJumpRollSpeed / Time.deltaTime;

                ctx.rb.velocity = new Vector3(ctx.rb.velocity.x, y, ctx.rb.velocity.z);
            }
        }
    }
    public override void ExitState() 
    {
        if (ctx.isLanding)
        {
            ctx.tallCollider.material = ctx.friction;
        }
        else
        {
            ctx.tallCollider.material = null;
        }

        ctx.isLanding = false;
        ctx.disableInputRotations = false;
    }
    public override void CheckSwitchState()
    {
        if (!ctx.isHeavyLand)
        {
            if (ctx.input.isMovementHeld)
            {
                SwitchState(factory.Walk());
            }
            else
            {
                SwitchState(factory.Idle());
            }
        }
    }
    public override void InitializeSubState() { }


   
}
