
using System;

public class PlayerStateFactory
{
    PlayerStateMachine context;
    VariableScriptObject vso;

    public PlayerStateFactory(PlayerStateMachine currentContext, VariableScriptObject variableScriptableObject)
    {
        context = currentContext;
        vso = variableScriptableObject;
    }

    public PlayerBaseState Idle()
    {
        return new PlayerIdleState(context, this, vso);
    }
    public PlayerBaseState Walk()
    {
        return new PlayerWalkState(context, this, vso);
    }
    public PlayerBaseState Run()
    {
        return new PlayerRunState(context, this, vso);
    }
    public PlayerBaseState Jump()
    {
        return new PlayerJumpState(context, this, vso);
    }
    public PlayerBaseState Grounded()
    {
        return new PlayerGroundedState(context, this, vso);
    }
}
