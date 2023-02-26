using UnityEngine;

public class A2UManager : MonoBehaviour
{
    public const int A2ULayer = 29;

    protected A2UController _a2uController;

    protected A2UCamera _a2uCamera;

    public A2UController a2uController => _a2uController;

    public A2UCamera a2uCamera => _a2uCamera;

    private void OnDestroy()
    {
        Final();
    }

    public void InitController(ref A2UController.InitContext context)
    {
        if (!(null != _a2uController))
        {
            _a2uController = GetComponentInChildren<A2UController>();
            _a2uController.Init(ref context);
        }
    }

    public void InitCamera(int rtWidth, int rtHeight)
    {
        if (!(null != _a2uCamera))
        {
            _a2uCamera = GetComponentInChildren<A2UCamera>();
            _a2uCamera.Init(rtWidth, rtHeight);
        }
    }

    public virtual void Final()
    {
        if (null != _a2uController)
        {
            _a2uController.Final();
            _a2uController = null;
        }
        if (null != _a2uCamera)
        {
            _a2uCamera.Final();
            _a2uCamera = null;
        }
    }
}
