using UnityEngine;

public class A2UCamera : MonoBehaviour
{
    private RenderTexture _renderTexture;

    [SerializeField]
    private Material _material;

    private Camera _camera;

    private A2U.Renderer _renderer = new A2U.Renderer();

    public RenderTexture renderTexture => _renderTexture;

    public A2U.Renderer a2uRenderer => _renderer;

    private void OnDestroy()
    {
        Final();
    }

    private void OnEnable()
    {
        _renderer.isEnabled = true;
    }

    private void OnDisable()
    {
        _renderer.isEnabled = false;
    }

    public void Init(int rtWidth, int rtHeight)
    {
        _renderTexture = new RenderTexture(rtWidth, rtHeight, 0, RenderTextureFormat.ARGB32);
        if (!_renderTexture.Create())
        {
            _renderTexture.Release();
            _renderTexture = null;
        }
        Camera component = GetComponent<Camera>();
        component.targetTexture = _renderTexture;
        _camera = component;
        _renderer.Init(_renderTexture, _material);
    }

    public virtual void Final()
    {
        _renderer.Final();
        if (null != _renderTexture)
        {
            _renderTexture.Release();
            _renderTexture = null;
        }
        _material = null;
    }

    public void SetBlendMode(A2U.Blend blendMode)
    {
        switch (blendMode)
        {
            case A2U.Blend.Normal:
            case A2U.Blend.Add:
            case A2U.Blend.Screen:
                _camera.backgroundColor = new Color(1f, 1f, 1f, 0f);
                break;
            case A2U.Blend.Multiply:
                _camera.backgroundColor = new Color(1f, 1f, 1f, 1f);
                break;
            case A2U.Blend.Overlay:
                _camera.backgroundColor = new Color(1f, 1f, 1f, 0f);
                break;
        }
        _renderer.SetBlendMode(blendMode);
    }

    public void SetRenderingOrder(A2U.Order order)
    {
        _renderer.order = order;
    }
}
