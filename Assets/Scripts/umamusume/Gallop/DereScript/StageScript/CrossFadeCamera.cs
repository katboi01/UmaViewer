using UnityEngine;

public class CrossFadeCamera : MonoBehaviour
{
    private RenderTexture _renderTexture;

    private Camera _camera;

    private PostEffectLive3D _postEffectLive3D;

    public RenderTexture ColorTexture => _renderTexture;

    public PostEffectLive3D postEffectLive3D => _postEffectLive3D;

    public Camera GetCamera()
    {
        return _camera;
    }

    public void SetEnable(bool b)
    {
        if (_camera.enabled != b)
        {
            _camera.enabled = b;
        }
    }

    public bool Initialize()
    {
        Release();
        Camera component = GetComponent<Camera>();
        int width = (int)(Screen.width * Director.instance.mainCameraRect.width);
        int height = (int)(Screen.height * Director.instance.mainCameraRect.height);
        _renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        _renderTexture.name = "CrossFadeCamera.colorBuffer";
        if (!_renderTexture.Create())
        {
            return false;
        }
        _camera = component;
        _camera.clearFlags = CameraClearFlags.Color;
        _camera.allowHDR = false;
        return true;
    }

    public void Setup(RenderTexture colorTexture, RenderTexture depthTexture, RenderTexture monitorTexture)
    {
        if (!(_postEffectLive3D != null))
        {
            _postEffectLive3D = _camera.gameObject.AddComponent<PostEffectLive3D>();
            _postEffectLive3D.SetCameraRenderTarget(colorTexture, depthTexture, _renderTexture, monitorTexture);
        }
    }

    public void Release()
    {
        if (_camera != null)
        {
            _camera.enabled = false;
        }
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            _renderTexture = null;
        }
        if (_postEffectLive3D != null)
        {
            Object.Destroy(_postEffectLive3D);
            _postEffectLive3D = null;
        }
        _camera = null;
    }

    public void OnPostRender()
    {
        if (_postEffectLive3D != null)
        {
            _postEffectLive3D.UpdateRuntimePostEffectInfomation();
            _postEffectLive3D.Work();
        }
    }

    private void OnDestroy()
    {
        Release();
    }
}
