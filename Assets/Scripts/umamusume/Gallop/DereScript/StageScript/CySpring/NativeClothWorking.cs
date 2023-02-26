using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit)]
public struct NativeClothWorking
{
    [FieldOffset(0)]
    public Quaternion _initLocalRotation;

    [FieldOffset(16)]
    public Quaternion _oldRotation;

    [FieldOffset(32)]
    public Quaternion _finalRotation;

    [FieldOffset(48)]
    public Vector3 _boneAxis;

    [FieldOffset(64)]
    public Vector3 _currentPosition;

    [FieldOffset(80)]
    public Vector3 _prevPosition;

    [FieldOffset(96)]
    public Vector3 _force;

    [FieldOffset(112)]
    public Vector3 _aimVector;

    [FieldOffset(128)]
    public Vector3 _diff;

    [FieldOffset(144)]
    public Vector3 _oldPosition;

    [FieldOffset(160)]
    public Vector3 _springForce;

    [FieldOffset(176)]
    public Vector3 _windPower;

    [FieldOffset(192)]
    public float _initBoneDistance;

    [FieldOffset(196)]
    public float _stiffnessForce;

    [FieldOffset(200)]
    public float _dragForce;

    [FieldOffset(204)]
    public float _collisionRadius;

    [FieldOffset(208)]
    public float _gravity;

    [FieldOffset(212)]
    public int _activeCollision;

    [FieldOffset(216)]
    public short _cIndex0;

    [FieldOffset(218)]
    public short _cIndex1;

    [FieldOffset(220)]
    public short _cIndex2;

    [FieldOffset(222)]
    public short _cIndex3;

    [FieldOffset(224)]
    public short _cIndex4;

    [FieldOffset(226)]
    public short _cIndex5;

    [FieldOffset(228)]
    public short _cIndex6;

    [FieldOffset(230)]
    public short _cIndex7;

    [FieldOffset(232)]
    public short _cIndex8;

    [FieldOffset(234)]
    public short _cIndex9;

    [FieldOffset(236)]
    public short _cIndex10;

    [FieldOffset(238)]
    public short _cIndex11;

    [FieldOffset(240)]
    public short _cIndex12;

    [FieldOffset(242)]
    public short _cIndex13;

    [FieldOffset(244)]
    public short _cIndex14;

    [FieldOffset(246)]
    public short _cIndex15;

    [FieldOffset(248)]
    public short _cIndex16;

    [FieldOffset(250)]
    public short _cIndex17;

    [FieldOffset(252)]
    public short _cIndex18;

    [FieldOffset(254)]
    public short _cIndex19;

    [FieldOffset(256)]
    public bool _existCollision;

    [FieldOffset(260)]
    public bool _isSkip;

    [FieldOffset(264)]
    public CySpring.eCapability _capability;

    [FieldOffset(268)]
    public int _capsGroupIndex;

    [FieldOffset(272)]
    public int _wind;

    [FieldOffset(276)]
    public float _windGroupIndex;
}
