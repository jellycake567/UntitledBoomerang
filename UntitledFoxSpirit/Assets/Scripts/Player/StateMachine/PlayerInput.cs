using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlayerInput
{
    private bool _isInputHeld;
    private bool _isInputMovementHeld;
    private bool _isInputJumpHeld;
    private bool _isInputDashPressed;
    private bool _isInputAttackPressed;
    private bool _isInputCrouchPressed;

    public bool isInputHeld { get { return _isInputHeld; } }
    public bool isInputMovementHeld { get { return _isInputMovementHeld; } }
    public bool isInputJumpHeld { get { return _isInputJumpHeld; } }
    public bool isInputDashPressed { get { return _isInputDashPressed; } }
    public bool isInputAttackPressed { get { return _isInputAttackPressed; } }
    public bool isInputCrouchPressed { get { return _isInputCrouchPressed; } }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _isInputHeld = Input.anyKey ? true : false;
        
        _isInputMovementHeld = Input.GetAxisRaw("Horizontal") > 0f || Input.GetAxisRaw("Horizontal") < 0f ? true : false;
        _isInputJumpHeld = Input.GetKey(KeyCode.Space) ? true : false;
        
        _isInputDashPressed = Input.GetKeyDown(KeyCode.LeftShift) ? true : false;
        _isInputAttackPressed = Input.GetKeyDown(KeyCode.Mouse0) ? true : false;
        _isInputCrouchPressed = Input.GetKeyDown(KeyCode.C) ? true : false;
    }
}
