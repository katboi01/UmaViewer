using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit)]
public struct NativeClothCollision
{
    [FieldOffset(0)]
    public Vector3 _position;

    [FieldOffset(16)]
    public Vector3 _position2;

    [FieldOffset(32)]
    public int _type;

    [FieldOffset(36)]
    public float _radius;

    [FieldOffset(40)]
    public int _parentIndex;

    [FieldOffset(44)]
    public int _purpose;
}
