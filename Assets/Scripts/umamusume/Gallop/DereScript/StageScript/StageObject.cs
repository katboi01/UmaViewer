using UnityEngine;

public class StageObject : MonoBehaviour
{
    [SerializeField]
    protected Renderer[] _renderers;

    protected MaterialPropertyBlock _materialPropertyBlock;

    protected Camera _camera;

    private int _propertyIdCameraFov;

    protected virtual void Awake()
    {
        _propertyIdCameraFov = Shader.PropertyToID("_CameraFov");
        _materialPropertyBlock = new MaterialPropertyBlock();
        _materialPropertyBlock.SetFloat(_propertyIdCameraFov, 60f);
    }

    protected virtual void LateUpdate()
    {
        if (_camera != null && _renderers != null && _renderers.Length != 0)
        {
            _materialPropertyBlock.SetFloat(_propertyIdCameraFov, _camera.fieldOfView);
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].SetPropertyBlock(_materialPropertyBlock);
            }
        }
    }

    public void ResetCamera(Camera camera)
    {
        _camera = camera;
    }
}
