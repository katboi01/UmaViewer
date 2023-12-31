using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Easing
{
    static public float EaseIn(float x)
    {
        return x * x * x;
    }

    static public float EaseOut(float x)
    {
        return 1 - Mathf.Pow(1 - x, 3);
    }

    static public float EaseInOut(float x)
    {
        return x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
    }
}
