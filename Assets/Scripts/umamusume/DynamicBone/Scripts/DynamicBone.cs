using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Dynamic Bone/Dynamic Bone")]
public class DynamicBone : MonoBehaviour
{
#if UNITY_5_3_OR_NEWER
	[Tooltip("The root of the transform hierarchy to apply physics.")]
#endif
    public Transform m_Root = null;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("Internal physics simulation rate.")]
#endif
    public float m_UpdateRate = 60.0f;
	
    public enum UpdateMode
    {
        Normal,
        AnimatePhysics,
        UnscaledTime,
        Default
    }
    public UpdateMode m_UpdateMode = UpdateMode.Default;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("How much the bones slowed down.")]
#endif
    [Range(0, 1)]
    public float m_Damping = 0.1f;
    public AnimationCurve m_DampingDistrib = null;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("How much the force applied to return each bone to original orientation.")]
#endif
    [Range(0, 1)]
    public float m_Elasticity = 0.1f;
    public AnimationCurve m_ElasticityDistrib = null;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("How much bone's original orientation are preserved.")]
#endif
    [Range(0, 1)]
    public float m_Stiffness = 0.1f;
    public AnimationCurve m_StiffnessDistrib = null;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("How much character's position change is ignored in physics simulation.")]
#endif
    [Range(0, 1)]
    public float m_Inert = 0;
    public AnimationCurve m_InertDistrib = null;

#if UNITY_5_3_OR_NEWER
    [Tooltip("How much the bones slowed down when collide.")]
#endif
    public float m_Friction = 0;
    public AnimationCurve m_FrictionDistrib = null;

#if UNITY_5_3_OR_NEWER
	[Tooltip("Each bone can be a sphere to collide with colliders. Radius describe sphere's size.")]
#endif
    public float m_Radius = 0;
    public AnimationCurve m_RadiusDistrib = null;

#if UNITY_5_3_OR_NEWER
	[Tooltip("If End Length is not zero, an extra bone is generated at the end of transform hierarchy.")]
#endif
    public float m_EndLength = 0;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("If End Offset is not zero, an extra bone is generated at the end of transform hierarchy.")]
#endif
    public Vector3 m_EndOffset = Vector3.zero;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("The force apply to bones. Partial force apply to character's initial pose is cancelled out.")]
#endif
    public Vector3 m_Gravity = Vector3.zero;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("The force apply to bones.")]
#endif
    public Vector3 m_Force = Vector3.zero;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("Collider objects interact with the bones.")]
#endif
    public List<DynamicBoneColliderBase> m_Colliders = null;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("Bones exclude from physics simulation.")]
#endif
    public List<Transform> m_Exclusions = null;
	
	
    public enum FreezeAxis
    {
        None, X, Y, Z
    }
#if UNITY_5_3_OR_NEWER
	[Tooltip("Constrain bones to move on specified plane.")]
#endif	
    public FreezeAxis m_FreezeAxis = FreezeAxis.None;
	
#if UNITY_5_3_OR_NEWER
	[Tooltip("Disable physics simulation automatically if character is far from camera or player.")]
#endif		
    public bool m_DistantDisable = false;
    public Transform m_ReferenceObject = null;
    public float m_DistanceToObject = 20;

    Vector3 m_LocalGravity = Vector3.zero;
    Vector3 m_ObjectMove = Vector3.zero;
    Vector3 m_ObjectPrevPosition = Vector3.zero;
    float m_BoneTotalLength = 0;
    float m_ObjectScale = 1.0f;
    float m_Time = 0;
    float m_Weight = 1.0f;
    bool m_DistantDisabled = false;

    class Particle
    {
        public Transform m_Transform = null;
        public int m_ParentIndex = -1;
        public float m_Damping = 0;
        public float m_Elasticity = 0;
        public float m_Stiffness = 0;
        public float m_Inert = 0;
        public float m_Friction = 0;
        public float m_Radius = 0;
        public float m_BoneLength = 0;
        public bool m_isCollide = false;

        public Vector3 m_Position = Vector3.zero;
        public Vector3 m_PrevPosition = Vector3.zero;
        public Vector3 m_EndOffset = Vector3.zero;
        public Vector3 m_InitLocalPosition = Vector3.zero;
        public Quaternion m_InitLocalRotation = Quaternion.identity;
    }

    List<Particle> m_Particles = new List<Particle>();

    void Start()
    {
        SetupParticles();
    }

    void FixedUpdate()
    {
        if (m_UpdateMode == UpdateMode.AnimatePhysics)
            PreUpdate();
    }

    void Update()
    {
        if (m_UpdateMode != UpdateMode.AnimatePhysics)
            PreUpdate();
    }

    void LateUpdate()
    {
        if (m_DistantDisable)
            CheckDistance();

        if (m_Weight > 0 && !(m_DistantDisable && m_DistantDisabled))
        {
#if UNITY_5_3_OR_NEWER
            float dt = m_UpdateMode == UpdateMode.UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
#else
            float dt = Time.deltaTime;
#endif
            UpdateDynamicBones(dt);
        }
    }

    void PreUpdate()
    {
        if (m_Weight > 0 && !(m_DistantDisable && m_DistantDisabled))
            InitTransforms();
    }

    void CheckDistance()
    {
        Transform rt = m_ReferenceObject;
        if (rt == null && Camera.main != null)
            rt = Camera.main.transform;
        if (rt != null)
        {
            float d = (rt.position - transform.position).sqrMagnitude;
            bool disable = d > m_DistanceToObject * m_DistanceToObject;
            if (disable != m_DistantDisabled)
            {
                if (!disable)
                    ResetParticlesPosition();
                m_DistantDisabled = disable;
            }
        }
    }

    void OnEnable()
    {
        ResetParticlesPosition();
    }

    void OnDisable()
    {
        InitTransforms();
    }

    void OnValidate()
    {
        m_UpdateRate = Mathf.Max(m_UpdateRate, 0);
        m_Damping = Mathf.Clamp01(m_Damping);
        m_Elasticity = Mathf.Clamp01(m_Elasticity);
        m_Stiffness = Mathf.Clamp01(m_Stiffness);
        m_Inert = Mathf.Clamp01(m_Inert);
        m_Friction = Mathf.Clamp01(m_Friction);
        m_Radius = Mathf.Max(m_Radius, 0);

        if (Application.isEditor && Application.isPlaying)
        {
            InitTransforms();
            SetupParticles();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!enabled || m_Root == null)
            return;

        if (Application.isEditor && !Application.isPlaying && transform.hasChanged)
        {
            InitTransforms();
            SetupParticles();
        }

        Gizmos.color = Color.white;
        for (int i = 0; i < m_Particles.Count; ++i)
        {
            Particle p = m_Particles[i];
            if (p.m_ParentIndex >= 0)
            {
                Particle p0 = m_Particles[p.m_ParentIndex];
                Gizmos.DrawLine(p.m_Position, p0.m_Position);
            }
            if (p.m_Radius > 0)
                Gizmos.DrawWireSphere(p.m_Position, p.m_Radius * m_ObjectScale);
        }
    }

    public void SetWeight(float w)
    {
        if (m_Weight != w)
        {
            if (w == 0)
                InitTransforms();
            else if (m_Weight == 0)
                ResetParticlesPosition();
            m_Weight = w;
        }
    }

    public float GetWeight()
    {
        return m_Weight;
    }

    void UpdateDynamicBones(float t)
    {
        if (m_Root == null)
            return;

        m_ObjectScale = Mathf.Abs(transform.lossyScale.x);
        m_ObjectMove = transform.position - m_ObjectPrevPosition;
        m_ObjectPrevPosition = transform.position;

        int loop = 1;
        float timeVar = 1;

        if (m_UpdateMode == UpdateMode.Default)
        {
            if (m_UpdateRate > 0)
            {
                timeVar = Time.deltaTime * m_UpdateRate;
            }
            else
            {
                timeVar = Time.deltaTime;
            }
        }
        else
        {
            if (m_UpdateRate > 0)
            {
                float dt = 1.0f / m_UpdateRate;
                m_Time += t;
                loop = 0;

                while (m_Time >= dt)
                {
                    m_Time -= dt;
                    if (++loop >= 3)
                    {
                        m_Time = 0;
                        break;
                    }
                }
            }
        }

        if (loop > 0)
        {
            for (int i = 0; i < loop; ++i)
            {
                UpdateParticles1(timeVar);
                UpdateParticles2(timeVar);
                m_ObjectMove = Vector3.zero;
            }
        }
        else
        {
            SkipUpdateParticles();
        }

        ApplyParticlesToTransforms();
    }

    public void SetupParticles()
    {
        m_Particles.Clear();
        if (m_Root == null)
            return;

        m_LocalGravity = m_Root.InverseTransformDirection(m_Gravity);
        m_ObjectScale = Mathf.Abs(transform.lossyScale.x);
        m_ObjectPrevPosition = transform.position;
        m_ObjectMove = Vector3.zero;
        m_BoneTotalLength = 0;
        AppendParticles(m_Root, -1, 0);
        UpdateParameters();
    }

    void AppendParticles(Transform b, int parentIndex, float boneLength)
    {
        Particle p = new Particle();
        p.m_Transform = b;
        p.m_ParentIndex = parentIndex;
        if (b != null)
        {
            p.m_Position = p.m_PrevPosition = b.position;
            p.m_InitLocalPosition = b.localPosition;
            p.m_InitLocalRotation = b.localRotation;
        }
        else 	// end bone
        {
            Transform pb = m_Particles[parentIndex].m_Transform;
            if (m_EndLength > 0)
            {
                Transform ppb = pb.parent;
                if (ppb != null)
                    p.m_EndOffset = pb.InverseTransformPoint((pb.position * 2 - ppb.position)) * m_EndLength;
                else
                    p.m_EndOffset = new Vector3(m_EndLength, 0, 0);
            }
            else
            {
                p.m_EndOffset = pb.InverseTransformPoint(transform.TransformDirection(m_EndOffset) + pb.position);
            }
            p.m_Position = p.m_PrevPosition = pb.TransformPoint(p.m_EndOffset);
        }

        if (parentIndex >= 0)
        {
            boneLength += (m_Particles[parentIndex].m_Transform.position - p.m_Position).magnitude;
            p.m_BoneLength = boneLength;
            m_BoneTotalLength = Mathf.Max(m_BoneTotalLength, boneLength);
        }

        int index = m_Particles.Count;
        m_Particles.Add(p);

        if (b != null)
        {
            for (int i = 0; i < b.childCount; ++i)
            {
                Transform child = b.GetChild(i);
                bool exclude = false;
                if (m_Exclusions != null)
                {
                    exclude = m_Exclusions.Contains(child);
                }
                if (!exclude)
                    AppendParticles(child, index, boneLength);
                else if (m_EndLength > 0 || m_EndOffset != Vector3.zero)
                    AppendParticles(null, index, boneLength);
            }

            if (b.childCount == 0 && (m_EndLength > 0 || m_EndOffset != Vector3.zero))
                AppendParticles(null, index, boneLength);
        }
    }

    public void UpdateParameters()
    {
        if (m_Root == null)
            return;

        m_LocalGravity = m_Root.InverseTransformDirection(m_Gravity);

        for (int i = 0; i < m_Particles.Count; ++i)
        {
            Particle p = m_Particles[i];
            p.m_Damping = m_Damping;
            p.m_Elasticity = m_Elasticity;
            p.m_Stiffness = m_Stiffness;
            p.m_Inert = m_Inert;
            p.m_Friction = m_Friction;
            p.m_Radius = m_Radius;

            if (m_BoneTotalLength > 0)
            {
                float a = p.m_BoneLength / m_BoneTotalLength;
                if (m_DampingDistrib != null && m_DampingDistrib.keys.Length > 0)
                    p.m_Damping *= m_DampingDistrib.Evaluate(a);
                if (m_ElasticityDistrib != null && m_ElasticityDistrib.keys.Length > 0)
                    p.m_Elasticity *= m_ElasticityDistrib.Evaluate(a);
                if (m_StiffnessDistrib != null && m_StiffnessDistrib.keys.Length > 0)
                    p.m_Stiffness *= m_StiffnessDistrib.Evaluate(a);
                if (m_InertDistrib != null && m_InertDistrib.keys.Length > 0)
                    p.m_Inert *= m_InertDistrib.Evaluate(a);
                if (m_FrictionDistrib != null && m_FrictionDistrib.keys.Length > 0)
                    p.m_Friction *= m_FrictionDistrib.Evaluate(a);
                if (m_RadiusDistrib != null && m_RadiusDistrib.keys.Length > 0)
                    p.m_Radius *= m_RadiusDistrib.Evaluate(a);
            }

            p.m_Damping = Mathf.Clamp01(p.m_Damping);
            p.m_Elasticity = Mathf.Clamp01(p.m_Elasticity);
            p.m_Stiffness = Mathf.Clamp01(p.m_Stiffness);
            p.m_Inert = Mathf.Clamp01(p.m_Inert);
            p.m_Friction = Mathf.Clamp01(p.m_Friction);
            p.m_Radius = Mathf.Max(p.m_Radius, 0);
        }
    }

    void InitTransforms()
    {
        for (int i = 0; i < m_Particles.Count; ++i)
        {
            Particle p = m_Particles[i];
            if (p.m_Transform != null)
            {
                p.m_Transform.localPosition = p.m_InitLocalPosition;
                p.m_Transform.localRotation = p.m_InitLocalRotation;
            }
        }
    }

    void ResetParticlesPosition()
    {
        for (int i = 0; i < m_Particles.Count; ++i)
        {
            Particle p = m_Particles[i];
            if (p.m_Transform != null)
            {
                p.m_Position = p.m_PrevPosition = p.m_Transform.position;
            }
            else	// end bone
            {
                Transform pb = m_Particles[p.m_ParentIndex].m_Transform;
                p.m_Position = p.m_PrevPosition = pb.TransformPoint(p.m_EndOffset);
            }
            p.m_isCollide = false;
        }
        m_ObjectPrevPosition = transform.position;
    }

    void UpdateParticles1(float timeVar)
    {
        Vector3 force = m_Gravity;
        Vector3 fdir = m_Gravity.normalized;
        Vector3 rf = m_Root.TransformDirection(m_LocalGravity);
        Vector3 pf = fdir * Mathf.Max(Vector3.Dot(rf, fdir), 0);	// project current gravity to rest gravity
        force -= pf;	// remove projected gravity
        force = (force + m_Force) * (m_ObjectScale * timeVar);

        for (int i = 0; i < m_Particles.Count; ++i)
        {
            Particle p = m_Particles[i];
            if (p.m_ParentIndex >= 0)
            {
                // verlet integration
                Vector3 v = p.m_Position - p.m_PrevPosition;
                Vector3 rmove = m_ObjectMove * p.m_Inert;
                p.m_PrevPosition = p.m_Position + rmove;
                float damping = p.m_Damping;
                if (p.m_isCollide)
                {
					damping += p.m_Friction;
					if (damping > 1)
						damping = 1;
                    p.m_isCollide = false;
                }
                p.m_Position += v * (1 - damping) + force + rmove;
            }
            else
            {
                p.m_PrevPosition = p.m_Position;
                p.m_Position = p.m_Transform.position;
            }
        }
    }

    void UpdateParticles2(float timeVar)
    {
        Plane movePlane = new Plane();

        for (int i = 1; i < m_Particles.Count; ++i)
        {
            Particle p = m_Particles[i];
            Particle p0 = m_Particles[p.m_ParentIndex];

            float restLen;
            if (p.m_Transform != null)
                restLen = (p0.m_Transform.position - p.m_Transform.position).magnitude;
            else
                restLen = p0.m_Transform.localToWorldMatrix.MultiplyVector(p.m_EndOffset).magnitude;

            // keep shape
            float stiffness = Mathf.Lerp(1.0f, p.m_Stiffness, m_Weight);
            if (stiffness > 0 || p.m_Elasticity > 0)
            {
                Matrix4x4 m0 = p0.m_Transform.localToWorldMatrix;
                m0.SetColumn(3, p0.m_Position);
                Vector3 restPos;
                if (p.m_Transform != null)
                    restPos = m0.MultiplyPoint3x4(p.m_Transform.localPosition);
                else
                    restPos = m0.MultiplyPoint3x4(p.m_EndOffset);

                Vector3 d = restPos - p.m_Position;
                p.m_Position += d * (p.m_Elasticity * timeVar);

                if (stiffness > 0)
                {
                    d = restPos - p.m_Position;
                    float len = d.magnitude;
                    float maxlen = restLen * (1 - stiffness) * 2;
                    if (len > maxlen)
                        p.m_Position += d * ((len - maxlen) / len);
                }
            }

            // collide
            if (m_Colliders != null)
            {
                float particleRadius = p.m_Radius * m_ObjectScale;
                for (int j = 0; j < m_Colliders.Count; ++j)
                {
                    DynamicBoneColliderBase c = m_Colliders[j];
                    if (c != null && c.enabled)                    
                        p.m_isCollide |= c.Collide(ref p.m_Position, particleRadius);                    
                }
            }

            // freeze axis, project to plane 
            if (m_FreezeAxis != FreezeAxis.None)
            {
                switch (m_FreezeAxis)
                {
                    case FreezeAxis.X:
                        movePlane.SetNormalAndPosition(p0.m_Transform.right, p0.m_Position);
                        break;
                    case FreezeAxis.Y:
                        movePlane.SetNormalAndPosition(p0.m_Transform.up, p0.m_Position);
                        break;
                    case FreezeAxis.Z:
                        movePlane.SetNormalAndPosition(p0.m_Transform.forward, p0.m_Position);
                        break;
                }
                p.m_Position -= movePlane.normal * movePlane.GetDistanceToPoint(p.m_Position);
            }

            // keep length
            Vector3 dd = p0.m_Position - p.m_Position;
            float leng = dd.magnitude;
            if (leng > 0)
                p.m_Position += dd * ((leng - restLen) / leng);
        }
    }

    // only update stiffness and keep bone length
    void SkipUpdateParticles()
    {
        for (int i = 0; i < m_Particles.Count; ++i)
        {
            Particle p = m_Particles[i];
            if (p.m_ParentIndex >= 0)
            {
                p.m_PrevPosition += m_ObjectMove;
                p.m_Position += m_ObjectMove;

                Particle p0 = m_Particles[p.m_ParentIndex];

                float restLen;
                if (p.m_Transform != null)
                    restLen = (p0.m_Transform.position - p.m_Transform.position).magnitude;
                else
                    restLen = p0.m_Transform.localToWorldMatrix.MultiplyVector(p.m_EndOffset).magnitude;

                // keep shape
                float stiffness = Mathf.Lerp(1.0f, p.m_Stiffness, m_Weight);
                if (stiffness > 0)
                {
                    Matrix4x4 m0 = p0.m_Transform.localToWorldMatrix;
                    m0.SetColumn(3, p0.m_Position);
                    Vector3 restPos;
                    if (p.m_Transform != null)
                        restPos = m0.MultiplyPoint3x4(p.m_Transform.localPosition);
                    else
                        restPos = m0.MultiplyPoint3x4(p.m_EndOffset);

                    Vector3 d = restPos - p.m_Position;
                    float len = d.magnitude;
                    float maxlen = restLen * (1 - stiffness) * 2;
                    if (len > maxlen)
                        p.m_Position += d * ((len - maxlen) / len);
                }

                // keep length
                Vector3 dd = p0.m_Position - p.m_Position;
                float leng = dd.magnitude;
                if (leng > 0)
                    p.m_Position += dd * ((leng - restLen) / leng);
            }
            else
            {
                p.m_PrevPosition = p.m_Position;
                p.m_Position = p.m_Transform.position;
            }
        }
    }

    static Vector3 MirrorVector(Vector3 v, Vector3 axis)
    {
        return v - axis * (Vector3.Dot(v, axis) * 2);
    }

    void ApplyParticlesToTransforms()
    {
#if !UNITY_5_4_OR_NEWER
        // detect negative scale
        Vector3 ax = Vector3.right;
        Vector3 ay = Vector3.up;
        Vector3 az = Vector3.forward;
        bool nx = false, ny = false, nz = false;

        Vector3 loosyScale = transform.lossyScale;
        if (loosyScale.x < 0 || loosyScale.y < 0 || loosyScale.z < 0)
        {
            Transform mirrorObject = transform;
            do
            {
                Vector3 ls = mirrorObject.localScale;
                nx = ls.x < 0;
                if (nx)
                    ax = mirrorObject.right;
                ny = ls.y < 0;
                if (ny)
                    ay = mirrorObject.up;
                nz = ls.z < 0;
                if (nz)
                    az = mirrorObject.forward;
                if (nx || ny || nz)
                    break;

                mirrorObject = mirrorObject.parent;
            }
            while (mirrorObject != null);
        }
#endif

        for (int i = 1; i < m_Particles.Count; ++i)
        {
            Particle p = m_Particles[i];
            Particle p0 = m_Particles[p.m_ParentIndex];

            if (p0.m_Transform.childCount <= 1)		// do not modify bone orientation if has more then one child
            {
                Vector3 v;
                if (p.m_Transform != null)
                    v = p.m_Transform.localPosition;
                else
                    v = p.m_EndOffset;
                Vector3 v2 = p.m_Position - p0.m_Position;
#if !UNITY_5_4_OR_NEWER					
                if (nx)
                    v2 = MirrorVector(v2, ax);
                if (ny)
                    v2 = MirrorVector(v2, ay);
                if (nz)
                    v2 = MirrorVector(v2, az);
#endif					
                Quaternion rot = Quaternion.FromToRotation(p0.m_Transform.TransformDirection(v), v2);
                p0.m_Transform.rotation = rot * p0.m_Transform.rotation;
            }

            if (p.m_Transform != null)
                p.m_Transform.position = p.m_Position;
        }
    }
}
