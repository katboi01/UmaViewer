using UnityEngine;

public class Character3DBase_CheekRenderer : MonoBehaviour
{
    private Renderer _renderer;

    private Transform _rootTransform;

    public float DistanceForLOD { get; set; }

    public bool EnableLOD { get; set; }

    public bool IsVisible { get; set; }

    public void Initialize(Renderer target)
    {
        _renderer = target;
        _rootTransform = base.transform;
    }

    private void OnWillRenderObject()
    {
        if (!IsVisible || _renderer == null)
        {
            return;
        }
        if (!EnableLOD)
        {
            _renderer.enabled = true;
            return;
        }
        Camera current = Camera.current;
        Vector3 position = current.transform.position;
        float num = current.fieldOfView / 360f;
        if ((position - _rootTransform.position).sqrMagnitude * num * num > DistanceForLOD * DistanceForLOD)
        {
            _renderer.enabled = false;
        }
        else
        {
            _renderer.enabled = true;
        }
    }
}
