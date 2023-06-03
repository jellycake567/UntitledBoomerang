using Cinemachine.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.WSA;

[CustomEditor(typeof(PlayerStateMachine))]
public class PlayerEditor : Editor
{
    private Editor cachedEditor;

    PlayerStateMachine obj;

    //GameObject targetObject;

    void OnEnable()
    {
        obj = (PlayerStateMachine)target;
        EditorUtility.SetDirty(obj);
        cachedEditor = null;
    }

    public override void OnInspectorGUI()
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

        // Editor stuff
        if (cachedEditor == null)
        {
            cachedEditor = Editor.CreateEditor(obj.vso);
        }

        // Draw scriptable object variables in inspector
        cachedEditor.DrawDefaultInspector();

        //Component[] comArr = (Component[])targetObject.GetComponents<Component>();
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

        //serializedObject.ApplyModifiedProperties();
    }
}
