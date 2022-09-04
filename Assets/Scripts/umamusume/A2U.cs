using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class A2U
{
    public enum Blend
    {
        Normal = 0,
        Add = 1,
        Multiply = 2,
        Screen = 3,
        Overlay = 4
    }

    public enum Order
    {
        PreImageEffect = 0,
        InImageEffect = 1,
        PostImageEffect = 2
    }
}
