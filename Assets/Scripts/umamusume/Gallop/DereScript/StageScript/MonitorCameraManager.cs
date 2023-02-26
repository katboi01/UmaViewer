using System.Collections;
using UnityEngine;

/// <summary>
/// モニターカメラ管理
/// </summary>
public class MonitorCameraManager
{
    private static readonly string MonitorCameraPrefabPath = "Prefab/MonitorCamera";

    private int _countMonitorCamera;

    private MonitorCamera[] _arrMonitorCamera;

    public int countMonitorCamera => _countMonitorCamera;

    public MonitorCamera[] monitorCameras => _arrMonitorCamera;

    public IEnumerator CreateMonitorCamera(int cntCamera, Transform tmParent, Vector2 rtResolution)
    {
        DestroyAllCamera();
        yield return null;
        if (cntCamera > 0)
        {
            GameObject original = Resources.LoadAsync<GameObject>(MonitorCameraPrefabPath).asset as GameObject;
            _countMonitorCamera = cntCamera;
            _arrMonitorCamera = new MonitorCamera[cntCamera];
            for (int i = 0; i < cntCamera; i++)
            {
                GameObject gameObject = Object.Instantiate(original);
                _arrMonitorCamera[i] = gameObject.GetComponent<MonitorCamera>();
            }
            yield return null;
            for (int j = 0; j < cntCamera; j++)
            {
                _arrMonitorCamera[j].Initialize(tmParent, (int)rtResolution.x, (int)rtResolution.y);
                _arrMonitorCamera[j].gameObject.SetActive(value: false);
            }
        }
        else if (cntCamera == 0)
        {
            _countMonitorCamera = 0;
            _arrMonitorCamera = null;
        }
    }

    public RenderTexture GetMonitorCameraTexture(int idx)
    {
        RenderTexture result = null;
        if (_countMonitorCamera > idx)
        {
            result = (_arrMonitorCamera[idx].isActiveAndEnabled ? _arrMonitorCamera[idx].targetTexture : null);
        }
        return result;
    }

    public MonitorCamera GetMonitorCamera(int idx)
    {
        MonitorCamera result = null;
        if (_countMonitorCamera > idx)
        {
            result = _arrMonitorCamera[idx];
        }
        return result;
    }

    public void SetCameraEnable(int idx, bool bEnable)
    {
        _arrMonitorCamera[idx].gameObject.SetActive(bEnable);
    }

    private void DestroyAllCamera()
    {
        for (int i = 0; i < _countMonitorCamera; i++)
        {
            Object.Destroy(_arrMonitorCamera[i].gameObject);
        }
        _countMonitorCamera = 0;
        _arrMonitorCamera = null;
    }
}
