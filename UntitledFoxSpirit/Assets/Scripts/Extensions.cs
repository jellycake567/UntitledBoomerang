using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector3 IgnoreYAxis(this Vector3 position)
    {
        position.y = 0f;
        return position;
    }
}
