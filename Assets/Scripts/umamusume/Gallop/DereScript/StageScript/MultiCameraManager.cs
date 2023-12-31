using System.Collections;
using UnityEngine;

public class MultiCameraManager : MonoBehaviour
{
    public struct MaskInfo
    {
        public GameObject gameObj;

        public Transform transform;

        public Renderer[] renderer;
    }

    public const string MultiCameraAssetBundleName = "3d_multicameramask_{0}.unity3d";

    public const string MultiCameraMaterialAssetBundleName = "3d_multicameramask_mat.unity3d";

    private const string MultiCameraMaskPath = "3D/MultiCameraMask/{0}";

    private const string MultiCameraMaskMaterialPath = "3D/MultiCameraMask/Material/MultiCameraMask";

    private const string MultiCameraName = "MultiCamera:";

    private const int SortingOrder = -500;

    private MultiCamera[] _multiCamera;

    private MaskInfo[] _multiCameraMask;

    private bool _isInitialze;

    public bool isInitialize => _isInitialze;

    public int BaseCameraDepth { get; set; }

    public MultiCamera[] MultiCameraArray => _multiCamera;

    public void AttachMask(int cameraIndex, int maskIndex)
    {
        if (maskIndex >= 0 && _multiCameraMask.Length > maskIndex)
        {
            _ = _multiCamera[cameraIndex].maskIndex;
            _multiCamera[cameraIndex].DetachMask();
            _multiCamera[cameraIndex].AttachMask(ref _multiCameraMask[maskIndex], maskIndex);
        }
    }

    public void ReleaseMask()
    {
        if (_multiCameraMask == null)
        {
            return;
        }
        if (_multiCamera != null)
        {
            MultiCamera[] multiCamera = _multiCamera;
            for (int i = 0; i < multiCamera.Length; i++)
            {
                multiCamera[i].DetachMask();
            }
        }
        MaskInfo[] multiCameraMask = _multiCameraMask;
        for (int i = 0; i < multiCameraMask.Length; i++)
        {
            Object.Destroy(multiCameraMask[i].gameObj);
        }
        _multiCameraMask = null;
    }

    public void Release()
    {
        _isInitialze = false;
        if (_multiCamera != null)
        {
            MultiCamera[] multiCamera = _multiCamera;
            for (int i = 0; i < multiCamera.Length; i++)
            {
                Object.Destroy(multiCamera[i].gameObject);
            }
            _multiCamera = null;
        }
    }

    public IEnumerator Initialize(int multiCameraNum)
    {
        Release();
        _multiCamera = new MultiCamera[multiCameraNum];
        for (int i = 0; i < multiCameraNum; i++)
        {
            GameObject gameObject = new GameObject(MultiCameraName + i);
            gameObject.transform.SetParent(base.transform, worldPositionStays: false);
            _multiCamera[i] = gameObject.AddComponent<MultiCamera>();
            _multiCamera[i].Initialize();
        }
        yield break;
    }

    public IEnumerator Setup(RenderTexture colorBuffer, RenderTexture depthBuffer)
    {
        if (_multiCamera != null)
        {
            for (int i = 0; i < _multiCamera.Length; i++)
            {
                _multiCamera[i].Setup(BaseCameraDepth + i, colorBuffer, depthBuffer);
            }
        }
        yield break;
    }

    public void InitializeFinish()
    {
        _isInitialze = true;
    }

    private void OnDestroy()
    {
        ReleaseMask();
        Release();
    }
}
