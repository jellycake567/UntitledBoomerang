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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("humanSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("humanDashTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("humanDashDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("humanJumpHeight"));
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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("camera3D"));

        // Jump
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpCooldown"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpBufferTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpCoyoteTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gravity"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityScale"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fallGravityMultiplier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ignorePlayerMask"));

        // Camera
        EditorGUILayout.PropertyField(serializedObject.FindProperty("camRotationSpeed2D"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mainCamera"));

        // References
        EditorGUILayout.PropertyField(serializedObject.FindProperty("path"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("virtualCam2D"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("virtualCam3D"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("human"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fox"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("attack"));

    }
}
