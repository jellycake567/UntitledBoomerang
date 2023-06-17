using System.Collections;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    public PlayerDashState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

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
    public override void OnAnimatorMoveState() { }

    public override void ExitState() 
    {
        ctx.disableInputRotations = false;
        ctx.isDashing = false;
        ctx.animController.SetBool("isRunning", true);
    }
    public override void CheckSwitchState() 
    {
        if (!ctx.input.isMovementHeld && !ctx.isDashing)
        {
            SwitchState(factory.Idle());
        }
        else if (ctx.input.isMovementHeld && !ctx.isDashing || ctx.animIsRunning)
        {
            SwitchState(factory.Walk());
        }
    }
    public override void InitializeSubState() { }

    void DashInput()
    {
        ctx.isDashing = true;
        ctx.animController.SetTrigger("DashTrigger");
        ctx.disableInputRotations = true;

        ctx.currentDashCooldown = vso.dashCooldown;

        Quaternion targetRot = ctx.GetPathRotation();

        if (ctx.prevInputDirection.x < 0.1f)
        {
            ctx.previousRotation = ctx.Flip(targetRot);
            ctx.StartCoroutine(Dash(false, ctx.previousRotation)); // Left
        }
        else
        {
            ctx.previousRotation = targetRot;
            ctx.StartCoroutine(Dash(true, ctx.previousRotation)); // Right
        }
    }

    IEnumerator Dash(bool usePathRotation, Quaternion targetRot)
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
            if (!ctx.isDashing)
                break;

            currentDashTime -= Time.deltaTime;

            #region Rotation

            
            if (usePathRotation)
            {
                ctx.distanceOnPath += speed * Time.deltaTime; // Right
            }
            else
            {
                ctx.distanceOnPath -= speed * Time.deltaTime; // Left
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

        ctx.animController.SetBool("isRunning", true);
    }
}
