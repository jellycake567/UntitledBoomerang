using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory) { }

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
        //ApplyGravity();
    }

    public override void ExitState() { }
    public override void CheckSwitchState()
    {
        //if (isGrounded)
        //{
        //    SwitchState(factory.Grounded());
        //}
    }
    public override void InitializeSubState() { }


    void JumpBuffer()
    {
        //if (input.isInputJumpHeld)
        //{
        //    jumpBufferCounter = jumpBufferTime;
        //}
        //else
        //{
        //    jumpBufferCounter -= Time.deltaTime;
        //}
    }

    void Jump()
    {
        //if (animController.GetCurrentAnimatorStateInfo(0).IsName("DoubleJump") && animController.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f)
        //{
        //    animController.SetBool("DoubleJump", false);
        //}

        //Player jump input
        //if (jumpBufferCounter > 0f && jumpCoyoteCounter > 0f && jumpCounter <= 0f)
        //{
        //    reduceVelocityOnce = true;

        //    Calculate Velocity
        //    float velocity = CalculateVelocity(humanJumpHeight);

        //    Jump
        //    rb.AddForce(new Vector3(0, velocity, 0), ForceMode.Impulse);

        //    animController.SetBool("Jump", true);

        //    Set jump cooldown
        //   jumpCounter = jumpCooldown;

        //    jumpCoyoteCounter = 0f; // So you don't triple jump

        //} //2nd jump
        //else if (input.isInputJumpHeld && canDoubleJump)
        //{
        //    isHeavyLand = false;
        //    canDoubleJump = false;

        //    float velocity = CalculateVelocity(humanJumpHeight * doubleJumpHeightPercent);

        //    Jump
        //    rb.AddForce(new Vector3(0, velocity, 0), ForceMode.Impulse);

        //    animController.SetBool("DoubleJump", true);

        //    Set jump cooldown
        //   jumpCounter = jumpCooldown;
        //}
    }

    //float CalculateVelocity(float jumpHeight)
    //{
    //    float velocity = Mathf.Sqrt(-2 * gravity * jumpHeight * gravityScale);
    //    velocity += -rb.velocity.y; // Cancel out current velocity

    //    return velocity;
    //}

    //void ApplyGravity()
    //{
    //    if (rb.velocity.y > 0f && !input.isInputJumpPressed && reduceVelocityOnce) // while jumping and not holding jump
    //    {
    //        reduceVelocityOnce = false;
    //        float percentageOfVelocity = rb.velocity.y * reduceVelocity;
    //        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - percentageOfVelocity, rb.velocity.z);
    //    }
    //    else
    //    {
    //        Jumping while holding jump input
    //        rb.AddForce(new Vector3(0, gravity, 0));

    //        if (rb.velocity.y < 0f)
    //            reduceVelocityOnce = false;
    //    }
    //}
}
