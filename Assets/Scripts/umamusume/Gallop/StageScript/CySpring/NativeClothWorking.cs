using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Explicit)]
public struct NativeClothWorking {
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
  public float _initBoneDistance;
  [FieldOffset(180)]
  public float _stiffnessForce;
  [FieldOffset(184)]
  public float _dragForce;
  [FieldOffset(188)]
  public float _collisionRadius;
  [FieldOffset(192)]
  public float _gravity;
  [FieldOffset(196)]
  public int _activeCollision;
  [FieldOffset(200)]
  public short _cIndex0;
  [FieldOffset(202)]
  public short _cIndex1;
  [FieldOffset(204)]
  public short _cIndex2;
  [FieldOffset(206)]
  public short _cIndex3;
  [FieldOffset(208)]
  public short _cIndex4;
  [FieldOffset(210)]
  public short _cIndex5;
  [FieldOffset(212)]
  public short _cIndex6;
  [FieldOffset(214)]
  public short _cIndex7;
  [FieldOffset(216)]
  public short _cIndex8;
  [FieldOffset(218)]
  public short _cIndex9;
  [FieldOffset(220)]
  public short _cIndex10;
  [FieldOffset(222)]
  public short _cIndex11;
  [FieldOffset(224)]
  public short _cIndex12;
  [FieldOffset(226)]
  public short _cIndex13;
  [FieldOffset(228)]
  public short _cIndex14;
  [FieldOffset(230)]
  public short _cIndex15;
  [FieldOffset(232)]
  public bool _existCollision;
  [FieldOffset(226)]
  public bool _isSkip;
  [FieldOffset(230)]
  private int _padding2;
}