using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine context, PlayerStateFactory playerStateFactory) : base(context, playerStateFactory) { }

    public override void EnterState() { }
    public override void UpdateState() 
    {
        CheckSwitchState();
    }
    public override void ExitState() { }
    public override void InitializeSubState() { }
    public override void CheckSwitchState() 
    {
        if (context.isJumpPressed)
        {
            SwitchState(factory.Jump());
        }    
    }

}
