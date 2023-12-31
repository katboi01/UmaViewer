// CySpring
using System;
using System.Collections.Generic;
using Stage;
using UnityEngine;

public class CySpring : MonoBehaviour
{
    public enum eCapability
    {
        None,
        GravityControl,
        WindForce
    }

    public class CollisionParent
    {
        public Transform transform;
    }

    public struct BindContext
    {
        public NativeClothCollision[] commonNativeCollisions;

        public CollisionParent[] commonCollisionParentTable;

        public CySpringCollisionComponent[] commonUnionCollisionList;

        public Vector3[] commonCollisionParentWorldPosition;

        public Quaternion[] commonCoollisionParentWorldRotation;
    }

    private const float DEFAULT_SPECIFIC_RATE = 0.1f;

    private const float HALF_FPS_RATE = 1.5f;

    private static List<CySpringCollisionComponent> _lstUnionCollision = null;

    private static CySpringCollisionComponent _groundCollision = null;

    private static float _stiffnessForceRate = 1f;

    private static float _dragForceRate = 1f;

    private static float _gravityRate = 1f;

    private bool _bCollisionSwitch = true;

    [SerializeField]
    private CySpringParamDataAsset _springData;

    private bool _bUseUnionCollision;

    private CySpringCollisionComponent _unionCollision;

    private CySpringCollisionComponent[] _unionCollisionList;

    private CySpringCollisionComponent[] _collisionList;

    public NativeClothCollision[] _nativeCollisions;

    private CollisionParent[] _collisionParentTable;

    private Vector3[] _collisionParentWorldPosition;

    private Quaternion[] _collisionParentWorldRotation;

    [NonSerialized]
    private float _sqartDeltaTime;

    [SerializeField]
    private CySpringRootBone[] _boneList;

    [NonSerialized]
    private Transform _cachedTransform;

    private bool _isUseSpecificValue;

    private float _specificStiffnessForceRate = 0.1f;

    private float _specificDragForceRate = 0.1f;

    private float _specificGravityRate = 0.1f;

    private bool _needWorldRotationShare;

    private Transform _oftenAccessedParent;

    private static readonly string[] IgnoreTransformName = new string[8] { "Pole_Arm_L", "Pole_Arm_R", "Pole_Leg_L", "Pole_Leg_R", "Eff_Leg_L", "Eff_Leg_R", "Eff_Wrist_L", "Eff_Wrist_R" };

    private static bool _unbinded = false;

    public static float stiffnessForceRate
    {
        set
        {
            _stiffnessForceRate = value;
        }
    }

    public static float dragForceRate
    {
        set
        {
            _dragForceRate = value;
        }
    }

    public static float gravityRate
    {
        set
        {
            _gravityRate = value;
        }
    }

    public bool CollisionSwitch
    {
        get
        {
            return _bCollisionSwitch;
        }
        set
        {
            _bCollisionSwitch = value;
        }
    }

    public CySpringParamDataAsset springData
    {
        set
        {
            _springData = value;
        }
    }

    public CySpringCollisionComponent[] CollisionList => _collisionList;

    public float sqartDeltaTime => _sqartDeltaTime;

    public CySpringRootBone[] boneList => _boneList;

    private void Awake()
    {
        Create();
    }

    private void Start()
    {
        _cachedTransform = base.gameObject.transform;
        SetCollision();
        Reset();
    }

    private void Create()
    {
        _sqartDeltaTime = 0f;
        List<CySpringCollisionComponent> list = new List<CySpringCollisionComponent>(GetComponentsInChildren<CySpringCollisionComponent>());
        if (_springData == null)
        {
            return;
        }
        if (_lstUnionCollision != null && _bUseUnionCollision)
        {
            for (int i = 0; i < _lstUnionCollision.Count; i++)
            {
                CySpringCollisionComponent cySpringCollisionComponent = _lstUnionCollision[i];
                if (!list.Contains(cySpringCollisionComponent) && cySpringCollisionComponent != _unionCollision)
                {
                    list.Add(cySpringCollisionComponent);
                }
            }
        }
        if (_unionCollision != null && list.Contains(_unionCollision))
        {
            list.Remove(_unionCollision);
        }
        if (_groundCollision != null && !list.Contains(_groundCollision))
        {
            list.Add(_groundCollision);
        }
        List<CySpringRootBone> list2 = new List<CySpringRootBone>();
        for (int j = 0; j < _springData.mergeDataList.Count; j++)
        {
            CySpringRootBone cySpringRootBone = new CySpringRootBone();
            cySpringRootBone.CreateRoot(_springData.mergeDataList[j], this, cySpringRootBone, null);
            if (cySpringRootBone.Exist)
            {
                list2.Add(cySpringRootBone);
            }
        }
        _boneList = list2.ToArray();
        _collisionList = list.ToArray();
        if (_nativeCollisions != null)
        {
            return;
        }
        _nativeCollisions = new NativeClothCollision[_collisionList.Length];
        List<CollisionParent> list3 = new List<CollisionParent>(32);
        List<CySpringCollisionComponent> list4 = new List<CySpringCollisionComponent>(8);
        for (int k = 0; k < _collisionList.Length; k++)
        {
            CySpringCollisionComponent cySpringCollisionComponent2 = _collisionList[k];
            Transform parent = cySpringCollisionComponent2.transform.parent;
            int num = list3.FindIndex((CollisionParent x) => x.transform == parent);
            if (num < 0)
            {
                CollisionParent collisionParent = new CollisionParent();
                collisionParent.transform = parent;
                num = list3.Count;
                list3.Add(collisionParent);
            }
            if (cySpringCollisionComponent2.CollisionType == CySpringCollisionData.CollisionType.Sphere && cySpringCollisionComponent2.purpose == CySpringCollisionComponent.ePurpose.Union)
            {
                list4.Add(cySpringCollisionComponent2);
            }
            cySpringCollisionComponent2.SetNativeCollision(_nativeCollisions, k, num);
        }
        _collisionParentTable = list3.ToArray();
        _unionCollisionList = list4.ToArray();
        _collisionParentWorldPosition = new Vector3[list3.Count];
        _collisionParentWorldRotation = new Quaternion[list3.Count];
    }

    private void SetCollision()
    {
        if (_boneList != null)
        {
            for (int i = 0; i < _boneList.Length; i++)
            {
                _boneList[i].SetCollision();
            }
        }
    }

    public void Reset(bool bForceReset = true)
    {
        if (_boneList != null && _boneList.Length != 0)
        {
            if (_cachedTransform == null)
            {
                _cachedTransform = base.gameObject.transform;
            }
            Vector3 position = _cachedTransform.transform.position;
            Quaternion rotation = _cachedTransform.transform.rotation;
            _cachedTransform.position = Vector3.zero;
            _cachedTransform.rotation = Quaternion.identity;
            for (int i = 0; i < _boneList.Length; i++)
            {
                _boneList[i].Reset(bForceReset);
            }
            _cachedTransform.position = position;
            _cachedTransform.rotation = rotation;
        }
    }

    public static GameObject FindGameObject(GameObject rootObject, string objectName)
    {
        if (!objectName.Contains("/"))
        {
            return FindGameObjectLoop(rootObject, objectName);
        }
        string[] array = objectName.Split('/');
        if (array.Length == 0)
        {
            return null;
        }
        GameObject gameObject = rootObject;
        for (int i = 0; i < array.Length; i++)
        {
            gameObject = FindGameObjectLoop(gameObject, array[i]);
            if (gameObject == null)
            {
                return null;
            }
        }
        return gameObject;
    }

    private static GameObject FindGameObjectLoop(GameObject rootObject, string objectName)
    {
        if (rootObject == null)
        {
            return null;
        }
        foreach (Transform item in rootObject.transform)
        {
            if (item.name.Contains(objectName) && Array.IndexOf(IgnoreTransformName, item.name) < 0)
            {
                return item.gameObject;
            }
        }
        foreach (Transform item2 in rootObject.transform)
        {
            GameObject gameObject = FindGameObjectLoop(item2.gameObject, objectName);
            if (gameObject != null)
            {
                return gameObject;
            }
        }
        return null;
    }
}
