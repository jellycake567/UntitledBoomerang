using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private Vector3 _movementVector;
    private bool _isInputHeld;
    private bool _isInputJumpHeld;
    private bool _isInputDashPressed;
    private bool _isInputAttackPressed;
    private bool _isInputCrouchPressed;

    public Vector3 GetMovementInput { get { return _movementVector; } }
    public bool isInputHeld { get { return _isInputHeld; } }
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
        _movementVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0, 0);

        _isInputHeld = Input.anyKey ? true : false;
        _isInputJumpHeld = Input.GetKey(KeyCode.Space) ? true : false;
        _isInputDashPressed = Input.GetKeyDown(KeyCode.LeftShift) ? true : false;
        _isInputAttackPressed = Input.GetKeyDown(KeyCode.Mouse0) ? true : false;
        _isInputCrouchPressed = Input.GetKeyDown(KeyCode.C) ? true : false;
    }
}
