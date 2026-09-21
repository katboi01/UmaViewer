using System.Collections.Generic;
using UnityEngine;
using Gallop;
using LibMMD.Model;
using static LibMMD.Model.MMDRigidBody;

// Builds PMX rigid bodies + joints from UmaViewer's CySpring spring-bone data so a viewer
// (babylon-mmd) can simulate skirt/hair/ear/tail physics at runtime. CySpring has no direct
// MMD equivalent, so this is an approximation: each spring chain becomes a kinematic anchor
// body on the root plus a dynamic sphere body per swaying bone, joined in sequence with
// rotation limits; body colliders become kinematic capsules/spheres. Values are heuristic
// and meant to be plausible, not a bit-exact CySpring match.
public static class CySpringPhysicsExporter
{
    const int BODY_GROUP = 0;         // static body colliders (legs/torso)
    const int CLOTH_GROUP = 1;        // dynamic skirt/hair/tail
    // A set bit means "collide with that group" (hence 0xFFFF = collide with everything, the
    // usual PMX default). Bullet needs it from both sides: (groupA & maskB) && (groupB & maskA).
    const ushort CLOTH_MASK = 0x0001; // collide with group 0 (body colliders) only, not self
    const ushort BODY_MASK = 0xFFFE;  // collide with every group except 0 (other body colliders)

    // drag/stiff are held in the solver's own scale, not the raw asset values:
    // CySpringPlugin.dll divides StiffnessForce by 1000 and DragForce by 100 (0x18008ec8c,
    // 0x18008ec80). Raw StiffnessForce runs 130..700 and DragForce 200..1050, so stiff is
    // 0.13..0.7 and drag 2..10.5.
    struct P { public float drag, stiff, radius; public bool limited; public Vector3 lmax; }

    // CySpring's per-step stiffness and a PMX/Bullet angular spring constant are different
    // quantities, so only the ratio between bones carries over. This factor sets the absolute
    // level; it is a calibration.
    const float SPRING_SCALE = 50f;

    public static void Build(UmaContainerCharacter container, RawMMDModel model)
    {
        if (container == null || container.cySpringDataContainers == null) return;

        var idx = new Dictionary<string, int>();
        for (int i = 0; i < model.Bones.Length; i++)
        {
            if (!string.IsNullOrEmpty(model.Bones[i].NameEn) && !idx.ContainsKey(model.Bones[i].NameEn)) idx[model.Bones[i].NameEn] = i;
            if (!string.IsNullOrEmpty(model.Bones[i].Name) && !idx.ContainsKey(model.Bones[i].Name)) idx[model.Bones[i].Name] = i;
        }
        var kids = new Dictionary<int, List<int>>();
        for (int i = 0; i < model.Bones.Length; i++)
        {
            int p = model.Bones[i].ParentIndex;
            if (p < 0) continue;
            if (!kids.TryGetValue(p, out var l)) kids[p] = l = new List<int>();
            l.Add(i);
        }

        var param = new Dictionary<string, P>();
        var roots = new List<string>();
        foreach (var c in container.cySpringDataContainers)
        {
            if (c == null || c.springParam == null) continue;
            foreach (var e in c.springParam)
            {
                if (e == null || string.IsNullOrEmpty(e.BoneName)) continue;
                roots.Add(e.BoneName);
                param[e.BoneName] = new P { drag = e.DragForce / 100f, stiff = e.StiffnessForce / 1000f, radius = e.CollisionRadius, limited = e._isLimit, lmax = e._limitAngleMax };
                if (e._childElements == null) continue;
                foreach (var ce in e._childElements)
                {
                    if (ce == null || string.IsNullOrEmpty(ce.Name)) continue;
                    param[ce.Name] = new P { drag = ce.DragForce / 100f, stiff = ce.StiffnessForce / 1000f, radius = ce.CollisionRadius, limited = ce.IsLimit, lmax = ce.LimitAngleMax };
                }
            }
        }

        var bodies = new List<MMDRigidBody>();
        var joints = new List<MMDJoint>();
        var bodyOf = new Dictionary<int, int>();

        AddColliders(container, model, idx, bodies);
        // Always add leg capsules: CySpring's own colliders here are mostly hip/skirt-panel
        // shapes, and the knee/ankle leg-avoidance colliders live in SkirtController (not
        // read), so without these the leg clips through the skirt.
        AddLegColliders(model, idx, bodies);

        var rootSet = new HashSet<int>();
        foreach (var rn in roots) if (idx.TryGetValue(rn, out int ri)) rootSet.Add(ri);

        foreach (int ri in rootSet)
        {
            bodyOf[ri] = AddBody(bodies, model, ri, true, default);
            var stack = new Stack<int>();
            if (kids.TryGetValue(ri, out var rk)) foreach (var k in rk) stack.Push(k);
            while (stack.Count > 0)
            {
                int b = stack.Pop();
                if (rootSet.Contains(b)) continue;
                // Only CySpring bones are cloth. Animated bones that hang under a spring root
                // (the ear chain under Sp_He_Ear0) are driven by their own VMD tracks and get
                // no body -- and nothing below them does either.
                if (!SpringBoneNames.IsSpringBone(model.Bones[b].NameEn)) continue;
                P p = param.TryGetValue(model.Bones[b].NameEn, out var pp) ? pp : new P { drag = 0.4f, stiff = 3f, radius = 0.02f, limited = false };
                bodyOf[b] = AddBody(bodies, model, b, false, p);
                int par = model.Bones[b].ParentIndex;
                if (bodyOf.TryGetValue(par, out int pbody)) joints.Add(MakeJoint(model, b, pbody, bodyOf[b], p));
                if (kids.TryGetValue(b, out var bk)) foreach (var k in bk) stack.Push(k);
            }
        }

        model.Rigidbodies = bodies.ToArray();
        model.Joints = joints.ToArray();
    }

    static int AddColliders(UmaContainerCharacter container, RawMMDModel model, Dictionary<string, int> idx, List<MMDRigidBody> bodies)
    {
        int n = 0;
        foreach (var c in container.cySpringDataContainers)
        {
            if (c == null || c.collisionParam == null) continue;
            foreach (var cd in c.collisionParam)
            {
                var rd = cd != null ? cd.RuntimeData : null;
                if (rd == null || rd.TargetTransform == null || rd.TargetTransform.parent == null) continue;
                if (rd.CollisionType == CySpringCollisionData.CollisionType.None || rd.CollisionType == CySpringCollisionData.CollisionType.Plane) continue;
                if (!idx.TryGetValue(rd.TargetTransform.parent.name, out int bi) || rd.Radius <= 0f) continue;

                // CySpring runtime radii are inconsistently scaled (some colliders come back
                // body-height huge); clamp to a sane world range so the cloth doesn't explode.
                float r = Mathf.Clamp(rd.Radius, 0.01f, 0.2f);
                var body = NewBody(rd.Name ?? ("col_" + bi), bi, BODY_GROUP, BODY_MASK, RigidBodyType.RigidTypeKinematic, 0.9f, 0f);
                if (rd.CollisionType == CySpringCollisionData.CollisionType.Capsule)
                {
                    Vector3 a = rd.TargetTransform.position, b = a + rd.CapsuleAxis;
                    body.Shape = RigidBodyShape.RigidShapeCapsule;
                    body.Position = (a + b) * 0.5f;
                    body.Dimemsions = new Vector3(r, (b - a).magnitude, 0);
                    body.Rotation = CapsuleEuler(b - a);
                }
                else
                {
                    body.Shape = RigidBodyShape.RigidShapeSphere;
                    body.Position = rd.TargetTransform.position;
                    body.Dimemsions = new Vector3(r, 0, 0);
                }
                bodies.Add(body); n++;
            }
        }
        return n;
    }

    // Capsules covering the legs so the skirt collides with them instead of clipping.
    // Radii are a bit larger than the visible leg so the cloth is held clear of the mesh.
    static void AddLegColliders(RawMMDModel model, Dictionary<string, int> idx, List<MMDRigidBody> bodies)
    {
        (string a, string b, float r)[] segs =
        {
            ("Thigh_L", "Knee_L", 0.12f), ("Knee_L", "Ankle_L", 0.09f),
            ("Thigh_R", "Knee_R", 0.12f), ("Knee_R", "Ankle_R", 0.09f),
        };
        foreach (var s in segs)
        {
            if (!idx.TryGetValue(s.a, out int ia) || !idx.TryGetValue(s.b, out int ib)) continue;
            Vector3 pa = model.Bones[ia].Position, pb = model.Bones[ib].Position;
            var body = NewBody("col_" + s.a, ia, BODY_GROUP, BODY_MASK, RigidBodyType.RigidTypeKinematic, 0.9f, 0f);
            body.Shape = RigidBodyShape.RigidShapeCapsule;
            body.Position = (pa + pb) * 0.5f;
            body.Dimemsions = new Vector3(s.r, (pb - pa).magnitude, 0);
            body.Rotation = CapsuleEuler(pb - pa);
            bodies.Add(body);
        }
    }

    static int AddBody(List<MMDRigidBody> bodies, RawMMDModel model, int bone, bool anchor, P p)
    {
        float r = anchor ? 0.02f : Mathf.Clamp(p.radius, 0.015f, 0.1f);
        // High damping keeps the pendulum chain calm (underdamped chains jitter, especially
        // when a fast run yanks the kinematic anchors).
        var body = NewBody(model.Bones[bone].NameEn, bone, CLOTH_GROUP, CLOTH_MASK,
            anchor ? RigidBodyType.RigidTypeKinematic : RigidBodyType.RigidTypePhysics,
            anchor ? 0.99f : BulletDamping(p.drag), anchor ? 0f : 1f);
        body.Shape = RigidBodyShape.RigidShapeSphere;
        body.Dimemsions = new Vector3(r, 0, 0);
        body.Position = model.Bones[bone].Position;
        bodies.Add(body);
        return bodies.Count - 1;
    }

    // CySpring keeps (1 - drag) of the velocity per 30fps step; Bullet's damping is a
    // per-second rate applied as pow(1 - damping, dt). Matching one second of decay gives
    // 1 - (1 - drag/10)^30. Every drag in the data (2..10.5) lands at 0.9988 or above, so this
    // saturates the ceiling for all but the very lightest chains.
    static float BulletDamping(float drag)
    {
        float kept = Mathf.Clamp01(1f - drag * 0.1f);
        return Mathf.Clamp(1f - Mathf.Pow(kept, 30f), 0.9f, 0.995f);
    }

    static MMDRigidBody NewBody(string name, int bone, int group, ushort mask, RigidBodyType type, float damp, float mass)
    {
        return new MMDRigidBody
        {
            Name = name, NameEn = name, AssociatedBoneIndex = bone,
            CollisionGroup = group, CollisionMask = mask, Type = type,
            Mass = mass, TranslateDamp = damp, RotateDamp = damp, Restitution = 0f, Friction = 0.5f,
            Position = Vector3.zero, Rotation = Vector3.zero, Dimemsions = Vector3.zero,
            Shape = RigidBodyShape.RigidShapeSphere,
        };
    }

    static MMDJoint MakeJoint(RawMMDModel model, int bone, int bodyA, int bodyB, P p)
    {
        string name = model.Bones[bone].NameEn ?? "";
        // Jiggle bones (bust, chest accessories) are short chains that should hold their rest
        // shape and only wobble slightly: tight limits + a restoring spring stop them folding
        // in on themselves. Cloth (skirt/hair/tail/ear) wants loose limits + no spring so it
        // hangs and swings freely (a spring there oscillates -> jitter).
        bool jiggle = name.Contains("Bust") || name.Contains("Ch_Acc");
        float degLimit = jiggle ? 8f : (p.limited ? Mathf.Max(1f, Mathf.Min(Mathf.Abs(p.lmax.x), Mathf.Min(Mathf.Abs(p.lmax.y), Mathf.Abs(p.lmax.z)))) : 40f);
        float rad = Mathf.Min(degLimit, 90f) * Mathf.Deg2Rad;
        // Stiffness is CySpring's main restoring term -- it pulls each bone back toward its
        // rest direction every step, and varies about 5x across a model -- so carry the
        // per-bone value.
        float k = Mathf.Max(0f, p.stiff) * SPRING_SCALE;
        var spring = new Vector3(k, k, k);
        return new MMDJoint
        {
            Name = name, NameEn = name,
            AssociatedRigidBodyIndex = new[] { bodyA, bodyB },
            Position = model.Bones[bone].Position, Rotation = Vector3.zero,
            PositionLowLimit = Vector3.zero, PositionHiLimit = Vector3.zero,
            RotationLowLimit = new Vector3(-rad, -rad, -rad), RotationHiLimit = new Vector3(rad, rad, rad),
            SpringTranslate = Vector3.zero, SpringRotate = spring,
        };
    }

    // Euler (degrees) rotating the capsule's local +Y axis onto `dir` (Unity space).
    static Vector3 CapsuleEuler(Vector3 dir)
    {
        if (dir.sqrMagnitude < 1e-8f) return Vector3.zero;
        return Quaternion.FromToRotation(Vector3.up, dir.normalized).eulerAngles;
    }
}
