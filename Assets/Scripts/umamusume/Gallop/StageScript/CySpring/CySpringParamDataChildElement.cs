using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CySpringParamDataChildElement {
    [SerializeField]
    public string _boneName;

    [SerializeField]
    public float _stiffnessForce = 0.0001f;

    [SerializeField]
    public float _dragForce = 0.05f;

    [SerializeField]
    public Vector3 _springForce = new Vector3(0f, 0f, 0f);

    [SerializeField]
    public float _collisionRadius = 0.01f;

    [SerializeField]
    public List<string> _collisionNameList = new List<string>();

    [SerializeField]
    public float _gravity;

    [SerializeField]
    public CySpring.eCapability _capability;

    [SerializeField]
    [Range(0f, 4f)]
    public int _capsGroupIndex;

    public float _verticalWindRateSlow;
    public float _horizontalWindRateSlow;
    public float _verticalWindRateFast;
    public float _horizontalWindRateFast;
    public bool _isLimit;
    public Vector3 _limitAngleMin;
    public Vector3 _limitAngleMax;
    public float MoveSpringApplyRate;
}