// fakeShadow
using UnityEngine;

public class fakeShadow : MonoBehaviour
{
    [SerializeField]
    private float _maxAlpha = 1f;

    [SerializeField]
    private float _shadowPower = 0.75f;

    [SerializeField]
    private Color _color = new Color(0f, 0f, 0f);

    private Transform _rootTM;

    private Transform _tm;

    private GameObject _rootObject;

    private Transform _locationTransform;

    private Material _shadowMaterial;

    private Renderer _renderer;

    private MaterialPropertyBlock _materialPropertyBlock;

    private Color _prevColor = new Color(0f, 0f, 0f);

    private Vector3 _position = Vector3.zero;

    private float _y;

    private float _alpha;

    private int _shadowIntensityID;

    private int _shadowColorID;

    private bool _isRootObj;

    private bool _isVisible;

    private bool _isInitialized;

    public float maxAlpha
    {
        get
        {
            return _maxAlpha;
        }
        set
        {
            _maxAlpha = value;
        }
    }

    public float shadowPower
    {
        get
        {
            return _shadowPower;
        }
        set
        {
            _shadowPower = value;
        }
    }

    public Color color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
        }
    }

    public GameObject rootObject
    {
        set
        {
            _rootObject = value;
            _rootTM = _rootObject.gameObject.transform;
            _isRootObj = _rootObject != null;
        }
    }

    public Transform locationTransform
    {
        set
        {
            _locationTransform = value;
        }
    }

    public Material shadowMaterial
    {
        set
        {
            _shadowMaterial = value;
        }
    }

    public bool Visible
    {
        get
        {
            return _renderer.enabled;
        }
        set
        {
            if (_renderer != null && _renderer.enabled != value)
            {
                _renderer.enabled = value & base.enabled;
                _isVisible = value & base.enabled;
            }
        }
    }

    public void Initialize()
    {
        if (!(_rootObject == null))
        {
            GameObject gameObject = base.transform.Find("shadow").gameObject;
            _renderer = gameObject.GetComponent<Renderer>();
            if (_shadowMaterial != null)
            {
                _renderer.sharedMaterial = _shadowMaterial;
            }
            _shadowIntensityID = Shader.PropertyToID("_ShadowIntensity");
            _shadowColorID = Shader.PropertyToID("_ShadowColor");
            _materialPropertyBlock = new MaterialPropertyBlock();
            _materialPropertyBlock.SetFloat(_shadowIntensityID, 1f);
            _alpha = -1f;
            _y = _rootObject.transform.position.y;
            _tm = base.transform;
            _isVisible = _renderer.enabled;
            _isInitialized = true;
        }
    }

    private void Start()
    {
        if (!_isInitialized)
        {
            Initialize();
        }
    }

    private void LateUpdate()
    {
        if (!_isRootObj)
        {
            Object.Destroy(base.gameObject);
        }
        else if (_isVisible)
        {
            float y = _y;
            Vector3 position = _rootTM.position;
            _position.x = position.x;
            _position.z = position.z;
            _position.y = _locationTransform.position.y;
            y += _position.y;
            _tm.position = _position;
            _tm.rotation = Quaternion.identity;
            float num = _maxAlpha - Mathf.Clamp(position.y - y, 0f, _maxAlpha);
            num *= _shadowPower;
            int num2 = (int)(num * 64f);
            num = (float)num2 * 0.015625f;
            if (_alpha != num || _color != _prevColor)
            {
                _alpha = num;
                _prevColor = _color;
                _materialPropertyBlock.SetFloat(_shadowIntensityID, _alpha);
                _materialPropertyBlock.SetColor(_shadowColorID, _color);
                _renderer.SetPropertyBlock(_materialPropertyBlock);
                _renderer.sortingOrder = 100 + num2;
            }
        }
    }
}
