using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stage
{

    [ExecuteInEditMode]
    public class BgSeaSurface : MonoBehaviour
    {
        public int textureSize = 256;

        public float clipPlaneOffset = 0.07f;

        public LayerMask reflectLayers = -1;

        public LayerMask refractLayers = -1;

        public float waterPlaneOffsetY = -0f;

        public Color _ClearColor;

        private Dictionary<Camera, Camera> _reflectionCameras = new Dictionary<Camera, Camera>();

        private Dictionary<Camera, Camera> _refractionCameras = new Dictionary<Camera, Camera>();

        private RenderTexture _reflectionTexture;

        private RenderTexture _refractionTexture;

        private int _oldReflectionTextureSize;

        private int _oldRefractionTextureSize;

        private bool _useReflection;

        private bool _useRefraction;

        private bool _useSunPosition;

        private bool _useSunMirrorPosition;

        private float _localTime;

        private int _idReflectionTex;

        private int _idRefractionTex;

        private int _idCameraProjector;

        private int _idLocalTime;

        private int _idWaveOffset;

        private int _idSunPosition;

        private int _idSunMirrorPosition;

        private Vector3 _normal;

        private static bool _insideWater;

        private Material _material;

        private Vector4 _waveSpeed;

        private Vector4 _waveScale4;

        private Matrix4x4 _reflectionMatrix = Matrix4x4.zero;

        public GameObject _sunObject;

        private Vector3 GetWaterPosition()
        {
            Vector3 position = base.transform.position;
            position.y += waterPlaneOffsetY;
            return position;
        }

        public void Start()
        {
            Renderer component = GetComponent<Renderer>();
            if ((bool)component && component.sharedMaterial != null)
            {
                _material = component.sharedMaterial;
                Shader shader = component.sharedMaterial.shader;
                if (shader != null)
                {
                    switch (shader.name)
                    {
                        case "Cygames/3DLive/Stage/Special/StageSeaLite":
                        case "Cygames/3DLive/Stage/Special/StageSeaLiteTransparent":
                            _useReflection = true;
                            _useSunPosition = false;
                            _useSunMirrorPosition = false;
                            _normal = base.transform.up;
                            break;
                        case "Cygames/3DLive/Stage/Special/StageSeaSunrise":
                        case "Cygames/3DLive/Stage/Special/StageSeaSunriseTransparent":
                            _useReflection = true;
                            _useSunPosition = false;
                            _useSunMirrorPosition = true;
                            _normal = base.transform.up;
                            break;
                        case "Cygames/3DLive/Stage/Special/StageUnderSea":
                            _useReflection = false;
                            _useSunPosition = true;
                            _useSunMirrorPosition = false;
                            _normal = -base.transform.up;
                            break;
                    }
                }
            }
            UpdateShaderParams();
            _idReflectionTex = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.ReflectionTex);
            _idRefractionTex = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.RefractionTex);
            _idCameraProjector = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.CameraProjector);
            _idLocalTime = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.LocalTime);
            _idWaveOffset = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.WaveOffset);
            _idSunPosition = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.SunPosition);
            _idSunMirrorPosition = SharedShaderParam.instance.getPropertyID(SharedShaderParam.ShaderProperty.SunMirrorPosition);
        }

        public void OnWillRenderObject()
        {
            if (!base.enabled || !GetComponent<Renderer>() || !GetComponent<Renderer>().sharedMaterial || !GetComponent<Renderer>().enabled)
            {
                return;
            }
            Camera current = Camera.current;
            if (!(current == null) && !_insideWater)
            {
                _insideWater = true;
                CreateWaterObjects(current, out var reflectionCamera, out var refractionCamera);
                UpdateCameraModes(current, reflectionCamera);
                UpdateCameraModes(current, refractionCamera);
                if ((bool)reflectionCamera)
                {
                    reflectionCamera.backgroundColor = _ClearColor;
                }
                if ((bool)refractionCamera)
                {
                    refractionCamera.backgroundColor = _ClearColor;
                }
                Material sharedMaterial = GetComponent<Renderer>().sharedMaterial;
                float num = 1f / Mathf.Tan(current.fieldOfView * (float)Math.PI / 180f / 2f);
                float value = num / current.aspect;
                Vector3 waterPosition = GetWaterPosition();
                if (_useReflection)
                {
                    Vector3 position = current.transform.position;
                    Vector3 position2 = _reflectionMatrix.MultiplyPoint(position);
                    reflectionCamera.worldToCameraMatrix = current.worldToCameraMatrix * _reflectionMatrix;
                    Vector4 clipPlane = CameraSpacePlane(reflectionCamera, waterPosition, _normal, 1f);
                    Matrix4x4 projectionMatrix = current.CalculateObliqueMatrix(clipPlane);
                    projectionMatrix[0, 0] = value;
                    projectionMatrix[1, 1] = num;
                    reflectionCamera.projectionMatrix = projectionMatrix;
                    reflectionCamera.cullingMatrix = current.projectionMatrix * current.worldToCameraMatrix;
                    reflectionCamera.cullingMask = -17 & reflectLayers.value;
                    reflectionCamera.targetTexture = _reflectionTexture;
                    bool invertCulling = GL.invertCulling;
                    GL.invertCulling = !invertCulling;
                    reflectionCamera.transform.position = position2;
                    Vector3 eulerAngles = current.transform.eulerAngles;
                    reflectionCamera.transform.eulerAngles = new Vector3(0f - eulerAngles.x, eulerAngles.y, eulerAngles.z);
                    reflectionCamera.Render();
                    reflectionCamera.transform.position = position;
                    GL.invertCulling = invertCulling;
                    sharedMaterial.SetTexture(_idReflectionTex, _reflectionTexture);
                }
                if (_useRefraction)
                {
                    refractionCamera.worldToCameraMatrix = current.worldToCameraMatrix;
                    Vector4 clipPlane2 = CameraSpacePlane(refractionCamera, waterPosition, _normal, -1f);
                    refractionCamera.projectionMatrix = current.CalculateObliqueMatrix(clipPlane2);
                    refractionCamera.cullingMatrix = current.projectionMatrix * current.worldToCameraMatrix;
                    refractionCamera.cullingMask = -17 & refractLayers.value;
                    refractionCamera.targetTexture = _refractionTexture;
                    refractionCamera.transform.SetPositionAndRotation(current.transform.position, current.transform.rotation);
                    refractionCamera.Render();
                    sharedMaterial.SetTexture(_idRefractionTex, _refractionTexture);
                }
                Matrix4x4 identity = Matrix4x4.identity;
                identity = current.worldToCameraMatrix;
                sharedMaterial.SetMatrix(_idCameraProjector, identity);
                _insideWater = false;
            }
        }

        private void OnDisable()
        {
            if ((bool)_reflectionTexture)
            {
                UnityEngine.Object.DestroyImmediate(_reflectionTexture);
                _reflectionTexture = null;
            }
            if ((bool)_refractionTexture)
            {
                UnityEngine.Object.DestroyImmediate(_refractionTexture);
                _refractionTexture = null;
            }
            foreach (KeyValuePair<Camera, Camera> reflectionCamera in _reflectionCameras)
            {
                UnityEngine.Object.DestroyImmediate(reflectionCamera.Value.gameObject);
            }
            _reflectionCameras.Clear();
            foreach (KeyValuePair<Camera, Camera> refractionCamera in _refractionCameras)
            {
                UnityEngine.Object.DestroyImmediate(refractionCamera.Value.gameObject);
            }
            _refractionCameras.Clear();
        }

        public void Update()
        {
            _localTime = (_localTime + Time.deltaTime) % 3600f;
            if (!(_material != null))
            {
                return;
            }
            _material.SetFloat(_idLocalTime, _localTime);
            float num = _localTime / 20f;
            Vector4 value = default(Vector4);
            value.x = _waveSpeed.x * _waveScale4.x * num % 1f;
            value.y = _waveSpeed.y * _waveScale4.y * num % 1f;
            value.z = _waveSpeed.z * _waveScale4.z * num % 1f;
            value.w = _waveSpeed.w * _waveScale4.w * num % 1f;
            _material.SetVector(_idWaveOffset, value);
            if (_sunObject != null)
            {
                if (_useSunPosition)
                {
                    Vector3 position = _sunObject.transform.position;
                    _material.SetVector(_idSunPosition, position);
                }
                if (_useSunMirrorPosition)
                {
                    Vector3 vector = _reflectionMatrix.MultiplyPoint(_sunObject.transform.position);
                    _material.SetVector(_idSunMirrorPosition, vector);
                }
            }
        }

        private void UpdateShaderParams()
        {
            if (!(GetComponent<Renderer>() == null))
            {
                Material sharedMaterial = GetComponent<Renderer>().sharedMaterial;
                if (!(sharedMaterial == null))
                {
                    _waveSpeed = sharedMaterial.GetVector("WaveSpeed");
                    float @float = sharedMaterial.GetFloat("_WaveScale");
                    _waveScale4 = new Vector4(@float, @float, @float * 0.4f, @float * 0.45f);
                    sharedMaterial.SetVector("_WaveScale4", _waveScale4);
                    Vector3 waterPosition = GetWaterPosition();
                    _reflectionMatrix = Matrix4x4.zero;
                    float w = 0f - Vector3.Dot(_normal, waterPosition) - clipPlaneOffset;
                    CalculateReflectionMatrix(plane: new Vector4(_normal.x, _normal.y, _normal.z, w), reflectionMat: ref _reflectionMatrix);
                }
            }
        }

        private void UpdateCameraModes(Camera src, Camera dest)
        {
            if (dest == null)
            {
                return;
            }
            dest.clearFlags = src.clearFlags;
            dest.backgroundColor = src.backgroundColor;
            if (src.clearFlags == CameraClearFlags.Skybox)
            {
                Skybox component = src.GetComponent<Skybox>();
                Skybox component2 = dest.GetComponent<Skybox>();
                if (!component || !component.material)
                {
                    component2.enabled = false;
                }
                else
                {
                    component2.enabled = true;
                    component2.material = component.material;
                }
            }
            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.orthographic = src.orthographic;
            dest.fieldOfView = src.fieldOfView;
            dest.aspect = src.aspect;
            dest.orthographicSize = src.orthographicSize;
        }

        private void CreateWaterObjects(Camera currentCamera, out Camera reflectionCamera, out Camera refractionCamera)
        {
            reflectionCamera = null;
            refractionCamera = null;
            if (_useReflection)
            {
                if (!_reflectionTexture || _oldReflectionTextureSize != textureSize)
                {
                    if ((bool)_reflectionTexture)
                    {
                        UnityEngine.Object.DestroyImmediate(_reflectionTexture);
                    }
                    _reflectionTexture = new RenderTexture(textureSize, textureSize, 16);
                    _reflectionTexture.name = "__WaterReflection" + GetInstanceID();
                    _reflectionTexture.isPowerOfTwo = true;
                    _reflectionTexture.hideFlags = HideFlags.DontSave;
                    _oldReflectionTextureSize = textureSize;
                }
                _reflectionCameras.TryGetValue(currentCamera, out reflectionCamera);
                if (!reflectionCamera)
                {
                    GameObject gameObject = new GameObject("Water Refl Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
                    reflectionCamera = gameObject.GetComponent<Camera>();
                    reflectionCamera.enabled = false;
                    reflectionCamera.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
                    reflectionCamera.gameObject.AddComponent<FlareLayer>();
                    gameObject.hideFlags = HideFlags.HideAndDontSave;
                    _reflectionCameras[currentCamera] = reflectionCamera;
                }
            }
            if (!_useRefraction)
            {
                return;
            }
            if (!_refractionTexture || _oldRefractionTextureSize != textureSize)
            {
                if ((bool)_refractionTexture)
                {
                    UnityEngine.Object.DestroyImmediate(_refractionTexture);
                }
                _refractionTexture = new RenderTexture(textureSize, textureSize, 16);
                _refractionTexture.name = "__WaterRefraction" + GetInstanceID();
                _refractionTexture.isPowerOfTwo = true;
                _refractionTexture.hideFlags = HideFlags.DontSave;
                _oldRefractionTextureSize = textureSize;
            }
            _refractionCameras.TryGetValue(currentCamera, out refractionCamera);
            if (!refractionCamera)
            {
                GameObject gameObject2 = new GameObject("Water Refr Camera id" + GetInstanceID() + " for " + currentCamera.GetInstanceID(), typeof(Camera), typeof(Skybox));
                refractionCamera = gameObject2.GetComponent<Camera>();
                refractionCamera.enabled = false;
                refractionCamera.transform.SetPositionAndRotation(base.transform.position, base.transform.rotation);
                refractionCamera.gameObject.AddComponent<FlareLayer>();
                gameObject2.hideFlags = HideFlags.HideAndDontSave;
                _refractionCameras[currentCamera] = refractionCamera;
            }
        }

        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 point = pos + normal * clipPlaneOffset;
            Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
            Vector3 lhs = worldToCameraMatrix.MultiplyPoint(point);
            Vector3 rhs = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(rhs.x, rhs.y, rhs.z, 0f - Vector3.Dot(lhs, rhs));
        }

        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = 1f - 2f * plane[0] * plane[0];
            reflectionMat.m01 = -2f * plane[0] * plane[1];
            reflectionMat.m02 = -2f * plane[0] * plane[2];
            reflectionMat.m03 = -2f * plane[3] * plane[0];
            reflectionMat.m10 = -2f * plane[1] * plane[0];
            reflectionMat.m11 = 1f - 2f * plane[1] * plane[1];
            reflectionMat.m12 = -2f * plane[1] * plane[2];
            reflectionMat.m13 = -2f * plane[3] * plane[1];
            reflectionMat.m20 = -2f * plane[2] * plane[0];
            reflectionMat.m21 = -2f * plane[2] * plane[1];
            reflectionMat.m22 = 1f - 2f * plane[2] * plane[2];
            reflectionMat.m23 = -2f * plane[3] * plane[2];
            reflectionMat.m30 = 0f;
            reflectionMat.m31 = 0f;
            reflectionMat.m32 = 0f;
            reflectionMat.m33 = 1f;
        }
    }
}
