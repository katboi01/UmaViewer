using UnityEngine;
using UnityEngine.Rendering;

public class ModelAfterImage : AfterImage
{
    [SerializeField]
    [Tooltip("残像対象のMesh")]
    protected Mesh _mesh;

    private int _blurColorPropertyIndex = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _modelBlurTransformPropertyIndex = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _lightPosPropertyIndex = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _specularColorPropertyIndex = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _specularPowerPropertyIndex = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _luminousColorPropertyIndex = SharedShaderParam.INVALID_PROPERTY_ID;

    private Matrix4x4[] _matrixBuffer;

    private Material[] _materialBuffer;

    public void Start()
    {
        if (Check())
        {
            _blurColorPropertyIndex = Shader.PropertyToID("_BlurColor");
            _modelBlurTransformPropertyIndex = Shader.PropertyToID("_ModelBlurTransform");
            _lightPosPropertyIndex = Shader.PropertyToID("_lightPosition");
            _specularPowerPropertyIndex = Shader.PropertyToID("_specularPower");
            _specularColorPropertyIndex = Shader.PropertyToID("_specularColor");
            _luminousColorPropertyIndex = Shader.PropertyToID("_luminousColor");
            Create();
        }
    }

    public void LateUpdate()
    {
        UpdateModel();
    }

    public void OnEnable()
    {
        Create();
        Attach(bSwitch: true);
        Reset();
    }

    public void OnDisable()
    {
        Attach(bSwitch: false);
    }

    private void UpdateModel()
    {
        if (_matrixBuffer == null || _materialBuffer == null || _pause)
        {
            return;
        }
        _lastIndex = _nowIndex;
        _nowIndex = (_lastIndex + 1) % _dimention;
        if (_cntAfterImage < _dimention)
        {
            _cntAfterImage++;
        }
        _matrixBuffer[_lastIndex] = Matrix4x4.TRS(_targetTransform.position, _targetTransform.rotation, Vector3.one * _scale);
        for (int i = 0; i < _cntAfterImage; i++)
        {
            Color color = Color.Lerp(_rootColor, _tipColor, (float)i / (float)_dimention);
            color *= _color;
            int num = (_lastIndex - i + _dimention) % _dimention;
            _materialBuffer[i].SetVector(_blurColorPropertyIndex, color);
            if (base.lighting)
            {
                _materialBuffer[i].SetVector(_lightPosPropertyIndex, base.lightPosition);
                _materialBuffer[i].SetColor(_specularColorPropertyIndex, base.specularColor);
                _materialBuffer[i].SetFloat(_specularPowerPropertyIndex, base.specularPower);
                _materialBuffer[i].SetColor(_luminousColorPropertyIndex, base.luminousColor);
            }
            _materialBuffer[i].SetMatrix(_modelBlurTransformPropertyIndex, _matrixBuffer[num]);
        }
        for (int j = _cntAfterImage; j < _dimention; j++)
        {
            _materialBuffer[j].SetMatrix(_modelBlurTransformPropertyIndex, Matrix4x4.zero);
        }
    }

    public override void Create()
    {
        if (_commandBuffer == null)
        {
            _afterImageType = eShaderPass.Model;
            _matrixBuffer = new Matrix4x4[_dimention];
            _materialBuffer = new Material[_dimention];
            CommandBuffer commandBuffer = new CommandBuffer();
            int shaderPass = (base.lighting ? 1 : 0);
            for (int num = _dimention - 1; num >= 0; num--)
            {
                _materialBuffer[num] = new Material(_targetMaterial);
                commandBuffer.DrawMesh(_mesh, Matrix4x4.identity, _materialBuffer[num], 0, shaderPass);
            }
            _commandBuffer = commandBuffer;
        }
    }

    public override void Destory()
    {
        Attach(bSwitch: false);
        if (_commandBuffer != null)
        {
            _commandBuffer.Clear();
            _commandBuffer.Release();
            _commandBuffer = null;
        }
    }
}
