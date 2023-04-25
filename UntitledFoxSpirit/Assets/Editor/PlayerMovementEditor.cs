using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovementEditor : Editor
{
    public enum DisplayCategory
    {
        Human, Fox, Other
    }

    public DisplayCategory categoryToDisplay;

    public override void OnInspectorGUI()
    {
        categoryToDisplay = (DisplayCategory)EditorGUILayout.EnumPopup("Display", categoryToDisplay);

        EditorGUILayout.Space();

        switch (categoryToDisplay)
        {
            case DisplayCategory.Human:
                DisplayHumanInfo();
                break;
            case DisplayCategory.Fox:
                DisplayFoxInfo();
                break;
            case DisplayCategory.Other:
                DisplayOtherInfo();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DisplayHumanInfo()
    {
        // Attack
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackCooldown"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("resetComboDelay"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rootMotionAtkSpeed"));

        // Movement
        EditorGUILayout.PropertyField(serializedObject.FindProperty("humanSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("humanRunSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("accelTimeToMaxSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("decelTimeToZeroSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("animJogSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("animJogAccelSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("animJogDecelSpeed"));

        // Rotation
        EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToReachTargetRotation"));

        // Sneaking
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sneakSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sneakCheckOffset"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sneakCheckSize"));


        // Dash
        EditorGUILayout.PropertyField(serializedObject.FindProperty("humanDashTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("humanDashDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dashCooldown"));

        // Jump
        EditorGUILayout.PropertyField(serializedObject.FindProperty("humanJumpHeight"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpRollVelocity"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("rootMotionJumpRollSpeed"));

        // Stamina
        EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaConsumption"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaRecovery"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaCooldown"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStamina"));

        // Wall Climb
        EditorGUILayout.PropertyField(serializedObject.FindProperty("wallCheckDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ledgeHangDistanceOffset"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ledgeHangYOffset"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceFromGround"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ledgeHangCooldown"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("wallCheck"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ledgeCheck"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundLayer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("climbAnim"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ledgeRootJntTransform"));
    }

    void DisplayFoxInfo()
    {
        EditorGUILayout.PropertyField(serializedObject.FindProperty("foxSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("foxDashTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("foxDashDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("foxJumpHeight"));
    }

    void DisplayOtherInfo()
    {
        // Movement
        EditorGUILayout.PropertyField(serializedObject.FindProperty("turnSmoothTime2D"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("turnSmoothTime3D"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxVelocityChange"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("frictionAmount"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mode3D"));

        // Step Climb
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stepRayUpper"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stepRayUpperDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stepRayLower"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stepRayLowerDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stepHeight"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stepSmooth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("stepDebug"));

        // Jump
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpCooldown"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpBufferTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpCoyoteTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpMultiplier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("doubleJumpHeightPercent"));

        // Gravity
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityScale"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fallGravityMultiplier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("reduceVelocity"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundCheckOffset"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundCheckSize"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ignorePlayerMask"));


        // Take Damage
        EditorGUILayout.PropertyField(serializedObject.FindProperty("invulnerableTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("horizontalKnockback"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("verticalKnockback"));

        float currentValue = serializedObject.FindProperty("regainMovement").floatValue;
        float rightValue = serializedObject.FindProperty("invulnerableTime").floatValue;

        currentValue = EditorGUILayout.Slider("Regain Movement", currentValue, 0f, rightValue);
        serializedObject.FindProperty("regainMovement").floatValue = currentValue;

        // Camera
        EditorGUILayout.PropertyField(serializedObject.FindProperty("camRotationSpeed2D"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera"));

        // Path
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pathCreator"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDistancePath"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("distanceSpawn"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnYOffset"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("adjustVelocity"));

        // References
        EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaBar"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("virtualCam2D"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("virtualCam3D"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("human"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fox"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("friction"));


    }
}
