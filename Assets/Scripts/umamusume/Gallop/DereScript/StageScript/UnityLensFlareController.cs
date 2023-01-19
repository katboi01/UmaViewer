using Cutt;
using UnityEngine;
using System;

/// <summary>
/// Unityのレンズフレアをもとにしたコントローラー
/// </summary>
public class UnityLensFlareController : MonoBehaviour
{
    private const float POWER_CORRECTION = 2f;

    private int _nameHash;

    private LensFlare _flare;

    private Material _material;

    private Transform _transform;

    private float _underLimitColorPower;

    private float _standardColorPower = 3f;

    private float _brightnessFactor = 1f;

    private Color _colorFactor = Color.white;

    private const float EnableAngleMaxDgree = 180f;

    [SerializeField]
    [Range(0f, 180f)]
    private float _enableAngleDegree;

    private float _enableAngleCos;

    [SerializeField]
    private int _groupIndex = -1;

    private Color _mulColor = Color.white;

    private float _colorPower = 1f;

    private Transform _cachedMainCameraTransform;

    private bool _isInitialized;

    public bool IsGroup(Material material, int groupIndex = -1)
    {
        if (_material == material)
        {
            return _groupIndex == groupIndex;
        }
        return false;
    }

    public void SetWashLightColor(Color color, float power)
    {
        _mulColor = color;
        _colorPower = power;
    }

    /// <summary>
    /// フレアの強さを設定
    /// </summary>
    public void SetLimitColorPower(float standard, float under)
    {
        _underLimitColorPower = under;
        _standardColorPower = standard;
    }

    /// <summary>
    /// 設定を更新する
    /// </summary>
    public void UpdateInfo(ref LensFlareUpdateInfo updateInfo)
    {
        if (!(_transform == null) && !(_flare == null) && _nameHash == updateInfo.data.nameHash && base.enabled)
        {
            _transform.localPosition = updateInfo.offset;
            _flare.fadeSpeed = updateInfo.fadeSpeed;
            _colorFactor = updateInfo.color;
            _brightnessFactor = updateInfo.brightness;
        }
    }
    public void Initialize()
    {
        _flare = GetComponent<LensFlare>();
        if (_flare == null)
        {
            base.enabled = false;
        }
        _transform = base.gameObject.transform;
        _nameHash = FNVHash.Generate(base.name);
        MeshRenderer component = GetComponent<MeshRenderer>();
        if (component != null)
        {
            _material = component.sharedMaterial;
        }
        CalcEnableAngleCos();
        _cachedMainCameraTransform = (Camera.main ? Camera.main.transform : null);
        _isInitialized = true;
    }

    private void Start()
    {
        if (!_isInitialized)
        {
            Initialize();
        }
    }

    private void Update()
    {
        bool flag = _flare.enabled;
        Color color = ((!(_material != null)) ? _colorFactor : (_mulColor * _colorFactor * (_colorPower * POWER_CORRECTION)));
        Transform transform = null;
        transform = ((!(Director.instance != null) || !(Director.instance.mainCamera != null)) ? _cachedMainCameraTransform : Director.instance.mainCameraTransform);
        if (transform != null)
        {
            float num = 0f - Vector3.Dot(transform.forward, _transform.up);
            float num2 = Mathf.Clamp01(num);
            float num3 = _brightnessFactor;
            if (num3 < _standardColorPower && _underLimitColorPower < _standardColorPower)
            {
                num3 -= _underLimitColorPower;
                if (num3 < 0f)
                {
                    num3 = 0f;
                }
                num3 = Mathf.Lerp(t: num3 / (_standardColorPower - _underLimitColorPower), a: 0f, b: _standardColorPower);
            }
            float num4 = num2 * num3;
            if (_enableAngleDegree > 0f && num4 > 0f && num < _enableAngleCos)
            {
                num4 = 0f;
            }
            _flare.brightness = num4;
            flag = num4 > 0f;
        }
        if (Mathf.Epsilon >= color.r && Mathf.Epsilon >= color.g && Mathf.Epsilon >= color.b)
        {
            flag = false;
        }
        if (_flare.enabled != flag)
        {
            _flare.enabled = flag;
        }
        _flare.color = color;
    }

    private void CalcEnableAngleCos()
    {
        float num = Mathf.Clamp01(_enableAngleDegree / EnableAngleMaxDgree);
        _enableAngleCos = Mathf.Cos((float)Math.PI * num);
    }

    public void SetFlare(Flare flare, Camera designationCamera = null)
    {
        _flare.flare = flare;
        if (designationCamera != null)
        {
            _cachedMainCameraTransform = designationCamera.transform;
        }
    }
}

