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

    public static bool MOVEMENT; // Never use, editor stuff

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


    public static bool ROTATION; // Never use, editor stuff

    [Header("Rotation")]
    public float timeToReachTargetRotation = 0.14f;


    public static bool GRAVITY; // Never use, editor stuff

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

    public static bool JUMP; // Never use, editor stuff

    [Header("Jump")]
    public float humanJumpHeight = 5f;
    public float jumpRollVelocity = -5f;
    public float rootMotionJumpRollSpeed = 2f;
    public float jumpCooldown = 0.2f;
    public float jumpBufferTime = 0.1f;   // Detect jump input before touching the ground
    public float jumpCoyoteTime = 0.2f;   // Allow you to jump when you walk off platform
    public float jumpMultiplier = 1.0f;
    public float doubleJumpHeightPercent = 0.5f;


    public static bool SNEAKING; // Never use, editor stuff

    [Header("Sneaking")]
    public float sneakSpeed = 2f;
    public Vector3 sneakCheckOffset;
    public Vector3 sneakCheckSize;


    public static bool DASH; // Never use, editor stuff

    [Header("Dash")]
    public float humanDashTime = 5.0f;
    public float humanDashDistance = 4.0f;
    public float dashCooldown = 1f;


    public static bool LEDGECLIMB; // Never use, editor stuff

    [Header("Ledge Climb")]
    public float wallCheckDistance = 3.0f;
    public float ledgeHangDistanceOffset;
    public float ledgeHangYOffset;
    public float distanceFromGround = 1f;
    public float ledgeHangCooldown = 1f;
    public LayerMask groundLayer;


    public static bool ATTACK; // Never use, editor stuff

    [Header("Attack")]
    public float attackCooldown = 2f;
    public float resetComboDelay = 1f;
    public float rootMotionAtkSpeed = 2f;


    #endregion

    #region Other

    public static bool STAMINA; // Never use, editor stuff

    [Header("Stamina")]
    public float staminaConsumption = 20f;
    public float staminaRecovery = 5f;
    public float staminaCooldown = 1f;
    public float maxStamina = 100f;


    public static bool TAKEDAMAGE; // Never use, editor stuff

    [Header("Take Damage")]
    public float invulnerableTime = 1f;
    public float regainMovement = 0.5f;
    public float horizontalKnockback = 10f;
    public float verticalKnockback = 10f;


    public static bool CAMERA; // Never use, editor stuff

    [Header("Camera")]
    public float camRotationSpeed2D = 0.2f;


    public static bool PATH; // Never use, editor stuff

    [Header("Path")]
    public float maxDistancePath = 0.5f;
    public float distanceSpawn = 0f;
    public float spawnYOffset = 0f;
    public float adjustVelocity = 1.0f; //Velocity to push player towards the path

    #endregion                                                                                                                                                                                                                                                                
}
