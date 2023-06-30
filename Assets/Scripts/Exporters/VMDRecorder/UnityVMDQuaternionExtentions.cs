using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityVMDQuaternionExtentions 
{
    public static Quaternion PlusRotation(this Quaternion q1, Quaternion q2)
    {
        return Quaternion.Euler(q1.eulerAngles + q2.eulerAngles);
    }

    public static Quaternion MinusRotation(this Quaternion q1, Quaternion q2)
    {
        return Quaternion.Euler(q1.eulerAngles - q2.eulerAngles);
    }
}
