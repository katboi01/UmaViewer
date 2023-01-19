// CameraSharedCache
using UnityEngine;

public class CameraSharedCache : ICameraSharedCache
{
    private const int FRUSTUM_PLANE_COUNT = 6;

    private Camera _camera;

    private Transform _transform;

    private Plane[] _cacheFrustumPlanes = new Plane[FRUSTUM_PLANE_COUNT];

    public Camera camera => _camera;

    public Transform transform => _transform;

    public Plane[] frustumPlanes => _cacheFrustumPlanes;

    public void Fetch()
    {
        GeometryUtility.CalculateFrustumPlanes(_camera, _cacheFrustumPlanes);
    }

    public CameraSharedCache(Camera camera)
    {
        _camera = camera;
        _transform = camera.transform;
    }
}
