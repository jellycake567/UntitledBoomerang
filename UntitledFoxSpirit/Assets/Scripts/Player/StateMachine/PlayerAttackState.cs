using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    public PlayerAttackState(PlayerStateMachine context, PlayerStateFactory playerStateFactory, VariableScriptObject vso) : base(context, playerStateFactory, vso) { }

    public override void EnterState()
    {
        Debug.Log("Attack State");
    }
    public override void UpdateState()
    {
        CheckSwitchState();
    }

    public override void FixedUpdateState() { }
    public override void ExitState() { }
    public override void CheckSwitchState()
    {
        if (ctx.input.isInputDashPressed && ctx.currentDashCooldown <= 0f)
        {
            SwitchState(factory.Dash());
        }
        else if (ctx.input.isMovementHeld)
        {
            SwitchState(factory.Walk());
        }
        else if (!ctx.input.isMovementHeld)
        {
            SwitchState(factory.Idle());
        }
    }
    public override void InitializeSubState() { }



    //void Attack()
    //{
    //    // Is currently playing attack animation
    //    if (animController.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
    //    {
    //        // Attack animation has started!
    //        if (!isAttacking)
    //        {
    //            rb.velocity = Vector3.zero;
    //            disableMovement = true;
    //            disableInputRotations = true;
    //            isAttacking = true;
    //            currentSpeed = 0f;
    //        }

    //        // Move after attacking
    //        if (animController.IsInTransition(0) && animController.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f)
    //        {
    //            disableMovement = false;
    //        }

    //    }
    //    else
    //    { // Not playing attacking animation
    //        if (isAttacking)
    //        {
    //            disableMovement = false;
    //            disableInputRotations = false;
    //            isAttacking = false;
    //            comboCounter = 0;

    //            animController.SetBool("Attack1", false);
    //            animController.SetBool("Attack2", false);
    //            animController.SetBool("Attack3", false);
    //        }
    //    }

    //    // End animation combo
    //    if (animController.GetCurrentAnimatorStateInfo(0).normalizedTime > animController.GetFloat("resetComboTime") && animController.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
    //    {
    //        if (!animController.IsInTransition(0) && resetAttack) // Check if not in transiton, so it doesn't reset run during transition
    //        {
    //            animController.SetBool("Attack1", false);
    //            disableMovement = false;
    //            disableInputRotations = false;
    //            resetAttack = false;
    //            comboCounter = 0;

    //            int atkNum = Random.Range(1, 5);
    //            if (lastAttackInt == atkNum)
    //            {
    //                atkNum++;

    //                if (atkNum > 4)
    //                    atkNum = 1;
    //            }
    //            lastAttackInt = atkNum;

    //            animController.SetInteger("RngAttack", atkNum);
    //        }
    //    }
    //    else
    //    {
    //        resetAttack = true;
    //    }

    //    if (animController.GetCurrentAnimatorStateInfo(0).normalizedTime > animController.GetFloat("resetComboTime") && animController.GetCurrentAnimatorStateInfo(0).IsName("Attack2") && !animController.IsInTransition(0))
    //    {
    //        animController.SetBool("Attack2", false);
    //        disableMovement = false;
    //        comboCounter = 0;
    //    }
    //    if (animController.GetCurrentAnimatorStateInfo(0).normalizedTime > animController.GetFloat("resetComboTime") && animController.GetCurrentAnimatorStateInfo(0).IsName("Attack3") && !animController.IsInTransition(0))
    //    {
    //        animController.SetBool("Attack3", false);
    //        disableMovement = false;
    //        comboCounter = 0;
    //    }

    //    // Cooldown to click again
    //    if (currentAttackCooldown <= 0f)
    //    {
    //        if (Input.GetKeyDown(KeyCode.Mouse0) && isGrounded && !isSneaking && !animController.IsInTransition(0))
    //        {
    //            OnClick();
    //        }
    //    }

    //    if (currentAttackCooldown > 0f)
    //        currentAttackCooldown -= Time.deltaTime;
    //}

    //void OnClick()
    //{
    //    if (comboCounter == 0)
    //    {
    //        disableInputRotations = true;
    //        isAttacking = true;
    //    }

    //    animController.speed = 1f;

    //    // Set time
    //    currentAttackCooldown = attackCooldown;

    //    // Increase combo count
    //    comboCounter++;
    //    // Clamp combo
    //    comboCounter = Mathf.Clamp(comboCounter, 0, 2);


    //    if (comboCounter == 1 && !animController.GetBool("Attack1"))
    //    {
    //        animController.SetTrigger("Attack");
    //        animController.SetBool("Attack1", true);
    //    }

    //    // Transitions to next combo animation
    //    if (comboCounter >= 2 && animController.GetCurrentAnimatorStateInfo(0).normalizedTime > animController.GetFloat("attackInputTime") && animController.GetCurrentAnimatorStateInfo(0).IsName("Attack1"))
    //    {
    //        animController.SetBool("Attack1", false);
    //        animController.SetBool("Attack2", true);
    //    }
    //    if (comboCounter >= 3 && animController.GetCurrentAnimatorStateInfo(0).normalizedTime > animController.GetFloat("attackInputTime") && animController.GetCurrentAnimatorStateInfo(0).IsName("Attack2"))
    //    {
    //        animController.SetBool("Attack2", false);
    //        animController.SetBool("Attack3", true);
    //    }
    //}
}
