using System.Collections.Generic;
using UnityEngine;

public class CySpringRootBone : CySpringBoneBase
{
    public struct OverrideParameter
    {
        public bool enableGravity;

        public float gravity;

        public bool enableStiffness;

        public float stiffness;

        public bool enableDragForce;

        public float dragForce;
    }

    public const float DefaultSuppressionLength = 1f;

    public List<string> _collisionNameList;

    private List<CySpringParamDataChildElement> _childElements;

    private int _depth;

    private List<CySpringBoneBase> _tmpChildBoneList;

    private CySpringBoneBase[] _childBoneList;

    private CySpringParamDataElement _rootElement;

    private bool _isRootElement;

    private bool _isSuppression;

    private float _suppressionLength = 1f;

    private bool _applyCollision = true;

    public NativeClothWorking[] _natives;

    protected bool _hasOftenAccessedParent;

    public bool _isSkip;

    public bool ApplyCollision => _applyCollision;

    public List<CySpringParamDataChildElement> ChildElements => _childElements;

    public CySpringBoneBase[] ChildBoneList => _childBoneList;

    public bool IsSuppression
    {
        get
        {
            return _isSuppression;
        }
        set
        {
            _isSuppression = value;
        }
    }

    public float SuppressionLength
    {
        get
        {
            return _suppressionLength;
        }
        set
        {
            _suppressionLength = value;
        }
    }

    public List<CySpringBoneBase> TmpChildBoneList => _tmpChildBoneList;

    public void CreateRoot(CySpringParamDataElement element, CySpring root, CySpringRootBone rootBone, CySpringBoneBase parentBone)
    {
        _boneName = element._boneName;
        _collisionNameList = element._collisionNameList;
        _childElements = element._childElements;
        _rootElement = element;
        _isRootElement = _rootElement != null;
        Transform transform = root.gameObject.transform;
        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        Create(root, rootBone, parentBone);
        if (!_exist)
        {
            transform.position = position;
            transform.rotation = rotation;
            return;
        }
        _tmpChildBoneList = new List<CySpringBoneBase>();
        foreach (Transform item in _transform)
        {
            if (!(item == _transform) && !(item.GetComponent<CySpringCollisionComponent>() != null))
            {
                CySpringBoneBase cySpringBoneBase = new CySpringBoneBase(item.name);
                _tmpChildBoneList.Add(cySpringBoneBase);
                cySpringBoneBase.CreateChild(_root, _rootBone, this);
            }
        }
        _childBoneList = _tmpChildBoneList.ToArray();
        _tmpChildBoneList = null;
        _depth = _childBoneList.Length;
        _natives = new NativeClothWorking[_childBoneList.Length + 1];
        for (int i = 0; i < _natives.Length; i++)
        {
            _natives[i]._stiffnessForce = 0.0001f;
            _natives[i]._dragForce = 0.05f;
            _natives[i]._springForce = new Vector3(0f, 0f, 0f);
            _natives[i]._collisionRadius = 0.01f;
            _natives[i]._boneAxis = new Vector3(0f, 1f, 0f);
            _natives[i]._gravity = 0f;
            _natives[i]._wind = 0;
            _natives[i]._windGroupIndex = 0f;
        }
        Initialize(0);
        for (int j = 0; j < _childBoneList.Length; j++)
        {
            _childBoneList[j].Initialize(j + 1);
        }
        _natives[0]._stiffnessForce = element._stiffnessForce;
        _natives[0]._dragForce = element._dragForce;
        _natives[0]._springForce = element._springForce;
        _natives[0]._collisionRadius = element._collisionRadius;
        _natives[0]._gravity = element._gravity;
        _natives[0]._capability = element._capability;
        _natives[0]._capsGroupIndex = element._capsGroupIndex;
        transform.position = position;
        transform.rotation = rotation;
    }

    public bool CheckHasOftenAccessedParent(string oftenAccessedParent)
    {
        _hasOftenAccessedParent = false;
        if (_parentTransform != null && _parentTransform.name.Equals(oftenAccessedParent))
        {
            _hasOftenAccessedParent = true;
        }
        return _hasOftenAccessedParent;
    }

    private void UpdateCollisionIndexList()
    {
        if (_collisionNameList == null || _collisionNameList.Count == 0 || _root.CollisionList == null || _root.CollisionList.Length == 0)
        {
            return;
        }
        CreateCollisionIndexList();
        foreach (string collisionName in _collisionNameList)
        {
            int i = 0;
            for (int num = _root.CollisionList.Length; i < num; i++)
            {
                CySpringCollisionComponent cySpringCollisionComponent = _root.CollisionList[i];
                if (!(cySpringCollisionComponent == null) && !(collisionName != cySpringCollisionComponent.name))
                {
                    AddCollisionIndex(i);
                }
            }
        }
    }

    public void SetCollision()
    {
        UpdateCollisionIndexList();
        ChangeRootValue();
    }

    public void DoGatherAllSpring(ref Quaternion oftenAccessedParentRot)
    {
        if (_depth < 0)
        {
            _isSkip = true;
            return;
        }
        _isSkip = false;
        UpdateNativeOldTransform(ref oftenAccessedParentRot);
        DoGatherSpring();
        CySpringBoneBase[] childBoneList = _childBoneList;
        for (int i = 0; i < childBoneList.Length; i++)
        {
            childBoneList[i].DoGatherSpring();
        }
    }

    public void DoGatherAllSpringReset(ref Quaternion oftenAccessedParentRot)
    {
        if (_depth < 0)
        {
            _isSkip = true;
            return;
        }
        _isSkip = false;
        UpdateNativeOldTransform(ref oftenAccessedParentRot);
        DoGatherSpringReset();
        CySpringBoneBase[] childBoneList = _childBoneList;
        for (int i = 0; i < childBoneList.Length; i++)
        {
            childBoneList[i].DoGatherSpringReset();
        }
    }

    public void UpdateNativeOldTransform(ref Quaternion oftenAccessedParentRot)
    {
        Vector3 position = _transform.position;
        if (_hasOftenAccessedParent)
        {
            _rootBone._natives[_index]._oldRotation = oftenAccessedParentRot * _rootBone._natives[_index]._initLocalRotation;
        }
        else
        {
            _rootBone._natives[_index]._oldRotation = _parentTransform.rotation * _rootBone._natives[_index]._initLocalRotation;
        }
        if (_isSuppression)
        {
            Vector3 vector = position - _rootBone._natives[_index]._oldPosition;
            float sqrMagnitude = vector.sqrMagnitude;
            if (sqrMagnitude >= _suppressionLength + float.Epsilon)
            {
                float num = 0.01f;
                if (sqrMagnitude < 0.0001f)
                {
                    num = Mathf.Sqrt(sqrMagnitude);
                    vector.x /= num;
                    vector.y /= num;
                    vector.z /= num;
                }
                else
                {
                    vector.Normalize();
                }
                _rootBone._natives[_index]._currentPosition = position;
                _rootBone._natives[_index]._prevPosition = position - vector * num;
                _rootBone._natives[_index]._finalRotation = _rootBone._natives[_index]._oldRotation;
                for (int i = 1; i < _rootBone._natives.Length; i++)
                {
                    _rootBone._natives[i]._currentPosition = _rootBone._natives[i - 1]._currentPosition;
                    _rootBone._natives[i]._prevPosition = _rootBone._natives[i - 1]._prevPosition;
                    _rootBone._natives[i]._finalRotation = _rootBone._natives[i - 1]._finalRotation;
                }
            }
            _suppressionLength = 1f;
        }
        _rootBone._natives[_index]._oldPosition = position;
    }

    public void DoPostAllSpring()
    {
        DoPostSpring();
        CySpringBoneBase[] childBoneList = _childBoneList;
        for (int i = 0; i < childBoneList.Length; i++)
        {
            childBoneList[i].DoPostSpring();
        }
    }

    public override bool Reset(bool bForceReset = true)
    {
        if (base.Reset(bForceReset))
        {
            if (_childBoneList == null)
            {
                return false;
            }
            CySpringBoneBase[] childBoneList = _childBoneList;
            for (int i = 0; i < childBoneList.Length; i++)
            {
                childBoneList[i].Reset(bForceReset);
            }
        }
        return true;
    }

    public void ChangeRootValue()
    {
        if (_childBoneList != null)
        {
            _rootBone._natives[_index]._existCollision = _rootBone._applyCollision;
            CySpringBoneBase[] childBoneList = _childBoneList;
            for (int i = 0; i < childBoneList.Length; i++)
            {
                childBoneList[i].ChangeValue();
            }
        }
    }

    public override void ResetClothParameter()
    {
        if (_isRootElement)
        {
            _natives[0]._stiffnessForce = _rootElement._stiffnessForce;
            _natives[0]._dragForce = _rootElement._dragForce;
            _natives[0]._gravity = _rootElement._gravity;
            int num = _childBoneList.Length;
            for (int i = 0; i < num; i++)
            {
                _childBoneList[i].ResetClothParameter();
            }
        }
    }

    public void OverrideClothParameter(ref OverrideParameter overrideParameter)
    {
        if (!_isRootElement)
        {
            return;
        }
        if (overrideParameter.enableStiffness)
        {
            _natives[0]._stiffnessForce = overrideParameter.stiffness;
        }
        if (overrideParameter.enableDragForce)
        {
            _natives[0]._dragForce = overrideParameter.dragForce;
        }
        if (overrideParameter.enableGravity)
        {
            _natives[0]._gravity = overrideParameter.gravity;
        }
        int num = _childBoneList.Length;
        for (int i = 0; i < num; i++)
        {
            int index = _childBoneList[i]._index;
            if (overrideParameter.enableStiffness)
            {
                _natives[index]._stiffnessForce = overrideParameter.stiffness;
            }
            if (overrideParameter.enableDragForce)
            {
                _natives[index]._dragForce = overrideParameter.dragForce;
            }
            if (overrideParameter.enableGravity)
            {
                _natives[index]._gravity = overrideParameter.gravity;
            }
        }
    }

    public void GetWindGroupIndex(ref float index)
    {
        index = _natives[0]._windGroupIndex;
    }

    public void UpdateWindPower(Vector3 windPower)
    {
        _natives[0]._windPower = windPower;
    }
}
