using UnityEngine;

[AddComponentMenu("Dynamic Bone/Dynamic Bone Collider")]
public class DynamicBoneCollider : DynamicBoneColliderBase
{
#if UNITY_5_3_OR_NEWER
	[Tooltip("The radius of the sphere or capsule.")]
#endif	
    public float m_Radius = 0.5f;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("The height of the capsule.")]
#endif		
    public float m_Height = 0;

    void OnValidate()
    {
        m_Radius = Mathf.Max(m_Radius, 0);
        m_Height = Mathf.Max(m_Height, 0);
    }

    public override bool Collide(ref Vector3 particlePosition, float particleRadius)
    {
        float radius = m_Radius * Mathf.Abs(transform.lossyScale.x);
        float h = m_Height * 0.5f - m_Radius;
        if (h <= 0)
        {
            if (m_Bound == Bound.Outside)
                return OutsideSphere(ref particlePosition, particleRadius, transform.TransformPoint(m_Center), radius);
            else
                return InsideSphere(ref particlePosition, particleRadius, transform.TransformPoint(m_Center), radius);
        }
        else
        {
            Vector3 c0 = m_Center;
            Vector3 c1 = m_Center;

            switch (m_Direction)
            {
                case Direction.X:
                    c0.x -= h;
                    c1.x += h;
                    break;
                case Direction.Y:
                    c0.y -= h;
                    c1.y += h;
                    break;
                case Direction.Z:
                    c0.z -= h;
                    c1.z += h;
                    break;
            }
            if (m_Bound == Bound.Outside)
                return OutsideCapsule(ref particlePosition, particleRadius, transform.TransformPoint(c0), transform.TransformPoint(c1), radius);
            else
                return InsideCapsule(ref particlePosition, particleRadius, transform.TransformPoint(c0), transform.TransformPoint(c1), radius);
        }
    }

    static bool OutsideSphere(ref Vector3 particlePosition, float particleRadius, Vector3 sphereCenter, float sphereRadius)
    {
        float r = sphereRadius + particleRadius;
        float r2 = r * r;
        Vector3 d = particlePosition - sphereCenter;
        float len2 = d.sqrMagnitude;

        // if is inside sphere, project onto sphere surface
        if (len2 > 0 && len2 < r2)
        {
            float len = Mathf.Sqrt(len2);
            particlePosition = sphereCenter + d * (r / len);
            return true;
        }
        return false;
    }

    static bool InsideSphere(ref Vector3 particlePosition, float particleRadius, Vector3 sphereCenter, float sphereRadius)
    {
        float r = sphereRadius - particleRadius;
        float r2 = r * r;
        Vector3 d = particlePosition - sphereCenter;
        float len2 = d.sqrMagnitude;

        // if is outside sphere, project onto sphere surface
        if (len2 > r2)
        {
            float len = Mathf.Sqrt(len2);
            particlePosition = sphereCenter + d * (r / len);
            return true;
        }
        return false;
    }

    static bool OutsideCapsule(ref Vector3 particlePosition, float particleRadius, Vector3 capsuleP0, Vector3 capsuleP1, float capsuleRadius)
    {
        float r = capsuleRadius + particleRadius;
        float r2 = r * r;
        Vector3 dir = capsuleP1 - capsuleP0;
        Vector3 d = particlePosition - capsuleP0;
        float t = Vector3.Dot(d, dir);

        if (t <= 0)
        {
            // check sphere1
            float len2 = d.sqrMagnitude;
            if (len2 > 0 && len2 < r2)
            {
                float len = Mathf.Sqrt(len2);
                particlePosition = capsuleP0 + d * (r / len);
                return true;
            }
        }
        else
        {
            float dl = dir.sqrMagnitude;
            if (t >= dl)
            {
                // check sphere2
                d = particlePosition - capsuleP1;
                float len2 = d.sqrMagnitude;
                if (len2 > 0 && len2 < r2)
                {
                    float len = Mathf.Sqrt(len2);
                    particlePosition = capsuleP1 + d * (r / len);
                    return true;
                }
            }
            else if (dl > 0)
            {
                // check cylinder
                t /= dl;
                d -= dir * t;
                float len2 = d.sqrMagnitude;
                if (len2 > 0 && len2 < r2)
                {
                    float len = Mathf.Sqrt(len2);
                    particlePosition += d * ((r - len) / len);
                    return true;
                }
            }
        }
        return false;
    }

    static bool InsideCapsule(ref Vector3 particlePosition, float particleRadius, Vector3 capsuleP0, Vector3 capsuleP1, float capsuleRadius)
    {
        float r = capsuleRadius - particleRadius;
        float r2 = r * r;
        Vector3 dir = capsuleP1 - capsuleP0;
        Vector3 d = particlePosition - capsuleP0;
        float t = Vector3.Dot(d, dir);

        if (t <= 0)
        {
            // check sphere1
            float len2 = d.sqrMagnitude;
            if (len2 > r2)
            {
                float len = Mathf.Sqrt(len2);
                particlePosition = capsuleP0 + d * (r / len);
                return true;
            }
        }
        else
        {
            float dl = dir.sqrMagnitude;
            if (t >= dl)
            {
                // check sphere2
                d = particlePosition - capsuleP1;
                float len2 = d.sqrMagnitude;
                if (len2 > r2)
                {
                    float len = Mathf.Sqrt(len2);
                    particlePosition = capsuleP1 + d * (r / len);
                    return true;
                }
            }
            else if (dl > 0)
            {
                // check cylinder
                t /= dl;
                d -= dir * t;
                float len2 = d.sqrMagnitude;
                if (len2 > r2)
                {
                    float len = Mathf.Sqrt(len2);
                    particlePosition += d * ((r - len) / len);
                    return true;
                }
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
        float radius = m_Radius * Mathf.Abs(transform.lossyScale.x);
        float h = m_Height * 0.5f - m_Radius;
        if (h <= 0)
        {
            Gizmos.DrawWireSphere(transform.TransformPoint(m_Center), radius);
        }
        else
        {
            Vector3 c0 = m_Center;
            Vector3 c1 = m_Center;

            switch (m_Direction)
            {
                case Direction.X:
                    c0.x -= h;
                    c1.x += h;
                    break;
                case Direction.Y:
                    c0.y -= h;
                    c1.y += h;
                    break;
                case Direction.Z:
                    c0.z -= h;
                    c1.z += h;
                    break;
            }
            Gizmos.DrawWireSphere(transform.TransformPoint(c0), radius);
            Gizmos.DrawWireSphere(transform.TransformPoint(c1), radius);
        }
    }
}
