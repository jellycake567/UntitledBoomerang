using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

    public override void EnterState()
    {
        JumpBuffer();
    }
    public override void UpdateState()
    {
        JumpBuffer();
        Jump();

        CheckSwitchState();
    }

    public override void FixedUpdateState()
    {
        ApplyGravity();
    }

    public override void ExitState() { }
    public override void CheckSwitchState()
    {
        if (ctx.isGrounded)
        {
            SwitchState(factory.Grounded());
        }
    }
    public override void InitializeSubState() { }


    void JumpBuffer()
    {
        if (ctx.input.isInputJumpHeld)
        {
            ctx.jumpBufferCounter = vso.jumpBufferTime;
        }
        else
        {
            ctx.jumpBufferCounter -= Time.deltaTime;
        }
    }

    void Jump()
    {
        if (ctx.animController.GetCurrentAnimatorStateInfo(0).IsName("DoubleJump") && ctx.animController.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f)
        {
            ctx.animController.SetBool("DoubleJump", false);
        }

        //Player jump input
        if (ctx.jumpBufferCounter > 0f && ctx.jumpCoyoteCounter > 0f && ctx.jumpCounter <= 0f)
        {
            ctx.reduceVelocityOnce = true;

            //Calculate Velocity
            float velocity = CalculateVelocity(vso.humanJumpHeight);

            //Jump
            ctx.rb.AddForce(new Vector3(0, velocity, 0), ForceMode.Impulse);

            ctx.animController.SetBool("Jump", true);

            //Set jump cooldown
            ctx.jumpCounter = vso.jumpCooldown;

            ctx.jumpCoyoteCounter = 0f; // So you don't triple jump

        } //2nd jump
        else if (ctx.input.isInputJumpHeld && ctx.canDoubleJump)
        {
            ctx.isHeavyLand = false;
            ctx.canDoubleJump = false;

            float velocity = CalculateVelocity(vso.humanJumpHeight * vso.doubleJumpHeightPercent);

            //Jump
            ctx.rb.AddForce(new Vector3(0, velocity, 0), ForceMode.Impulse);

            ctx.animController.SetBool("DoubleJump", true);

            //Set jump cooldown
            ctx.jumpCounter = vso.jumpCooldown;
        }
    }

    float CalculateVelocity(float jumpHeight)
    {
        float velocity = Mathf.Sqrt(-2 * vso.gravity * jumpHeight * vso.gravityScale);
        velocity += -ctx.rb.velocity.y; // Cancel out current velocity

        return velocity;
    }

    void ApplyGravity()
    {
        if (ctx.rb.velocity.y > 0f && !ctx.input.isInputJumpPressed && ctx.reduceVelocityOnce) // while jumping and not holding jump
        {
            ctx.reduceVelocityOnce = false;
            float percentageOfVelocity = ctx.rb.velocity.y * vso.reduceVelocity;
            ctx.rb.velocity = new Vector3(ctx.rb.velocity.x, ctx.rb.velocity.y - percentageOfVelocity, ctx.rb.velocity.z);
        }
        else
        {
            //Jumping while holding jump input
            ctx.rb.AddForce(new Vector3(0, vso.gravity, 0));

            if (ctx.rb.velocity.y < 0f)
                ctx.reduceVelocityOnce = false;
        }
    }
}
