using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeepCopy
{
    public static void CopyValues<T>(T from, T to)
    {
        var json = JsonUtility.ToJson(from);
        JsonUtility.FromJsonOverwrite(json, to);
    }
}
