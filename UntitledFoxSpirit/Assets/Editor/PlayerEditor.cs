using Cinemachine.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;
using UnityEngine.WSA;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;

[CustomEditor(typeof(PlayerStateMachine))]
public class PlayerEditor : Editor
{
    List<string> CategoryList = (new List<string>{"MOVEMENT", "ROTATION", "GRAVITY",
                                                 "JUMP", "SNEAKING", "DASH", "LEDGE_CLIMB", "ATTACK",
                                                 "STAMINA", "TAKE_DAMAGE", "CAMERA", "PATH", "OTHER" });


    PlayerStateMachine obj;
    SerializedObject vsoObject;
    VariableScriptObject vso;

    SerializedProperty property;
    bool showPosition;
    bool beginBox = false;
    int countHeader;

    void OnEnable()
    {
        obj = (PlayerStateMachine)target;
        EditorUtility.SetDirty(obj);

        SerializedProperty playerData = serializedObject.FindProperty("vso");
        vsoObject = new SerializedObject(playerData.objectReferenceValue);
        vso = (VariableScriptObject)playerData.objectReferenceValue;
    }

    public override void OnInspectorGUI()
    {
        vsoObject.Update();

        LoadScriptableObject();

        // Style the header of foldout
        GUIStyle style = EditorStyles.foldout;
        style.fontSize = 13;
        style.fontStyle = FontStyle.Bold;

        // Get all variables
        FieldInfo[] fields = vsoObject.targetObject.GetType().GetFields();

        foreach (FieldInfo field in fields)
        {
            // Check if variable is a header
            if (CategoryList.Contains(field.Name))
            {
                if (!beginBox)
                {
                    beginBox = true;
                }
                else
                {
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical("Box");

                string name = field.Name;
                name = name.Replace("_", " ");

                // Get bool value from scriptable object
                showPosition = (bool)field.GetValue(vsoObject.targetObject);

                // Create foldout and get bool if foldout is expanded
                showPosition = EditorGUILayout.Foldout(showPosition, name, true, style);

                // Set bool value in scriptable object
                field.SetValue(vsoObject.targetObject, showPosition);

                continue;
            }

            if (showPosition)
                EditorGUILayout.PropertyField(vsoObject.FindProperty(field.Name));

            
        }


        if (showPosition)
            base.OnInspectorGUI();

        //Component[] comArr = obj.GetComponents<Component>();
        //foreach (Component com in comArr)
        //{
        //    Type baseType = com.GetType().BaseType;

        //    if (baseType.Name != "MonoBehaviour")
        //        continue;

        //    foreach (var info in com.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
        //    {
        //        if (info.FieldType.ToString() == "PlayerBaseState" || info.FieldType.ToString() == "PlayerStateFactory" || !info.IsPublic)
        //            continue;

        //        EditorGUILayout.PropertyField(serializedObject.FindProperty(info.Name));
        //    }
        //}

        vsoObject.ApplyModifiedProperties();
        serializedObject.ApplyModifiedProperties();

        
    }

    void LoadScriptableObject()
    {
        obj.vso = Resources.Load<VariableScriptObject>("PlayerVariables");

        // If scriptable object exists
        if (obj.vso == null)
        {
            // If resources folder exists, if no create the folder
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // Create scriptable object
            obj.vso = ScriptableObject.CreateInstance<VariableScriptObject>();
            AssetDatabase.CreateAsset(obj.vso, "Assets/Resources/PlayerVariables.asset");
        }
    }
}
