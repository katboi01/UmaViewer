using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit)]
public struct NativeClothParameter
{
    [FieldOffset(0)]
    public float _stiffnessForceRate;

    [FieldOffset(4)]
    public float _dragForceRate;

    [FieldOffset(8)]
    public float _gravityRate;

    [FieldOffset(12)]
    public bool _collisionSwitch;

    [FieldOffset(16)]
    public CySpring.eCapability _capability0;

    [FieldOffset(20)]
    public CySpring.eCapability _capability1;

    [FieldOffset(24)]
    public CySpring.eCapability _capability2;

    [FieldOffset(28)]
    public CySpring.eCapability _capability3;

    [FieldOffset(32)]
    public CySpring.eCapability _capability4;

    [FieldOffset(36)]
    public Vector3 _gravityDirection0;

    [FieldOffset(52)]
    public Vector3 _gravityDirection1;

    [FieldOffset(68)]
    public Vector3 _gravityDirection2;

    [FieldOffset(84)]
    public Vector3 _gravityDirection3;

    [FieldOffset(100)]
    public Vector3 _gravityDirection4;

    [FieldOffset(116)]
    public Vector3 _forceScale;
}
