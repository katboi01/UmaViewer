using UnityEngine;

public class RendererController : MonoBehaviour
{
    [SerializeField]
    private int _lightmapIndex = -1;

    [SerializeField]
    private Vector4 _realtimeLightmapScaleOffset = new Vector4(1f, 1f, 0f, 0f);

    [SerializeField]
    private int _sortingOrder;

    private void UpdateRenderer(Renderer renderer)
    {
        if (!(renderer == null))
        {
            renderer.lightmapIndex = _lightmapIndex;
            renderer.realtimeLightmapScaleOffset = _realtimeLightmapScaleOffset;
            renderer.sortingOrder = _sortingOrder;
        }
    }

    private void Start()
    {
        Renderer component = base.gameObject.GetComponent<Renderer>();
        UpdateRenderer(component);
        base.enabled = false;
    }
}
