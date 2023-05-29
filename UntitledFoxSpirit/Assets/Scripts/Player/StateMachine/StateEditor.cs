using Cinemachine.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;

[CustomEditor(typeof(PlayerVariables))]
public class PlayerEditor : Editor
{
    GameObject targetObject;

    void OnEnable()
    {
        PlayerVariables script = (PlayerVariables)target;
        targetObject = script.gameObject;
    }

    public override void OnInspectorGUI()
    {
        Component[] comArr = (Component[])targetObject.GetComponents<Component>();
        foreach (Component com in comArr)
        {
            Type baseType = com.GetType().BaseType;

            if (baseType.Name != "MonoBehaviour")
                continue;
            
            //Debug.Log("type " + com.GetType() + " basetype " + baseType.Name + " fields: " + com.GetType().GetFields().Length);

            foreach (FieldInfo field in com.GetType().GetFields())
            {
                if (com.GetType().Name != "Test")
                    continue;

                    

                //EditorGUILayout.PropertyField(serializedObject.FindProperty(field.Name));
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("test"));


                //Debug.Log("fi name " + field.Name + " val " + field.GetValue(com));
                
                
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
