using Cutt;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    public class LaserObject
    {
        private const float DEG_CIRCLE_DIV_THREE = 120f;

        private const float DEG_CIRCLE_DIV_FIVE = 72f;

        private const float FACTOR_ONE_DIV_THREE = 0.333333343f;

        private Transform _parentTransform;

        private Transform _cachedTransform;

        private Renderer _renderer;

        private Vector3 _dirForward = Vector3.forward;

        private Vector3 _dirRight = Vector3.right;

        private float _distLevel;

        private float _rotationWeight;

        private int _nameHash;

        public Transform parentTransform => _parentTransform;

        public Transform cachedTransform => _cachedTransform;

        public bool enable
        {
            get
            {
                return _renderer.enabled;
            }
            set
            {
                _renderer.enabled = value;
            }
        }

        public int nameHash => _nameHash;

        public void Init(Transform tm, int rndSeed)
        {
            _cachedTransform = tm;
            _parentTransform = _cachedTransform.parent;
            _renderer = tm.GetComponent<Renderer>();
            _nameHash = FNVHash.Generate(_cachedTransform.name);
        }

        public void UpdateFormation(eLaserForm formation, int idx)
        {
            _dirForward = Vector3.zero;
            _distLevel = 1f;
            _rotationWeight = 1f;
            float num;
            float num2;
            switch (formation)
            {
                case eLaserForm.One:
                    _dirForward = Vector3.zero;
                    _distLevel = 0f;
                    break;
                case eLaserForm.Linear_2:
                    _dirForward.x = ((idx == 0) ? 1f : (-1f));
                    _distLevel = 0.5f;
                    break;
                case eLaserForm.Linear_3:
                    _dirForward.x = idx - 1;
                    _distLevel = 1f;
                    break;
                case eLaserForm.Linear_4:
                    _dirForward.x = ((idx % 2 == 0) ? 1f : (-1f));
                    if (idx > 1)
                    {
                        _distLevel = 1.5f;
                        _rotationWeight = 1f;
                    }
                    else
                    {
                        _distLevel = 0.5f;
                        _rotationWeight = 0.333333343f;
                    }
                    break;
                case eLaserForm.Linear_5:
                    if (idx == 0)
                    {
                        _dirForward.x = 0f;
                        _distLevel = 0f;
                        break;
                    }
                    _dirForward.x = ((idx < 3) ? (-1f) : 1f);
                    if (idx % 2 == 0)
                    {
                        _distLevel = 2f;
                    }
                    else
                    {
                        _rotationWeight = 0.5f;
                    }
                    break;
                case eLaserForm.Circle_2:
                    _dirForward.x = ((idx == 0) ? 1f : (-1f));
                    break;
                case eLaserForm.Circle_3:
                    num = 120f;
                    num2 = num;
                    num2 *= idx;
                    _dirForward.z = 1f;
                    _dirForward = Quaternion.Euler(0f, num2, 0f) * _dirForward;
                    _dirForward.Normalize();
                    break;
                case eLaserForm.Circle_4:
                    {
                        float num3 = ((idx > 1) ? 1f : (-1f));
                        if (idx % 2 == 0)
                        {
                            _dirForward.x = num3;
                        }
                        else
                        {
                            _dirForward.z = num3;
                        }
                        break;
                    }
                case eLaserForm.Circle_5:
                    num = 72f;
                    num2 = num;
                    num2 *= idx;
                    _dirForward.z = 1f;
                    _dirForward = Quaternion.Euler(0f, num2, 0f) * _dirForward;
                    _dirForward.Normalize();
                    break;

            }
            _dirRight = Vector3.Cross(Vector3.up, _dirForward);
        }

        public void UpdatePitch(float degPitch)
        {
            _parentTransform.localRotation = Quaternion.AngleAxis(degPitch * _rotationWeight, _dirRight);
        }

        public void UpdatePositionInterval(float posInterval)
        {
            _parentTransform.localPosition = _dirForward * (posInterval * _distLevel);
        }
    }

    public const int MAX_LASER_OBJ_COUNT = 5;

    private Director _director;

    private Transform _targetCameraTransform;

    private Transform _rootTransform;

    private Transform _parentTransform;

    private LaserObject[] _arrLaserObject;

    private Quaternion[] _baseLocalAngle;

    private Quaternion[] _offsetLocalAngle;

    private int _curCountEnabled;

    private int _curCountObject;

    private float _prevElapsedTime;

    private eLaserForm _curFormation;

    private float _curDegRootYaw;

    private float _curDegLaserPitch;

    private float _curPosInterval;

    private bool _isBlink;

    private float _blinkPeriod;

    private float _blinkTime;

    private bool _isPause;

    private int _parentNameHash;

    private static bool _isNoise;

    [SerializeField]
    private Camera _targetCamera;

    public static bool isNoise => _isNoise;

    private Camera targetCamera
    {
        get
        {
            return _targetCamera;
        }
        set
        {
            if (!(_targetCamera == value))
            {
                _targetCamera = value;
                if (_targetCamera != null)
                {
                    _targetCameraTransform = _targetCamera.transform;
                }
            }
        }
    }

    private void Start()
    {
        _director = Director.instance;
        _rootTransform = base.gameObject.transform;
        _parentTransform = ((_rootTransform.parent != null) ? _rootTransform.parent : _rootTransform);
        _parentNameHash = FNVHash.Generate(_parentTransform.name);
        LaserObject laserObject = null;
        List<LaserObject> list = new List<LaserObject>();
        Transform[] componentsInChildren = _rootTransform.GetComponentsInChildren<Transform>();
        int num = componentsInChildren.Length;
        for (int i = 0; i < num; i++)
        {
            if (componentsInChildren[i].childCount == 0)
            {
                laserObject = new LaserObject();
                laserObject.Init(componentsInChildren[i], i);
                list.Add(laserObject);
                if (_curFormation == eLaserForm.Off)
                {
                    laserObject.enable = false;
                    continue;
                }
                laserObject.enable = true;
                _curCountEnabled++;
            }
        }
        _arrLaserObject = list.ToArray();
        _curCountObject = _arrLaserObject.Length;
        _baseLocalAngle = new Quaternion[_curCountObject];
        for (int j = 0; j < _baseLocalAngle.Length; j++)
        {
            _baseLocalAngle[j] = _arrLaserObject[j].cachedTransform.localRotation;
        }
        _offsetLocalAngle = new Quaternion[_curCountObject];
    }

    private void LateUpdate()
    {
        if (Director.instance == null)
        {
            //Directorがない＝PhotoStudioの場合
            targetCamera = Camera.main;
        }
        else
        {
            targetCamera = Director.instance.mainCamera;
        }

        if (_targetCamera == null || _curCountEnabled == 0)
        {
            return;
        }
        LaserObject laserObject = null;
        Vector3 zero = Vector3.zero;
        Vector3 view = Vector3.zero;
        Vector3 zero2 = Vector3.zero;
        Quaternion identity = Quaternion.identity;
        float num = 0f;
        _blinkTime += (_isPause ? 0f : (Time.time - _prevElapsedTime));
        _prevElapsedTime = Time.time;
        bool flag = _isBlink & (_blinkTime >= _blinkPeriod);
        for (int i = 0; i < _curCountEnabled; i++)
        {
            laserObject = _arrLaserObject[i];
            if (laserObject == null)
            {
                continue;
            }
            if (flag)
            {
                laserObject.enable = !laserObject.enable;
                if (!laserObject.enable)
                {
                    continue;
                }
            }
            laserObject.cachedTransform.localRotation = _baseLocalAngle[i];
            identity = laserObject.cachedTransform.rotation;
            identity *= _offsetLocalAngle[i];
            zero = identity * Vector3.up;
            view = _targetCameraTransform.position - laserObject.cachedTransform.position;
            num = zero.x * view.x + zero.y * view.y + zero.z * view.z;
            zero2.x = zero.x * num;
            zero2.y = zero.y * num;
            zero2.z = zero.z * num;
            view.x -= zero2.x;
            view.y -= zero2.y;
            view.z -= zero2.z;
            identity.SetLookRotation(view, zero);
            laserObject.cachedTransform.rotation = identity;
        }
        if (flag)
        {
            this._blinkTime = 0f;
        }
    }

    public void UpdateInfo(ref LaserUpdateInfo updateInfo, Dictionary<int, bool> ignoreHashDic)
    {
        bool flag = false;
        if (_curFormation != updateInfo.formation)
        {
            _curFormation = updateInfo.formation;
            _curCountEnabled = (int)_curFormation % 6;
            for (int i = 0; i < _curCountEnabled; i++)
            {
                LaserObject obj = _arrLaserObject[i];
                obj.enable = true;
                obj.UpdateFormation(_curFormation, i);
                flag = true;
            }
            for (int j = _curCountEnabled; j < _curCountObject; j++)
            {
                _arrLaserObject[j].enable = false;
            }
        }
        if (_curCountEnabled < 1)
        {
            return;
        }
        if (_curDegRootYaw != updateInfo.degRootYaw)
        {
            _curDegRootYaw = updateInfo.degRootYaw;
            if (_rootTransform != null)
            {
                _rootTransform.localRotation = Quaternion.Euler(0f, _curDegRootYaw, 0f);
            }
        }
        if (_curDegLaserPitch != updateInfo.degLaserPitch || flag)
        {
            _curDegLaserPitch = updateInfo.degLaserPitch;
            for (int k = 0; k < _curCountEnabled; k++)
            {
                _arrLaserObject[k].UpdatePitch(_curDegLaserPitch);
            }
        }
        if (_curPosInterval != updateInfo.posInterval || flag)
        {
            _curPosInterval = updateInfo.posInterval;
            for (int l = 0; l < _curCountEnabled; l++)
            {
                _arrLaserObject[l].UpdatePositionInterval(_curPosInterval);
            }
        }
        _isBlink = updateInfo.isBlink;
        if (!_isBlink)
        {
            for (int m = 0; m < _curCountEnabled; m++)
            {
                _arrLaserObject[m].enable = true;
            }
        }
        else
        {
            _blinkPeriod = updateInfo.blinkPeroid;
        }
        if (!ignoreHashDic.ContainsKey(_parentNameHash))
        {
            _parentTransform.localPosition = updateInfo.rootPos;
            _parentTransform.localRotation = Quaternion.Euler(updateInfo.rootRot);
            _parentTransform.localScale = updateInfo.rootScale;
        }
        int num = Mathf.Min(_arrLaserObject.Length, updateInfo.localPos.Length);
        for (int n = 0; n < num; n++)
        {
            if (!ignoreHashDic.ContainsKey(_arrLaserObject[n].nameHash))
            {
                _arrLaserObject[n].cachedTransform.localPosition = updateInfo.localPos[n];
                _arrLaserObject[n].cachedTransform.localScale = updateInfo.localScale[n];
                _offsetLocalAngle[n] = Quaternion.Euler(updateInfo.localRot[n]);
            }
        }
    }
    public void Pause()
    {
        if (!_isPause)
        {
            _isPause = true;
        }
    }

    public void Resume()
    {
        if (_isPause)
        {
            _isPause = false;
        }
    }
}
