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
        Debug.Log("Walk State");
        ctx.animController.SetBool("isMoving", true);
    }
    public override void UpdateState()                                                                                                                                                                                                                                                         
    {
        CheckSwitchState();

        AccelAndDecel();
    }
    public override void FixedUpdateState() 
    {
        Movement();
    }

    public override void OnAnimatorMoveState() { }

    public override void ExitState()
    {
        ctx.animController.speed = 1f;
        ctx.animController.SetBool("isRunning", false);
        ctx.animController.SetBool("isMoving", false);
    }

    public override void CheckSwitchState()
    {
        // Any time input is released, stop accel and start decel
        if (ctx.input.isInputReleased && !ctx.input.isMovementHeld && !ctx.animIsRunning)
        {
            ctx.animController.speed = vso.animJogDecelSpeed;
            ctx.StartCoroutine(Deceleration());
        }
        else if (ctx.input.isInputDashPressed && !ctx.isDecel && ctx.currentDashCooldown <= 0f && ctx.isGrounded)
        {
            SwitchState(factory.Dash());
        }
        else if (ctx.input.isInputDashPressed && ctx.isDecel && ctx.currentDashCooldown <= 0f && ctx.isGrounded)
        {
            SwitchState(factory.BackStep());
        }
        else if (!ctx.input.isMovementHeld && ctx.animIsRunning && ctx.currentSpeed < 4f) // is running and switching to idle
        {
            SwitchState(factory.Idle());
            ctx.rb.velocity = new Vector3(-vso.reduceSpeed * ctx.prevInputDirection.x, 0f, 0f);
        }
        else if (ctx.input.isInputAttackPressed && ctx.isGrounded && ctx.currentAttackCooldown <= 0f)
        {
            SwitchState(factory.Attack());
        }
    }

    public override void InitializeSubState() { }

    

    void AccelAndDecel()
    {
        // Acceleration / Deceleration Calculation
        float maxSpeed = vso.humanSpeed;
        ctx.accelRatePerSec = maxSpeed / vso.accelTimeToMaxSpeed;
        ctx.decelRatePerSec = -maxSpeed / vso.decelTimeToZeroSpeed;

        // If input is not being held or is running
        if (!ctx.input.isMovementHeld || ctx.animIsRunning)
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
        Debug.Log("Decel Start");
        ctx.isDecel = true;

        float time = 0f;
        float timeToZero = vso.decelTimeToZeroSpeed * ctx.currentSpeed;

        // Waiting for deceleration to reach zero (Match decel anim with player movement)
        while (time < timeToZero)
        {
            time += -ctx.decelRatePerSec * Time.deltaTime;

            if (ctx.input.isMovementHeld || ctx.isBackStep)
            {
                Debug.Log("Break");
                ctx.isDecel = false;
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }

        if (ctx.isBackStep)
        {
            Debug.Log("Break!!");


            ctx.isDecel = false;
            yield break;
        }

        ctx.isDecel = false;

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
            if (ctx.animIsRunning)
            {
                ctx.currentSpeed = vso.humanRunSpeed;
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

            //ctx.isRunning = false;
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

        ctx.rb.AddForce(velocityChange, ForceMode.VelocityChange);

        #endregion

        AdjustPlayerOnPath();
    }


    void AdjustPlayerOnPath()
    {
        Vector3 pathPos = ctx.pathCreator.path.GetPointAtDistance(ctx.distanceOnPath, EndOfPathInstruction.Stop);
        //Debug.DrawLine(pathPos + new Vector3(0f, 1f, 0f), pathPos + Vector3.up * 3f, Color.green);

        // Distance between path and player
        float distance = Vector3.Distance(pathPos.IgnoreYAxis(), ctx.transform.position.IgnoreYAxis());

        // Direction from path towards player
        Vector3 dirTowardPlayer = ctx.transform.position.IgnoreYAxis() - pathPos.IgnoreYAxis();
        Debug.DrawLine(pathPos, pathPos + dirTowardPlayer * vso.maxDistancePath, Color.blue);

        // Keeps player on the path
        if (distance > vso.maxDistancePath)
        {
            Vector3 dirTowardPath = (pathPos.IgnoreYAxis() - ctx.transform.position.IgnoreYAxis()).normalized;
            ctx.rb.AddForce(dirTowardPath * vso.adjustVelocity, ForceMode.Impulse);
        }
    }
}
