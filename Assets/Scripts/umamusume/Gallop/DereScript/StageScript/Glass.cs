using Cutt;
using UnityEngine;

public class Glass : MonoBehaviour
{
    private MeshRenderer _renderer;

    private MaterialPropertyBlock _mtrlPropertyBlock;

    private int _transparentID = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _lightPositionID = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _specularPowerID = SharedShaderParam.INVALID_PROPERTY_ID;

    private int _specularColorID = SharedShaderParam.INVALID_PROPERTY_ID;

    public void Start()
    {
        _transparentID = Shader.PropertyToID("_transparent");
        _lightPositionID = Shader.PropertyToID("_lightPosition");
        _specularColorID = Shader.PropertyToID("_specularColor");
        _specularPowerID = Shader.PropertyToID("_specularPower");
        _renderer = GetComponentInChildren<MeshRenderer>();
        _mtrlPropertyBlock = new MaterialPropertyBlock();
        _renderer.SetPropertyBlock(_mtrlPropertyBlock);
        _renderer.sortingOrder = 3500;
    }

    public void UpdateInfo(ref GlassUpdateInfo updateInfo)
    {
        if (_mtrlPropertyBlock != null && _renderer != null)
        {
            _mtrlPropertyBlock.SetFloat(_transparentID, updateInfo.transparent);
            _mtrlPropertyBlock.SetFloat(_specularPowerID, updateInfo.specularPower);
            _mtrlPropertyBlock.SetVector(_lightPositionID, updateInfo.lightPosition);
            Color specularColor = updateInfo.specularColor;
            specularColor.r *= specularColor.a;
            specularColor.g *= specularColor.a;
            specularColor.b *= specularColor.a;
            _mtrlPropertyBlock.SetColor(_specularColorID, specularColor);
            _renderer.SetPropertyBlock(_mtrlPropertyBlock);
            _renderer.sortingOrder = 3500 + updateInfo.sortOrderOffset;
        }
    }
}
