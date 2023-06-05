using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) 
    {
        isRootState = true;
        InitializeSubState();
    }

    public override void EnterState()
    {
        Debug.Log("Jump State");
        JumpBuffer();
        FirstJump();
    }
    public override void UpdateState()
    {
        CheckSwitchState();

        JumpBuffer();
        FirstJump();
        DoubleJump();
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
    public override void InitializeSubState() 
    {
        if (ctx.input.isMovementHeld)
        {
            SetSubState(factory.Walk());
        }
        else
        {
            SetSubState(factory.Idle());
        }
    }


    void JumpBuffer()
    {
        if (ctx.input.isInputJumpPressed)
        {
            ctx.jumpBufferCounter = vso.jumpBufferTime;
        }
        else
        {
            ctx.jumpBufferCounter -= Time.deltaTime;
        }
    }

    void FirstJump()
    {
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
        }
    }

    void DoubleJump()
    {
        if (ctx.animController.GetCurrentAnimatorStateInfo(0).IsName("DoubleJump") && ctx.animController.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f)
        {
            ctx.animController.SetBool("DoubleJump", false);
        }

         //Double jump
        if (ctx.input.isInputJumpPressed && ctx.canDoubleJump)
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
        if (ctx.rb.velocity.y <= 0f)
        {
            // Player Falling
            ctx.rb.AddForce(new Vector3(0, vso.gravity, 0) * ctx.rb.mass * vso.fallGravityMultiplier);

            if (!ctx.isGrounded)
                ctx.animController.SetBool("Fall", true);
        }
        else if (ctx.rb.velocity.y > 0f && !ctx.input.isInputJumpHeld && ctx.reduceVelocityOnce) // while jumping and not holding jump
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
