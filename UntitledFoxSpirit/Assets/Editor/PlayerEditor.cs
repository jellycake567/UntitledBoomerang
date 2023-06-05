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

[CustomEditor(typeof(PlayerStateMachine))]
public class PlayerEditor : Editor
{
    List<string> CategoryList = new List<string>{"MOVEMENT", "ROTATION", "GRAVITY",
                                                 "JUMP", "SNEAKING", "DASH", "LEDGECLIMB", "ATTACK",
                                                 "STAMINA", "TAKEDAMAGE", "CAMERA", "PATH" };


    PlayerStateMachine obj;
    SerializedObject vsoObject;

    void OnEnable()
    {
        obj = (PlayerStateMachine)target;
        EditorUtility.SetDirty(obj);

        SerializedProperty playerData = serializedObject.FindProperty("vso");
        vsoObject = new SerializedObject(playerData.objectReferenceValue);

    }

    public override void OnInspectorGUI()
    {
        vsoObject.Update();

        LoadScriptableObject();


        FieldInfo[] fields = vsoObject.targetObject.GetType().GetFields();

        foreach (FieldInfo field in fields)
        {
            if (CategoryList.Contains(field.Name))
            {




                continue;
            }


            EditorGUILayout.PropertyField(vsoObject.FindProperty(field.Name));

        }

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
