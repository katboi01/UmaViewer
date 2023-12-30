using UnityEngine;

public class FollowSpotLightController : MonoBehaviour
{
    private static readonly Vector3 _SollowSpotScale = new Vector3(1.7f, 1f, 1.7f);

    private bool _active = true;

    private int _colorPowerPropertyID;

    private Transform _tm;

    private Transform _tmRoot;

    private GameObject _rootObject;

    private Renderer _renderer;

    private MaterialPropertyBlock _materialPropertyBlock;

    private Transform _locationTransform;

    public bool active
    {
        get
        {
            return _active;
        }
        set
        {
            _active = value;
        }
    }

    public GameObject rootObject
    {
        get
        {
            return _rootObject;
        }
        set
        {
            _rootObject = value;
            if (_rootObject != null)
            {
                _tmRoot = _rootObject.transform;
            }
        }
    }

    public MaterialPropertyBlock materialPropertyBlock => _materialPropertyBlock;

    public Transform locationTransform
    {
        set
        {
            _locationTransform = value;
        }
    }

    private void Start()
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
        _renderer = GetComponentInChildren<Renderer>();
        _renderer.transform.localScale = _SollowSpotScale;
        _renderer.sortingOrder = 110;
        _colorPowerPropertyID = Shader.PropertyToID("_ColorPower");
        _materialPropertyBlock.SetColor("_MulColor0", Color.white);
        _materialPropertyBlock.SetFloat(_colorPowerPropertyID, 0f);
        _tm = base.transform;
    }

    private void Update()
    {
        if (_rootObject == null)
        {
            return;
        }
        if (_locationTransform != null)
        {
            Vector3 position = _tmRoot.position;
            position.y = _locationTransform.position.y;
            _tm.position = position;
        }
        if (!(_renderer != null))
        {
            return;
        }
        if (_active)
        {
            if (_materialPropertyBlock.GetFloat(_colorPowerPropertyID) == 0f)
            {
                if (_renderer.enabled)
                {
                    _renderer.enabled = false;
                }
                return;
            }
            if (!_renderer.enabled)
            {
                _renderer.enabled = true;
            }
            _renderer.SetPropertyBlock(_materialPropertyBlock);
        }
        else
        {
            _renderer.enabled = false;
        }
    }
}
