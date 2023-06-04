using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkState : PlayerBaseState
{
    private const float REDUCE_SPEED = 1.414214f;

    public PlayerWalkState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base (context, playerStateFactory, vso) { }

    public override void EnterState() 
    {
        ctx.animController.SetBool("isMoving", true);
    }
    public override void UpdateState()
    {
        CheckSwitchState();

        AccelAndDecel();
        StoreInputMovement();
    }
    public override void FixedUpdateState() 
    {
        Movement();
    }

    public override void ExitState() { }
    public override void CheckSwitchState() 
    {
        // Any time input is released, stop accel and start decel
        if (ctx.input.isInputReleased && !ctx.input.isMovementHeld)
        {
            ctx.animController.speed = vso.animJogDecelSpeed;
            ctx.StartCoroutine(Deceleration());
        }
        else if (ctx.input.isMovementHeld && ctx.isRunning)
        {
            SwitchState(factory.Run());
        }
    }

    public override void InitializeSubState() { }

    void StoreInputMovement()
    {
        // Store when player presses left or right
        if (ctx.prevInputDirection != ctx.input.GetMovementInput.normalized)
        {
            // Reset speed when turning around
            ctx.currentSpeed = 2f;
            ctx.prevInputDirection = ctx.input.GetMovementInput.normalized;
        }
    }

    void AccelAndDecel()
    {
        // Acceleration / Deceleration Calculation
        float maxSpeed = vso.humanSpeed;
        ctx.accelRatePerSec = maxSpeed / vso.accelTimeToMaxSpeed;
        ctx.decelRatePerSec = -maxSpeed / vso.decelTimeToZeroSpeed;

        // If input is not being held
        if (!ctx.input.isMovementHeld)
            return;

        // Accel to max speed
        if (ctx.currentSpeed < ctx.maxSpeed)
        {
            ctx.animController.speed = vso.animJogAccelSpeed; // Set to accel jogging speed
        }
        if (ctx.currentSpeed >= ctx.maxSpeed) // If reached max speed, set anim speed to normal jogging speed
        {
            ctx.animController.speed = vso.animJogSpeed;
        }
    }

    IEnumerator Deceleration()
    {
        float time = 0f;
        float timeToZero = vso.decelTimeToZeroSpeed * ctx.currentSpeed;

        // Waiting for deceleration to reach zero (Match decel anim with player movement)
        while (time < timeToZero)
        {
            time += -ctx.decelRatePerSec * Time.deltaTime;

            if (ctx.input.isMovementHeld)
                yield break;

            yield return new WaitForEndOfFrame();
        }

        SwitchState(factory.Idle());

    }

    void Movement()
    {
        Vector3 targetVelocity = ctx.input.GetMovementInput;
        Vector3 direction = targetVelocity.normalized;

        ctx.maxSpeed = vso.humanSpeed;

        #region Is player moving?

        // If player is moving
        if (ctx.input.isMovementHeld)
        {
            #region Reached end of path
            if (direction.x < 0f && ctx.distanceOnPath <= 0)
            {
                ctx.maxSpeed = 0f;
            }
            else if (direction.x > 0f && ctx.distanceOnPath >= ctx.pathCreator.path.length)
            {
                ctx.maxSpeed = 0f;
            }
            #endregion

            // Acceleration
            ctx.currentSpeed += ctx.accelRatePerSec * Time.deltaTime;
            ctx.currentSpeed = Mathf.Min(ctx.currentSpeed, ctx.maxSpeed);

            ctx.tallCollider.material = null;
            ctx.shortCollider.material = null;

            // ============================================== TODO BOOLS ==============================================
            if (ctx.animController.GetCurrentAnimatorStateInfo(0).IsName("Running") || ctx.isRunning || ctx.isDashing)
            {
                ctx.currentSpeed = vso.humanRunSpeed;
                ctx.isRunning = true;
            }

            if (ctx.animController.GetCurrentAnimatorStateInfo(0).IsTag("Land"))
            {
                ctx.isRunning = false;
            }
        }
        else
        {
            // Deceleration
            ctx.currentSpeed += ctx.decelRatePerSec * Time.deltaTime;
            ctx.currentSpeed = Mathf.Max(ctx.currentSpeed, 0);

            ctx.tallCollider.material = ctx.friction;
            ctx.shortCollider.material = ctx.friction;

            ctx.isRunning = false;
        }

        ctx.animController.SetFloat("ForwardSpeed", ctx.currentSpeed);

        #endregion

        #region Calculate Velocity

        // Where we want to player to face/walk towards
        Vector3 desiredDir = ctx.targetRot2D * Vector3.forward;

        // Rotation Debug Line for path
        //Debug.DrawLine(transform.position, transform.position + targetRot2D * Vector3.forward * 3f, Color.red);


        if (targetVelocity.z == 1 && targetVelocity.x == 1 || targetVelocity.z == 1 && targetVelocity.x == -1 || targetVelocity.z == -1 && targetVelocity.x == 1 || targetVelocity.z == -1 && targetVelocity.x == -1)
        {
            // Player is moving diagonally
            targetVelocity = desiredDir * targetVelocity.magnitude * ctx.currentSpeed / REDUCE_SPEED;
        }
        else if (targetVelocity.magnitude > 0f)
        {
            // Player currently apply input
            targetVelocity = desiredDir * targetVelocity.magnitude * ctx.currentSpeed;
        }
        else
        {
            // No input
            targetVelocity = ctx.previousRotation * Vector3.forward * ctx.currentSpeed;
        }

        Vector3 rbVelocity = new Vector3(ctx.rb.velocity.x, 0f, ctx.rb.velocity.z);

        // Apply a force that attempts to reach our target velocity
        Vector3 velocity = rbVelocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -vso.maxVelocityChange, vso.maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -vso.maxVelocityChange, vso.maxVelocityChange);
        velocityChange.y = 0;

        Debug.Log(velocityChange);

        ctx.rb.AddForce(velocityChange, ForceMode.VelocityChange);

        #endregion

        ctx.AdjustPlayerOnPath();
    }
}
