using UnityEngine;

/// <summary>
/// ハーモニクスで使用
/// </summary>
public class ScaleKeeper : MonoBehaviour
{
    [SerializeField]
    public Transform parentTransform;

    [SerializeField]
    public Vector3 targetScale;

    private Vector3 tempScale;

    private void Update()
    {
        tempScale = Vector3.one;
        if (GetParentScaleRecursive(ref tempScale, base.transform.parent))
        {
            tempScale.x = targetScale.x / tempScale.x;
            tempScale.y = targetScale.y / tempScale.y;
            tempScale.z = targetScale.z / tempScale.z;
            base.transform.localScale = tempScale;
        }
    }

    private bool GetParentScaleRecursive(ref Vector3 totalScale, Transform node)
    {
        if (node == null || parentTransform == null)
        {
            return false;
        }
        if (node == parentTransform)
        {
            totalScale.Scale(node.localScale);
            return true;
        }
        return GetParentScaleRecursive(ref totalScale, node.parent);
    }
}
