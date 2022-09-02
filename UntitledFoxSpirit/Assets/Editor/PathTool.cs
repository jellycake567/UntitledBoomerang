using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;

public class PathTool : Editor
{
    [MenuItem("Tools/Path Tool/Reset Points Y-Axis")]
    static void ResetYAxis()
    {
        if (Selection.activeTransform != null)
        {
            Transform selectedTransform = Selection.activeTransform;

            PathCreator pathCreator = selectedTransform.GetComponent<PathCreator>();

            for (int i = 0; i < pathCreator.bezierPath.NumPoints; i++)
            {
                Vector3 point = pathCreator.bezierPath.GetPoint(i);

                pathCreator.bezierPath.SetPoint(i, new Vector3(point.x, 0f, point.z));

            }
        }
        else
        {
            Debug.LogError("Path not selected!");
        }
    }
}
