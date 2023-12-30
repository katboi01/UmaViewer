using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothController
{
    public class Cloth
    {
        private Character3DBase.Parts.eCategory _category = Character3DBase.Parts.eCategory.MAX;

        private CySpringCollisionDataAsset _colAsset;

        private CySpringParamDataAsset _prmAsset;

        private CySpringCollision _collision;

        private CySpring _spring;

        private string _oftenAccessedParent;

        public Character3DBase.Parts.eCategory category => _category;

        public CySpringCollisionDataAsset colAsset => _colAsset;

        public CySpringParamDataAsset prmAsset => _prmAsset;

        public CySpringCollision collision
        {
            get
            {
                return _collision;
            }
            set
            {
                _collision = value;
            }
        }

        public CySpring spring
        {
            get
            {
                return _spring;
            }
            set
            {
                _spring = value;
            }
        }

        public string oftenAccessedParent => _oftenAccessedParent;

        public Cloth(Character3DBase.Parts.eCategory category, CySpringCollisionDataAsset collisionDataAsset, CySpringParamDataAsset paramDataAsset, string accessedParent)
        {
            _category = category;
            _colAsset = collisionDataAsset;
            _prmAsset = paramDataAsset;
            _oftenAccessedParent = accessedParent;
        }

        public void Build(List<string> lstPartsCode)
        {
            if (_colAsset != null)
            {
                _colAsset.Build(lstPartsCode);
            }
            if (_prmAsset != null)
            {
                _prmAsset.Build(lstPartsCode);
            }
        }

        public void Reset(bool bForceReset)
        {
            if (_spring != null)
            {
                _spring.Reset(bForceReset);
            }
        }

        public void RemoveCySpringCollision()
        {
            if (_collision != null)
            {
                UnityEngine.Object.Destroy(_collision);
                _collision = null;
            }
        }

        public void Release()
        {
            if (_collision != null)
            {
                UnityEngine.Object.Destroy(_collision);
                _collision = null;
            }
            if (_spring != null)
            {
                UnityEngine.Object.Destroy(_spring);
                _spring = null;
            }
            _colAsset = null;
            _prmAsset = null;
        }
    }

    public enum WindCalcMode
    {
        None,
        Sin,
        Wind
    }

    public static readonly string UNION_COLLISION_NAME = "ColUnion";

    public static readonly float UNION_COLLISION_RADIUS = 0.45f;

    public static readonly string GROUND_COLLISION_NAME = "ColGround";

    public static readonly float GROUND_SPHERE_RADIUS = 1000000f;

    private bool _isReady;

    private bool _isCreated;

    private bool _isBinded;

    private GameObject _targetObject;

    private Transform _targetTransform;

    private Vector3 _bakPosition = Vector3.zero;

    private Quaternion _bakRotation = Quaternion.identity;

    private Cloth _rootCloth;

    private List<Cloth> _lstCloth = new List<Cloth>();

    private ClothParameter _clothParameter = new ClothParameter();

    private bool _useUnionCollision;

    private CySpringCollisionComponent _unionCollision;

    private CySpringCollisionComponent _groundCollision;

    private WindCalcMode _windCalcMode;

    private float _windTime;

    private float _windLoopTime;

    private Vector3 _windPower;

    public bool isReady => _isReady;

    public bool isCreated => _isCreated;

    public bool isBinded => _isBinded;

    public int clothCount => _lstCloth.Count;

    public ClothParameter clothParameter => _clothParameter;

    public bool useUnionCollision => _useUnionCollision;

    public CySpringCollisionComponent unionCollision => _unionCollision;

    public CySpringCollisionComponent groundCollision => _groundCollision;

    public List<Cloth> clothList => _lstCloth;

    private ClothController()
    {
    }

    public ClothController(GameObject target)
    {
        _targetObject = target;
        _targetTransform = target.transform;
        _isReady = false;
        _isCreated = false;
        _isBinded = false;
    }

    private void Destroy(UnityEngine.Object obj)
    {
        if (obj != null)
        {
            UnityEngine.Object.Destroy(obj);
        }
        _isReady = false;
        _isCreated = false;
        _isBinded = false;
    }

    private HashSet<string> BuildUsableCollisionList()
    {
        HashSet<string> setUsableCollision = new HashSet<string>();
        Action<CySpringParamDataAsset> action = delegate (CySpringParamDataAsset prmAsset)
        {
            Action<List<string>> fnAddusableCollision = delegate (List<string> lstCollision)
            {
                for (int l = 0; l < lstCollision.Count; l++)
                {
                    if (!setUsableCollision.Contains(lstCollision[l]))
                    {
                        setUsableCollision.Add(lstCollision[l]);
                    }
                    if (lstCollision[l] == UNION_COLLISION_NAME)
                    {
                        _useUnionCollision = true;
                    }
                }
            };
            Action<List<CySpringParamDataChildElement>> action2 = delegate (List<CySpringParamDataChildElement> lstChild)
            {
                for (int k = 0; k < lstChild.Count; k++)
                {
                    CySpringParamDataChildElement cySpringParamDataChildElement = lstChild[k];
                    fnAddusableCollision(cySpringParamDataChildElement._collisionNameList);
                }
            };
            for (int j = 0; j < prmAsset.mergeDataList.Count; j++)
            {
                CySpringParamDataElement cySpringParamDataElement = prmAsset.mergeDataList[j];
                fnAddusableCollision(cySpringParamDataElement._collisionNameList);
                action2(cySpringParamDataElement._childElements);
            }
        };
        _useUnionCollision = false;
        for (int i = 0; i < _lstCloth.Count; i++)
        {
            action(_lstCloth[i].prmAsset);
        }
        return setUsableCollision;
    }

    public void AddCloth(Cloth cloth)
    {
        if (cloth != null)
        {
            _lstCloth.Add(cloth);
        }
    }

    public IEnumerator CreateCollision(CySpringCollisionComponent.ePurpose purpose, float unionCollisionScale = 1f, float bodyCollisionScale = 1f)
    {
        if (_isReady || _isCreated)
        {
            yield break;
        }
        HashSet<string> setUsableCollision = BuildUsableCollisionList();
        foreach (Cloth item in _lstCloth)
        {
            if (item.category == Character3DBase.Parts.eCategory.Body)
            {
                _rootCloth = item;
            }
            if (item.collision != null)
            {
                Destroy(item.collision);
            }
            item.collision = _targetObject.AddComponent<CySpringCollision>();
            if (item.category == Character3DBase.Parts.eCategory.Body)
            {
                item.collision.CollisionScale = bodyCollisionScale;
            }
            item.collision.Create(item.colAsset, setUsableCollision);
            yield return null;
        }
        if (_rootCloth != null)
        {
            if ((purpose & CySpringCollisionComponent.ePurpose.Union) != 0)
            {
                _unionCollision = _rootCloth.collision.CreateCollision("Chest", UNION_COLLISION_NAME, UNION_COLLISION_RADIUS * unionCollisionScale);
                if (_unionCollision != null)
                {
                    _unionCollision.purpose = CySpringCollisionComponent.ePurpose.Union;
                    CySpring.AddUnionCollision(_unionCollision);
                }
            }
            if ((purpose & CySpringCollisionComponent.ePurpose.Ground) != 0 && setUsableCollision.Contains(GROUND_COLLISION_NAME))
            {
                _groundCollision = _rootCloth.collision.CreateCollision("Position", GROUND_COLLISION_NAME, GROUND_SPHERE_RADIUS);
                if (_groundCollision != null)
                {
                    _groundCollision.purpose = CySpringCollisionComponent.ePurpose.Ground;
                }
            }
        }
        foreach (Cloth item2 in _lstCloth)
        {
            item2.RemoveCySpringCollision();
        }
        _isCreated = true;
    }

    public IEnumerator BindSpring()
    {
        if (_isReady || _isBinded || !_isCreated)
        {
            yield break;
        }
        int cntCloth = _lstCloth.Count;
        CySpring.BindContext context = default(CySpring.BindContext);
        int idx = 0;
        while (idx < cntCloth)
        {
            Cloth cloth = _lstCloth[idx];
            if (cloth.spring != null)
            {
                Destroy(cloth.spring);
            }
            cloth.spring = _targetObject.AddComponent<CySpring>();
            cloth.spring.Bind(cloth.prmAsset, _groundCollision, _unionCollision, _useUnionCollision, cloth.oftenAccessedParent, ref context);
            cloth.spring.GetContext(ref context);
            yield return null;
            int num = idx + 1;
            idx = num;
        }
        bool flag = true;
        for (int i = 0; i < cntCloth; i++)
        {
            flag &= _lstCloth[i].spring != null;
        }
        _isReady = flag;
        _isBinded = _isReady;
    }

    public void Release()
    {
        foreach (Cloth item in _lstCloth)
        {
            item.Release();
        }
        _lstCloth.Clear();
        _rootCloth = null;
        _isReady = false;
        _isCreated = false;
        _isBinded = false;
    }

    public CySpringCollision GetCollision(int idx)
    {
        if (0 > idx || idx >= _lstCloth.Count)
        {
            return null;
        }
        return _lstCloth[idx].collision;
    }

    public CySpring GetSpring(int idx)
    {
        if (0 > idx || idx >= _lstCloth.Count)
        {
            return null;
        }
        return _lstCloth[idx].spring;
    }

    private void UpdateWind(float deltaTime)
    {
        _windTime += deltaTime;
        _windTime %= _windLoopTime;
        switch (_windCalcMode)
        {
            case WindCalcMode.Sin:
                {
                    Vector3 windPower2 = _windPower * Mathf.Sin((float)Math.PI * 2f * _windTime / _windLoopTime) * 0.001f;
                    for (int k = 0; k < _lstCloth.Count; k++)
                    {
                        CySpringRootBone[] boneList2 = _lstCloth[k].spring.boneList;
                        for (int l = 0; l < boneList2.Length; l++)
                        {
                            boneList2[l].UpdateWindPower(windPower2);
                        }
                    }
                    break;
                }
            case WindCalcMode.Wind:
                {
                    float num = (float)Math.PI * 2f * _windTime / _windLoopTime;
                    for (int m = 0; m < _lstCloth.Count; m++)
                    {
                        CySpringRootBone[] boneList3 = _lstCloth[m].spring.boneList;
                        for (int n = 0; n < boneList3.Length; n++)
                        {
                            float index = 0f;
                            boneList3[n].GetWindGroupIndex(ref index);
                            float num2 = Mathf.Sin(num + index) * 0.001f;
                            boneList3[n].UpdateWindPower(_windPower * num2);
                        }
                    }
                    break;
                }
            case WindCalcMode.None:
                {
                    Vector3 windPower = _windPower * 0.001f;
                    for (int i = 0; i < _lstCloth.Count; i++)
                    {
                        CySpringRootBone[] boneList = _lstCloth[i].spring.boneList;
                        for (int j = 0; j < boneList.Length; j++)
                        {
                            boneList[j].UpdateWindPower(windPower);
                        }
                    }
                    break;
                }
        }
    }

    public bool Update(float externalTime, bool isGather = true)
    {
        int count = _lstCloth.Count;
        for (int i = 0; i < count; i++)
        {
            if (_lstCloth[i].spring == null)
            {
                return false;
            }
        }
        if (_windLoopTime > 0f)
        {
            UpdateWind(externalTime);
        }
        if (_unionCollision != null && isGather)
        {
            _unionCollision.UpdateUnionCollision();
        }
        _bakPosition = _targetTransform.position;
        _bakRotation = _targetTransform.rotation;
        _targetTransform.position = Vector3.zero;
        _targetTransform.rotation = Quaternion.identity;
        if (isGather)
        {
            float smoothDeltaTime = Time.smoothDeltaTime;
            for (int j = 0; j < count; j++)
            {
                _lstCloth[j].spring.GatherSpring(smoothDeltaTime, ref _bakPosition, ref _bakRotation);
            }
        }
        for (int k = 0; k < count; k++)
        {
            _lstCloth[k].spring.UpdateSpring(_clothParameter);
        }
        for (int l = 0; l < count; l++)
        {
            _lstCloth[l].spring.PostSpring();
        }
        _targetTransform.position = _bakPosition;
        _targetTransform.rotation = _bakRotation;
        return true;
    }

    public void ForceUpdate(float fCollisionOffSec, float fCollisionOnSec)
    {
        int count = _lstCloth.Count;
        for (int i = 0; i < count; i++)
        {
            if (_lstCloth[i].spring == null)
            {
                return;
            }
        }
        float num = 0.0166666675f;
        int num2 = (int)(fCollisionOffSec / num);
        int num3 = (int)(fCollisionOnSec / num);
        Vector3 position = _targetTransform.position;
        Quaternion rotation = _targetTransform.rotation;
        _targetTransform.position = Vector3.zero;
        _targetTransform.rotation = Quaternion.identity;
        for (int j = 0; j < count; j++)
        {
            _lstCloth[j].spring.CollisionSwitch = false;
        }
        for (int k = 0; k < count; k++)
        {
            for (int l = 0; l < num2; l++)
            {
                _lstCloth[k].spring.UpdateSpring(_clothParameter);
            }
        }
        for (int m = 0; m < count; m++)
        {
            _lstCloth[m].spring.CollisionSwitch = true;
        }
        for (int n = 0; n < count; n++)
        {
            for (int num4 = 0; num4 < num3; num4++)
            {
                _lstCloth[n].spring.UpdateSpring(_clothParameter);
            }
        }
        for (int num5 = 0; num5 < count; num5++)
        {
            _lstCloth[num5].spring.PostSpring();
        }
        _targetTransform.position = position;
        _targetTransform.rotation = rotation;
    }

    public void ExcludeCapability(CySpring.eCapability caps)
    {
        _clothParameter.ExcludeCapability(caps);
    }

    public void SetCapability(CySpring.eCapability caps, int groupId, bool bSwitch, ref Vector3 direction)
    {
        switch (groupId)
        {
            case 0:
                clothParameter.capability0 = (bSwitch ? (clothParameter.capability0 | caps) : (clothParameter.capability0 & ~caps));
                clothParameter.gravityDirection0 = direction;
                break;
            case 1:
                clothParameter.capability1 = (bSwitch ? (clothParameter.capability1 | caps) : (clothParameter.capability1 & ~caps));
                clothParameter.gravityDirection1 = direction;
                break;
            case 2:
                clothParameter.capability2 = (bSwitch ? (clothParameter.capability2 | caps) : (clothParameter.capability2 & ~caps));
                clothParameter.gravityDirection2 = direction;
                break;
            case 3:
                clothParameter.capability3 = (bSwitch ? (clothParameter.capability3 | caps) : (clothParameter.capability3 & ~caps));
                clothParameter.gravityDirection3 = direction;
                break;
            case 4:
                clothParameter.capability4 = (bSwitch ? (clothParameter.capability4 | caps) : (clothParameter.capability4 & ~caps));
                clothParameter.gravityDirection4 = direction;
                break;
        }
    }

    public void EnableUnionCollision(bool bEnable)
    {
        if (_unionCollision != null)
        {
            _unionCollision.EnableUnionCollision(bEnable);
        }
    }

    public void SetWind(WindCalcMode mode, Vector3 windPower, float loopTime)
    {
        _windCalcMode = mode;
        _windPower = windPower;
        _windLoopTime = loopTime;
    }
}
