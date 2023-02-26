using UnityEngine;

namespace Stage
{
    public class BgSpecialTime : MonoBehaviour
    {
        [Header("スピード")]
        [SerializeField]
        [Range(0f, 5f)]
        private float _timeSpeed = 1f;

        private int _idLocalTime = -1;

        private Material _material;

        public void Start()
        {
            _idLocalTime = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.LocalTime);
            _material = GetComponent<Renderer>().sharedMaterial;
        }

        public void Update()
        {
            float value = (float)((double)Time.timeSinceLevelLoad / 20.0) * _timeSpeed;
            _material.SetFloat(_idLocalTime, value);
        }
    }
}
