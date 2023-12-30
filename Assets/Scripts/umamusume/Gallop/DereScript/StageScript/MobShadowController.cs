using UnityEngine;

public class MobShadowController : MonoBehaviour
{
    [SerializeField]
    private bool _isControlEnabled;

    private Renderer[] _renderers;

    private Color _mobColor = Color.white;

    private Color _ambientColor = Color.white;

    private const string CONTROL_SHADER = "MobShadow_control";

    private Material _material;

    private Matrix4x4[] _groupMatrices;

    public void SetOffsetPot(Vector3 offset)
    {
        base.transform.position = base.transform.position + offset;
    }

    public void SetMobColor(Color color)
    {
        _mobColor = color;
        _UpdateShaderParamColor();
    }

    public void SetAmbientColor(Color color)
    {
        _ambientColor = color;
        _UpdateShaderParamColor();
    }
    public void UpdateMaterial()
    {
        if (_material != null)
        {
            int propertyID = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.GroupMatrices);
            _material.SetMatrixArray(propertyID, _groupMatrices);
        }
    }

    public void SetGroupMatrix(int groupIndex, ref Matrix4x4 matrix)
    {
        if (_groupMatrices != null)
        {
            _groupMatrices[groupIndex] = matrix;
        }
    }

    private void OnDestroy()
    {
        if (_material != null)
        {
            Object.DestroyImmediate(_material);
            _material = null;
        }
    }

    private void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>();

        if (_isControlEnabled && _renderers != null)
        {
            _groupMatrices = CreateMobCyalumeGroupMatrices();
            _material = Object.Instantiate(_renderers[0].sharedMaterial);

            Shader shader = ResourcesManager.instance.GetShader(CONTROL_SHADER);
            _material.shader = shader;

            int propertyID = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.GroupMatrices);
            _material.SetMatrixArray(propertyID, _groupMatrices);
            SetMaterial(_material);
        }

        //設定の読み込み
        int save = SaveManager.GetInt("Mob");
        if (save != 1)
        {
            foreach (var tmp in _renderers)
            {
                tmp.enabled = false;
            }
        }
    }

    private static Matrix4x4[] CreateMobCyalumeGroupMatrices()
    {
        Matrix4x4[] array = new Matrix4x4[11];
        int i = 0;
        for (int num = 11; i < num; i++)
        {
            array[i] = Matrix4x4.identity;
        }
        return array;
    }

    private void _UpdateShaderParamColor()
    {
        if (_renderers == null)
        {
            return;
        }
        int propertyID = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.MobColor);
        Color color = _mobColor * _ambientColor;
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (!(_renderers[i].sharedMaterial == null))
            {
                _renderers[i].sharedMaterial.SetVector(propertyID, color);
            }
        }
    }

    private void SetMaterial(Material material)
    {
        Renderer[] renderers = _renderers;
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = material;
        }
    }
}
