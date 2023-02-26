using System;
using UnityEngine;

public class MonitorCamera : MonoBehaviour
{
    public enum RenderCallbackType
    {
        PreCull,
        PostRender
    }

    public delegate void MonitorCameraRenderCallback(MonitorCamera monitorCamera, RenderCallbackType type);

    public const int DEPTH_BUFFER_BIT = 16;

    private static int _sharedTextureNum;

    private static RenderTexture _sharedTexture;

    private Camera _targetCamera;

    private RenderTexture _targetTexture;

    private Transform _cacheTransform;

    private MonitorCameraRenderCallback _onRenderCallback;

    public Camera targetCamera => _targetCamera;

    public RenderTexture targetTexture => _targetTexture;

    public Transform cacheTransform => _cacheTransform;

    public float roll
    {
        set
        {
            _cacheTransform.Rotate(0f, 0f, value);
        }
    }

    public Vector2 GetRTResolution()
    {
        return new Vector2(_targetTexture.width, _targetTexture.height);
    }

    private void Start()
    {
        _targetCamera = GetComponent<Camera>();
        _cacheTransform = base.transform;
        _onRenderCallback = NullMethod;
    }

    private void OnDestroy()
    {
        _targetCamera.targetTexture = null;
        _targetCamera.enabled = false;
        _onRenderCallback = null;
        if (_targetTexture != null)
        {
            _targetTexture.Release();
            _targetTexture = null;
        }
        _sharedTextureNum--;
        if (_sharedTextureNum <= 0 && _sharedTexture != null)
        {
            _sharedTexture.Release();
            _sharedTexture = null;
        }
    }

    public void Initialize(Transform tmParent, int rtWidth, int rtHeight)
    {
        _onRenderCallback = NullMethod;
        if (_sharedTexture == null)
        {
            _sharedTexture = new RenderTexture(rtWidth, rtHeight, 16);
            _sharedTexture.useMipMap = false;
            _sharedTexture.autoGenerateMips = false;
            _sharedTexture.filterMode = FilterMode.Bilinear;
            _sharedTexture.Create();
        }
        _sharedTextureNum++;
        _targetTexture = new RenderTexture(rtWidth, rtHeight, 0);
        _targetTexture.useMipMap = false;
        _targetTexture.autoGenerateMips = false;
        _targetTexture.filterMode = FilterMode.Bilinear;
        _targetTexture.Create();
        _targetCamera.SetTargetBuffers(_sharedTexture.colorBuffer, _sharedTexture.depthBuffer);
        _cacheTransform.SetParent(tmParent);
    }

    public void AddMonitorCameraCallback(MonitorCameraRenderCallback callback)
    {
        _onRenderCallback = (MonitorCameraRenderCallback)Delegate.Combine(_onRenderCallback, callback);
    }

    public void RemoveMonitorCameraCallback(MonitorCameraRenderCallback callback)
    {
        _onRenderCallback = (MonitorCameraRenderCallback)Delegate.Remove(_onRenderCallback, callback);
    }
    private void NullMethod(MonitorCamera monitorCamera, RenderCallbackType type)
    {
    }

    private void OnPreCull()
    {
        _onRenderCallback(this, RenderCallbackType.PreCull);
    }

    private void OnPostRender()
    {
        _onRenderCallback(this, RenderCallbackType.PostRender);
        Graphics.Blit(_sharedTexture, _targetTexture);
    }
}
