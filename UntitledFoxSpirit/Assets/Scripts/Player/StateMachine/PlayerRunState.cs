using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

    public override void EnterState() 
    {
        Debug.Log("Dash State");

        DashInput();
    }
    public override void UpdateState() 
    {
        CheckSwitchState();
    }

    public override void FixedUpdateState() { }
    public override void ExitState() 
    {
        ctx.StopCoroutine(Dash(true));
        ctx.disableInputRotations = false;
        ctx.animController.SetBool("Dash", false);
        ctx.animController.SetBool("isSprinting", true);
    }
    public override void CheckSwitchState() 
    {
        if (!ctx.input.isMovementHeld && !ctx.animIsDashing)
        {
            SwitchState(factory.Idle());
        }
        else if (ctx.input.isMovementHeld && !ctx.animIsDashing || ctx.animIsRunning)
        {
            SwitchState(factory.Walk());
        }
    }
    public override void InitializeSubState() { }

    void DashInput()
    {
        ctx.animController.SetBool("Dash", true);
        ctx.disableInputRotations = true;

        ctx.currentDashCooldown = vso.dashCooldown;

        if (ctx.prevInputDirection.x < 0.1f)
        {
            ctx.StartCoroutine(Dash(false));
        }
        else
        {
            ctx.StartCoroutine(Dash(true));
        }
    }

    IEnumerator Dash(bool usePathRotation)
    {
        ctx.currentSpeed = vso.humanRunSpeed;
        ctx.animController.SetFloat("ForwardSpeed", ctx.currentSpeed);

        // Stamina
        ctx.currentStamina -= vso.staminaConsumption;
        ctx.currentStaminaCooldown = vso.staminaCooldown;

        // Set Y velocity to 0
        ctx.rb.velocity = new Vector3(ctx.rb.velocity.x, 0f, ctx.rb.velocity.z);

        float dashDistance = vso.humanDashDistance;
        float dashTime = vso.humanDashTime;

        // Calculate speed
        float speed = dashDistance / dashTime;

        float currentDashTime = dashTime;

        while (currentDashTime > 0f)
        {
            currentDashTime -= Time.deltaTime;

            #region Rotation

            Quaternion targetRot = ctx.GetPathRotation();
            if (usePathRotation)
            {
                ctx.distanceOnPath += speed * Time.deltaTime;
            }
            else
            {
                Vector3 rot = targetRot.eulerAngles;
                targetRot = Quaternion.Euler(rot.x, rot.y + 180f, rot.z);

                ctx.distanceOnPath -= speed * Time.deltaTime;
            }

            #endregion

            #region Velocity

            // Where we want to player to face/walk towards
            Vector3 desiredDir = targetRot * Vector3.forward;

            Vector3 targetVelocity = desiredDir * speed;

            // Get rigidbody x and y velocity
            Vector3 rbVelocity = new Vector3(ctx.rb.velocity.x, 0f, ctx.rb.velocity.z);

            // Apply a force that attempts to reach our target velocity
            Vector3 velocity = rbVelocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -vso.maxVelocityChange, vso.maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -vso.maxVelocityChange, vso.maxVelocityChange);
            velocityChange.y = 0;

            ctx.rb.AddForce(velocityChange, ForceMode.VelocityChange);

            #endregion

            yield return new WaitForEndOfFrame();
        }


        ctx.animController.SetBool("isSprinting", true);
    }
}
