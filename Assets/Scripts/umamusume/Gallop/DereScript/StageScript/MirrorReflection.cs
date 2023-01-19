using System;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public class MirrorReflection : MonoBehaviour
{
    public LayerMask _renderLayers = -1;

    public int _textureSize = 0x100;

    public float _clipPlaneOffset = 0.07f;

    [SerializeField]
    private Camera _baseCamera;

    [SerializeField]
    private Camera _reflectionCamera;

    private RenderTexture _reflectionTexture;

    private int _oldTextureSize;

    private float _reflectionRate = 1f;

    private Renderer _renderer;

    private Material[] _materials;

    private bool _isReflectionCamera;

    private void CreateMirrorCamera()
    {
        if (!(_reflectionCamera != null))
        {
            GameObject gameObject = new GameObject("Mirror Camera", typeof(Camera), typeof(BlurOptimized));
            gameObject.transform.parent = base.transform;
            _reflectionCamera = gameObject.GetComponent<Camera>();
            _reflectionCamera.depth = -100f;
            _reflectionCamera.transform.position = base.transform.position;
            _reflectionCamera.transform.rotation = base.transform.rotation;
            _reflectionCamera.gameObject.AddComponent<FlareLayer>();

            Shader shader = ResourcesManager.instance.GetShader("MirrorReplace");
            _reflectionCamera.SetReplacementShader(shader, "Mirror");

            _reflectionCamera.cullingMask = 0x7F40000 & _renderLayers.value;
            _isReflectionCamera = true;
            BlurOptimized component = gameObject.GetComponent<BlurOptimized>();
            if (component != null)
            {
                component.blurShader = Shader.Find("Hidden/FastBlur");
                //component.blurShader = ResourcesManager.instance.GetShader("FastBlur");
                component.downsample = 0;
                component.blurSize = 1f;
                component.blurIterations = 1;
            }
        }
    }

    private void UpdateRenderTexture()
    {
        if (!_reflectionTexture || _oldTextureSize != _textureSize)
        {
            if ((bool)_reflectionTexture)
            {
                _reflectionTexture.Release();
                _reflectionTexture = null;
            }
            _reflectionTexture = new RenderTexture(_textureSize, _textureSize, 16);
            _reflectionTexture.name = "MirrorReflection Texture";
            _reflectionTexture.isPowerOfTwo = true;
            _reflectionTexture.hideFlags = HideFlags.DontSave;
            UpdateMaterials();
            _oldTextureSize = _textureSize;
        }
    }

    private void UpdateMirrorParams()
    {
        if (Director.instance != null)
        {
            _baseCamera = Director.instance.mainCamera;
        }
        else
        {
            _baseCamera = Camera.main;
        }
        if (!(_baseCamera == null) && _isReflectionCamera)
        {
            _reflectionCamera.clearFlags = CameraClearFlags.Color;
            _reflectionCamera.backgroundColor = _baseCamera.backgroundColor;
            _reflectionCamera.farClipPlane = _baseCamera.farClipPlane;
            _reflectionCamera.nearClipPlane = _baseCamera.nearClipPlane;
            _reflectionCamera.orthographic = _baseCamera.orthographic;
            _reflectionCamera.fieldOfView = _baseCamera.fieldOfView;
            _reflectionCamera.aspect = _baseCamera.aspect;
            _reflectionCamera.orthographicSize = _baseCamera.orthographicSize;
            Vector3 position = base.transform.position;
            Vector3 up = base.transform.up;
            float w = 0f - Vector3.Dot(up, position) - _clipPlaneOffset;
            Vector4 plane = new Vector4(up.x, up.y, up.z, w);
            Matrix4x4 reflectionMat = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflectionMat, plane);
            _reflectionCamera.worldToCameraMatrix = _baseCamera.worldToCameraMatrix * reflectionMat;
            Vector4 clipPlane = CalculateCameraSpacePlane(_reflectionCamera, position, up, 1f);
            Matrix4x4 projectionMatrix = _baseCamera.CalculateObliqueMatrix(clipPlane);
            _reflectionCamera.projectionMatrix = projectionMatrix;
            _reflectionCamera.cullingMask = 0x7F40000 & _renderLayers.value;
            _reflectionCamera.targetTexture = _reflectionTexture;
            Vector3 position2 = reflectionMat.MultiplyPoint(_baseCamera.transform.position);
            _reflectionCamera.transform.position = position2;
            Vector3 eulerAngles = _baseCamera.transform.eulerAngles;
            _reflectionCamera.transform.eulerAngles = new Vector3(0f, eulerAngles.y, eulerAngles.z);
        }
    }

    private void UpdateMaterials()
    {
        if (_renderer == null)
        {
            return;
        }
        if (_materials == null)
        {
            _materials = _renderer.sharedMaterials;
        }
        SharedShaderParam instance = SharedShaderParam.instance;
        for (int i = 0; i < _materials.Length; i++)
        {
            Material material = _materials[i];
            if (material.HasProperty(instance.getPropertyID(SharedShaderParam.ShaderProperty.ReflectionTex)))
            {
                material.SetTexture(instance.getPropertyID(SharedShaderParam.ShaderProperty.ReflectionTex), _reflectionTexture);
            }
            if (material.HasProperty(instance.getPropertyID(SharedShaderParam.ShaderProperty.ReflectionRate)))
            {
                material.SetFloat(instance.getPropertyID(SharedShaderParam.ShaderProperty.ReflectionRate), _reflectionRate);
            }
        }
    }

    private void Start()
    {
        StartCoroutine(StartPostpone());
    }
    private IEnumerator StartPostpone()
    {
        yield return null;
        _renderer = GetComponent<Renderer>();
        if (Director.instance != null)
        {
            _baseCamera = Director.instance.mainCamera;
        }
        else
        {
            _baseCamera = Camera.main;
        }
        UpdateMaterials();
        if (!(_renderer == null) && !(_renderer.sharedMaterial == null) && _renderer.enabled && !(_baseCamera == null))
        {
            CreateMirrorCamera();
            UpdateRenderTexture();
            UpdateMirrorParams();
        }
    }

    public void MirrorCameraPreRender(Camera cam)
    {
        if (!(cam != _reflectionCamera))
        {
            UpdateRenderTexture();
            UpdateMirrorParams();
            GL.invertCulling = true;
        }
    }

    public void MirrorCameraPostRender(Camera cam)
    {
        if (!(cam != _reflectionCamera))
        {
            GL.invertCulling = false;
        }
    }

    public void UpdateReflectionRate(float rate)
    {
        _reflectionRate = rate;
        UpdateMaterials();
    }

    private void OnEnable()
    {
        Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(MirrorCameraPreRender));
        Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(MirrorCameraPostRender));
    }

    private void OnDisable()
    {
        Camera.onPreRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPreRender, new Camera.CameraCallback(MirrorCameraPreRender));
        Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(MirrorCameraPostRender));
        if ((bool)_reflectionTexture)
        {
            _reflectionCamera.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(_reflectionTexture);
            _reflectionTexture = null;
        }
    }

    private static float sgn(float a)
    {
        if (a > 0.0f) return 1.0f;
        if (a < 0.0f) return -1.0f;
        return 0.0f;
    }

    private Vector4 CalculateCameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 point = pos + normal * _clipPlaneOffset;
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
