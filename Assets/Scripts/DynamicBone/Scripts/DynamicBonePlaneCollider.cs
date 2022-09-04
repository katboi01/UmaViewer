using UnityEngine;

[AddComponentMenu("Dynamic Bone/Dynamic Bone Plane Collider")]
public class DynamicBonePlaneCollider : DynamicBoneColliderBase
{
    void OnValidate()
    {
    }

    public override bool Collide(ref Vector3 particlePosition, float particleRadius)
    {
        Vector3 normal = Vector3.up;
        switch (m_Direction)
        {
            case Direction.X:
                normal = transform.right;
                break;
            case Direction.Y:
                normal = transform.up;
                break;
            case Direction.Z:
                normal = transform.forward;
                break;
        }

        Vector3 p = transform.TransformPoint(m_Center);
        Plane plane = new Plane(normal, p);
        float d = plane.GetDistanceToPoint(particlePosition);

        if (m_Bound == Bound.Outside)
        {
            if (d < 0)
            {
                particlePosition -= normal * d;
                return true;
            }
        }
        else
        {
            if (d > 0)
            {
                particlePosition -= normal * d;
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (!enabled)
            return;

        if (m_Bound == Bound.Outside)
            Gizmos.color = Color.yellow;
        else
            Gizmos.color = Color.magenta;

        Vector3 normal = Vector3.up;
        switch (m_Direction)
        {
            case Direction.X:
                normal = transform.right;
                break;
            case Direction.Y:
                normal = transform.up;
                break;
            case Direction.Z:
                normal = transform.forward;
                break;
        }

        Vector3 p = transform.TransformPoint(m_Center);
        Gizmos.DrawLine(p, p + normal);
    }
}
