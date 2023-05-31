using Cinemachine.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.WSA;

[CustomEditor(typeof(PlayerStateMachine))]
public class PlayerEditor : Editor
{
    GameObject targetObject;

    void OnEnable()
    {
        PlayerStateMachine script = (PlayerStateMachine)target;
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

            //SerializedObject serObj = new SerializedObject(com);
            //SerializedProperty prop = serObj.GetIterator();

            Debug.Log("type " + com.GetType() + " basetype " + baseType.Name + " fields: " + com.GetType().BaseType);

            //while (prop.NextVisible(true))
            //{
            //    if (prop.name == "x" || prop.name == "y" || prop.name == "z")
            //        continue;
            //
            //    EditorGUILayout.PropertyField(serializedObject.FindProperty(prop.name));
            //}

            PropertyInfo[] parry = com.GetType().GetProperties();

            foreach (var info in com.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
            {
                if (!info.IsPublic)
                    continue;

                if (info.FieldType.ToString() == "PlayerBaseState" || info.FieldType.ToString() == "PlayerStateFactory")
                    continue;

                EditorGUILayout.PropertyField(serializedObject.FindProperty(info.Name));
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
