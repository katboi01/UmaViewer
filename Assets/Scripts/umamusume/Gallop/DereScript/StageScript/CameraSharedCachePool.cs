// CameraSharedCachePool
using UnityEngine;

public class CameraSharedCachePool
{
    private class Dummy : ICameraSharedCache
    {
        private const int FRUSTUM_PLANE_COUNT = 6;

        private Camera _camera;

        private Transform _transform;

        private Plane[] _cacheFrustumPlanes = new Plane[FRUSTUM_PLANE_COUNT];

        public Camera camera => _camera;

        public Transform transform => _transform;

        public Plane[] frustumPlanes
        {
            get
            {
                GeometryUtility.CalculateFrustumPlanes(_camera, _cacheFrustumPlanes);
                return _cacheFrustumPlanes;
            }
        }

        public Dummy(Camera camera)
        {
            _camera = camera;
            _transform = camera.transform;
        }
    }

    private static CameraSharedCachePool _instance;

    private const int MAX_CAMERA_COUNT = 16;

    private Camera[] _cameras = new Camera[MAX_CAMERA_COUNT];

    private CameraSharedCache[] _sharedCaches = new CameraSharedCache[MAX_CAMERA_COUNT];

    private int _count;

    public static CameraSharedCachePool instance => _instance;

    public static void Initialize()
    {
        if (_instance == null)
        {
            _instance = new CameraSharedCachePool();
        }
    }

    public static void Terminate()
    {
        if (_instance != null)
        {
            _instance = null;
        }
    }

    public static ICameraSharedCache CreateSharedCache(Camera camera)
    {
        if (_instance == null)
        {
            return new Dummy(camera);
        }
        return _instance.CreateSharedCacheImpl(camera);
    }

    public void Fetch()
    {
        int i = 0;
        for (int count = _count; i < count; i++)
        {
            CameraSharedCache cameraSharedCache = _sharedCaches[i];
            if (cameraSharedCache.camera.isActiveAndEnabled)
            {
                cameraSharedCache.Fetch();
            }
        }
    }

    private ICameraSharedCache CreateSharedCacheImpl(Camera camera)
    {
        int i = 0;
        for (int count = _count; i < count; i++)
        {
            if (_cameras[i] == camera)
            {
                return _sharedCaches[i];
            }
        }
        int num = _count++;
        _cameras[num] = camera;
        CameraSharedCache cameraSharedCache = new CameraSharedCache(camera);
        _sharedCaches[num] = cameraSharedCache;
        return cameraSharedCache;
    }
}
