using UnityEngine;
using UnityEngine.Rendering;

public abstract class AfterImage : MonoBehaviour
{
    protected enum eShaderPass
    {
        Model,
        ModelLight,
        TrailColoredTex,
        TrailShimmer
    }

    protected Camera _currentTargetCamera;

    [SerializeField]
    [Tooltip("残像の計算元となるノード")]
    protected Transform _targetTransform;

    [SerializeField]
    [Tooltip("残像の描画に利用されるマテリアル")]
    protected Material _targetMaterial;

    [SerializeField]
    [Tooltip("残像の数")]
    protected int _dimention = 8;

    [SerializeField]
    [Tooltip("残像モデルのスケール")]
    protected float _scale = 1f;

    protected Color _color = Color.white;

    protected Color _rootColor = Color.white;

    protected Color _tipColor = Color.blue;

    protected CameraEvent _cameraEvent = CameraEvent.AfterForwardAlpha;

    [SerializeField]
    [Tooltip("Lighting")]
    protected bool _lighting;

    protected Vector3 _lightPosition = Vector3.zero;

    protected float _specularPower = 5f;

    protected Color _specularColor = Color.white;

    protected Color _luminousColor = new Color(1f,1f,1f,0f);

    protected bool _pause;

    protected eShaderPass _afterImageType;

    protected bool _isAttachCommandBuffer;

    protected CommandBuffer _commandBuffer;

    protected int _cntAfterImage;

    protected int _lastIndex;

    protected int _nowIndex;

    public Camera currentTargetCamera
    {
        get
        {
            return _currentTargetCamera;
        }

        set
        {
            if(_currentTargetCamera != value)
            {
                if (_isAttachCommandBuffer)
                {
                    Attach(bSwitch: false);
                    _currentTargetCamera = value;
                    Attach(bSwitch: true);
                }
                else
                {
                    _currentTargetCamera = value;
                }
            }

        }
    }
    public Transform targetTransfrom => targetTransfrom;

    public Material targetMaterial => _targetMaterial;

    public int dimention
    {
        get
        {
            return _dimention;
        }
        set
        {
            dimention = value;
        }
    }

    public float scale => _scale;

    public Color color
    {
        get
        {
            return _color;
        }
        set
        {
            _color = value;
            _color.a = 1f;
        }
    }

    public Color rootColor
    {
        get
        {
            return _rootColor;
        }
        set
        {
            _rootColor = value;
        }
    }

    public Color tipColor
    {
        get
        {
            return _tipColor;
        }
        set
        {
            _tipColor = value;
        }
    }

    public CameraEvent cameraEvent
    {
        get
        {
            return _cameraEvent;
        }
        set
        {
            _cameraEvent = value;
        }
    }

    public bool lighting => _lighting;

    public Vector3 lightPosition
    {
        get
        {
            return _lightPosition;
        }
        set
        {
            _lightPosition = value;
        }
    }

    public float specularPower
    {
        get
        {
            return _specularPower;
        }
        set
        {
            _specularPower = value;
        }
    }

    public Color specularColor
    {
        get
        {
            return _specularColor;
        }
        set
        {
            _specularColor = value;
        }
    }

    public Color luminousColor
    {
        get
        {
            return _luminousColor;
        }
        set
        {
            _luminousColor = value;
        }
    }

    public bool pause
    {
        get
        {
            return _pause;
        }
        set
        {
            _pause = value;
        }
    }

    public void Reset()
    {
        _lastIndex = 0;
        _nowIndex = 0;
        _cntAfterImage = 0;
    }

    public bool Check()
    {
        if (_currentTargetCamera == null)
        {
            return false;
        }
        if (_targetMaterial == null)
        {
            return false;
        }
        if (_targetTransform == null)
        {
            return false;
        }
        if (_dimention < 1)
        {
            return false;
        }
        return true;
    }

    public void Attach(bool bSwitch)
    {
        if (!(_currentTargetCamera == null) && _commandBuffer != null)
        {
            if (bSwitch && !_isAttachCommandBuffer)
            {
                _currentTargetCamera.AddCommandBuffer(_cameraEvent, _commandBuffer);
                _isAttachCommandBuffer = true;
            }
            else if (!bSwitch && _isAttachCommandBuffer)
            {
                _currentTargetCamera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
                _isAttachCommandBuffer = false;
            }
        }
    }

    public abstract void Create();

    public abstract void Destory();
}
