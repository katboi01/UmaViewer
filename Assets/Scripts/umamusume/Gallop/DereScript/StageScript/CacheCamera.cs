using UnityEngine;

public class CacheCamera
{
    private Camera _camera;

    private Transform _cacheTransform;

    public Camera camera => _camera;

    public Transform cacheTransform => _cacheTransform;

    public CacheCamera(Camera c)
    {
        Set(c);
    }

    public void Set(Camera c)
    {
        _camera = c;
        if (c == null)
        {
            _cacheTransform = null;
        }
        else
        {
            _cacheTransform = c.GetComponent<Transform>();
        }
    }
}
