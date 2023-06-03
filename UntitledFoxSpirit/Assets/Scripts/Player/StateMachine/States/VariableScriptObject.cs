using Cinemachine;
using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class VariableScriptObject : ScriptableObject
{
    #region Movement / Rotation

    [Header("Movement")]
    public float humanSpeed = 5.0f;
    public float humanRunSpeed = 10.0f;
    public float accelTimeToMaxSpeed = 2.0f;
    public float decelTimeToZeroSpeed = 1.0f;
    public float animJogSpeed = 1.17f;
    public float animJogAccelSpeed = 0.8f;
    public float animJogDecelSpeed = 0.8f;
    
    public float turnSmoothTime2D = 0.03f;
    public float turnSmoothTime3D = 0.1f;
    public float maxVelocityChange = 10f;
    public float frictionAmount = 0.2f;
    

    [Header("Rotation")]
    public float timeToReachTargetRotation = 0.14f;
    

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float gravityScale = 3f;
    public float fallGravityMultiplier = 0.2f;
    public float reduceVelocityPeak = 5f;
    public float reduceVelocity = 5f;
    public Vector3 groundCheckOffset;
    public Vector3 groundCheckSize;
    public LayerMask ignorePlayerMask;

    #endregion

    #region Player Actions

    [Header("Jump")]
    public float humanJumpHeight = 5f;
    public float jumpRollVelocity = -5f;
    public float rootMotionJumpRollSpeed = 2f;
    

    public float jumpCooldown = 0.2f;
    public float jumpBufferTime = 0.1f;   // Detect jump input before touching the ground
    public float jumpCoyoteTime = 0.2f;   // Allow you to jump when you walk off platform
    public float jumpMultiplier = 1.0f;
    public float doubleJumpHeightPercent = 0.5f;
    

    [Header("Sneaking")]
    public float sneakSpeed = 2f;
    public Vector3 sneakCheckOffset;
    public Vector3 sneakCheckSize;
    

    [Header("Dash")]
    public float humanDashTime = 5.0f;
    public float humanDashDistance = 4.0f;
    public float dashCooldown = 1f;
    

    [Header("Ledge Climb")]
    public float wallCheckDistance = 3.0f;
    public float ledgeHangDistanceOffset;
    public float ledgeHangYOffset;
    public float distanceFromGround = 1f;
    public float ledgeHangCooldown = 1f;
    public Transform wallCheck;
    public Transform ledgeCheck;
    public Transform ledgeRootJntTransform;
    public LayerMask groundLayer;
    

    [Header("Attack")]
    public float attackCooldown = 2f;
    public float resetComboDelay = 1f;
    public float rootMotionAtkSpeed = 2f;
    

    #endregion

    #region Other

    [Header("Stamina")]
    public float staminaConsumption = 20f;
    public float staminaRecovery = 5f;
    public float staminaCooldown = 1f;
    public float maxStamina = 100f;
    
    [Header("Take Damage")]
    public float invulnerableTime = 1f;
    public float regainMovement = 0.5f;
    public float horizontalKnockback = 10f;
    public float verticalKnockback = 10f;

    [Header("Camera")]
    public float camRotationSpeed2D = 0.2f;
    public Transform mainCamera;

    [Header("Path")]
    public PathCreator pathCreator;
    public float maxDistancePath = 0.5f;
    public float distanceSpawn = 0f;
    public float spawnYOffset = 0f;
    public float adjustVelocity = 1.0f; //Velocity to push player towards the path

    [Header("References")]
    public Slider staminaBar;
    public CinemachineVirtualCamera virtualCam2D;
    public PhysicMaterial friction;

    #endregion                                                                                                                                                                                                                                                                
}
