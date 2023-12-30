using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// DirectorでAddCommpornentされる
/// </summary>
public class FinalizeCamera : MonoBehaviour
{
    public class Param
    {
        public bool useDimmer;

        public bool usePostEffect;

        public bool useMonitor;
    }

    private const CameraEvent FINALIZE_CAMERA_EVENT = CameraEvent.AfterImageEffects;

    private const string BUFFER_NAME = "FinalizeCamera";

    private const string SHADER_NAME = "FinalizeCamera";

    private const float DIMMER_RATE = 0.65f;

    private Director.RenderTarget _renderTarget;

    private Param _param;

    private bool _attachedCommandBuffer;

    private Camera _thisCamera;

    private CommandBuffer _commandBuffer;

    private Material _material;

    public Camera thisCamera => _thisCamera;

    public Rect rect
    {
        set
        {
            if (_thisCamera != null)
            {
                _thisCamera.rect = value;
            }
        }
    }
    public bool useDimmer => _param.useDimmer;

    public IEnumerator Initialize(Director.RenderTarget renderTarget, Param param, float dimmerAlpha)
    {
        _renderTarget = renderTarget;
        _param = param;
        _thisCamera = base.gameObject.AddComponent<Camera>();
        yield return null;
        _thisCamera.clearFlags = CameraClearFlags.Nothing;
        _thisCamera.cullingMask = 0;
        _thisCamera.allowMSAA = false;
        _thisCamera.allowHDR = false;
        _thisCamera.allowDynamicResolution = false;
        _thisCamera.useOcclusionCulling = false;
        _thisCamera.depth = -1f;
        if (_material == null)
        {
            Shader shader = ResourcesManager.instance.GetShader(SHADER_NAME);
            //Shader shader = Shader.Find(SHADER_NAME);
            _material = new Material(shader);
            dimmerAlpha = ((dimmerAlpha == 0f) ? DIMMER_RATE : (1f - dimmerAlpha));
            _material.SetTexture("_MainTex", _renderTarget.result);
            _material.SetFloat("_dimmerRate", dimmerAlpha);
        }
        if (!_attachedCommandBuffer && _commandBuffer == null)
        {
            _commandBuffer = new CommandBuffer();
            _commandBuffer.name = BUFFER_NAME;
            BuildCommandBuffer();
            _thisCamera.AddCommandBuffer(FINALIZE_CAMERA_EVENT, _commandBuffer);
            _attachedCommandBuffer = true;
        }
    }

    private void BuildCommandBuffer()
    {
        if (_commandBuffer != null)
        {
            _commandBuffer.Clear();
            if (!_param.usePostEffect && _param.useMonitor)
            {
                _commandBuffer.Blit(_renderTarget.color, _renderTarget.monitor);
            }
            _commandBuffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            _commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, Color.black);
            if (_param.useDimmer && _material != null)
            {
                _commandBuffer.Blit(_renderTarget.result, BuiltinRenderTextureType.CurrentActive, _material);
            }
            else
            {
                _commandBuffer.Blit(_renderTarget.result, BuiltinRenderTextureType.CurrentActive);
            }
        }
    }

    public void SetDimmer(bool bSwitch)
    {
        if (_param.useDimmer != bSwitch)
        {
            _param.useDimmer = bSwitch;
            BuildCommandBuffer();
        }
    }

    public void OnEnable()
    {
        if (!_attachedCommandBuffer && _commandBuffer != null)
        {
            _thisCamera.AddCommandBuffer(FINALIZE_CAMERA_EVENT, _commandBuffer);
            _attachedCommandBuffer = true;
        }
    }

    public void OnPreRender()
    {
        Director instance = Director.instance;
        if (_param.usePostEffect)
        {
            foreach (PostEffectLive3D imageEffectLive3d in instance.imageEffectLive3dList)
            {
                if (imageEffectLive3d.isActiveAndEnabled)
                {
                    imageEffectLive3d.UpdateRuntimePostEffectInfomation();
                    imageEffectLive3d.Work();
                    break;
                }
            }
        }
        ScreenFade[] screenFadeArray = instance.screenFadeArray;
        if (screenFadeArray == null)
        {
            return;
        }
        ScreenFade[] array = screenFadeArray;
        foreach (ScreenFade screenFade in array)
        {
            if (screenFade != null && screenFade.isActiveAndEnabled)
            {
                screenFade.Work();
            }
        }
    }

    public void OnDisable()
    {
        if (_attachedCommandBuffer && _commandBuffer != null)
        {
            _thisCamera.RemoveCommandBuffer(FINALIZE_CAMERA_EVENT, _commandBuffer);
            _attachedCommandBuffer = false;
        }
    }

    public void OnDestroy()
    {
        if (_commandBuffer != null)
        {
            _thisCamera.RemoveCommandBuffer(FINALIZE_CAMERA_EVENT, _commandBuffer);
            _attachedCommandBuffer = false;
            _commandBuffer.Release();
            _commandBuffer = null;
        }
        if (_material != null)
        {
            UnityEngine.Object.DestroyImmediate(_material);
            _material = null;
        }
        _thisCamera = null;
        _renderTarget = null;
        _param = null;
    }
}