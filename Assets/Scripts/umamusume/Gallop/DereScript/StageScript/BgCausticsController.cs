using UnityEngine;

namespace Stage
{
    /// <summary>
    /// secretdaybreakで使用
    /// 水面の光の揺らめきを管理する
    /// </summary>
    [ExecuteInEditMode]
    public class BgCausticsController : MonoBehaviour
    {
        public Vector3 _rotVector;

        private Material _material;

        private float _localTime;

        private int _idTextureMappingMatrix;

        private int _idLocalTime;

        private void Start()
        {
            _idLocalTime = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.LocalTime);
            _idTextureMappingMatrix = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.TextureMappingMatrix);
            Renderer component = GetComponent<Renderer>();
            if (component != null)
            {
                _material = component.sharedMaterial;
            }
            UpdateCausticsDirection();
        }

        public void Update()
        {
            _localTime = (_localTime + Time.deltaTime) % 3600f;
            if (_material != null)
            {
                _material.SetFloat(_idLocalTime, _localTime);
            }
        }

        public void UpdateCausticsDirection()
        {
            if (_material != null)
            {
                Matrix4x4 value = Matrix4x4.Rotate(Quaternion.Euler(_rotVector));
                _material.SetMatrix(_idTextureMappingMatrix, value);
            }
        }
    }
}
