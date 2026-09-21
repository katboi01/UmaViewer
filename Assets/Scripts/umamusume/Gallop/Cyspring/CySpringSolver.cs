using UnityEngine;

namespace Gallop
{
    // Managed C# implementation of CySpringPlugin.dll, operating in place on the same Native*
    // structs. Lets CySpring run without the native plugin (WebGL/IL2CPP, or any platform via
    // CySpringNative.UseNativePlugin = false).
    //
    // Each stage cites the plugin address it reproduces (x64 build, ImageBase 0x180000000).
    // Arithmetic is written in the plugin's evaluation order where it affects rounding, since
    // the output is compared to the plugin to the last bit. Verification: CySpringDiff
    // (--solver-diff, single-step) and CySpringTrace (--solver-trace, free-running).
    public static class CySpringSolver
    {
        // 0x18008ec7c: every force term is divided by 30 (0x180009b1f, 0x180009b24, 0x180009bef,
        // 0x180009c26). Wind is not.
        const float FORCE_DIV = 30f;

        static Quaternion Qmul(Quaternion a, Quaternion b) => new Quaternion(
            a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
            a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
            a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
            a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z);

        static Vector3 Qrot(Quaternion q, Vector3 v)
        {
            Vector3 u = new Vector3(q.x, q.y, q.z);
            float s = q.w;
            return u * (2f * Vector3.Dot(u, v)) + v * (s * s - Vector3.Dot(u, u)) + Vector3.Cross(u, v) * (2f * s);
        }

        static Quaternion QConj(Quaternion q) => new Quaternion(-q.x, -q.y, -q.z, q.w);

        static float Len(Vector3 a) => Mathf.Sqrt(a.x * a.x + a.y * a.y + a.z * a.z);
        static float Len2(Vector3 v) => v.x * v.x + v.y * v.y + v.z * v.z;
        static Vector3 Norm(Vector3 a) { float l = Len(a); return l > 1e-8f ? a * (1f / l) : Vector3.zero; }
        static float SafeSqrt(float x) => x > 0f ? Mathf.Sqrt(x) : 0f;

        /// Shortest-arc quaternion rotating unit a onto unit b. No near-parallel shortcut: the
        /// construction is well conditioned as d -> 1 and yields identity on its own; a guard
        /// there would quantise small swings to nothing. Only the antipodal case branches.
        static Quaternion QFromTo(Vector3 a, Vector3 b)
        {
            Vector3 an = Norm(a), bn = Norm(b);
            float d = Vector3.Dot(an, bn);
            if (d <= -1f + 1e-6f)
            {
                Vector3 ax = Norm(Vector3.Cross(new Vector3(1, 0, 0), an));
                if (Len(ax) < 1e-6f) ax = Norm(Vector3.Cross(new Vector3(0, 1, 0), an));
                return new Quaternion(ax.x, ax.y, ax.z, 0);
            }
            Vector3 c = Vector3.Cross(an, bn);
            float s = Mathf.Sqrt((1f + d) * 2f), inv = 1f / s;
            return new Quaternion(c.x * inv, c.y * inv, c.z * inv, s * 0.5f);
        }

        /// Axis-angle quaternion from degrees, as the plugin builds it: half-angle = deg * pi/180
        /// * 0.5 (0x18008ec00, 0x18008ec10), then (axis * sin, cos) via the CRT sin/cos
        /// (0x180002716, 0x180001938).
        static Quaternion QAxisDeg(Vector3 axis, float deg)
        {
            double h = deg * 0.0174532924 * 0.5;
            float sn = (float)System.Math.Sin(h), cs = (float)System.Math.Cos(h);
            return new Quaternion(axis.x * sn, axis.y * sn, axis.z * sn, cs);
        }

        const float FLT_EPS = 1.1920929e-07f;   // 0x18008ebe4

        // ------------------------------------------------------------------ diagnostics

        // Branch counters, reset per --solver-diff run and reported in the CLI log.
        public static long ClampCalls, ClampBites, CollisionCalls, SkirtKneeHits;
        public static long HitSphere, HitCapsuleMid, HitCapsuleEnd, HitPlane, SkipChara, SeenInner, SkipDisabled;
        public static readonly long[] TypeSeen = new long[8];
        public static long CollisionHits => HitSphere + HitCapsuleMid + HitCapsuleEnd + HitPlane;
        public static void ResetCounters()
        {
            ClampCalls = ClampBites = CollisionCalls = SkirtKneeHits = 0;
            HitSphere = HitCapsuleMid = HitCapsuleEnd = HitPlane = SkipChara = SeenInner = SkipDisabled = 0;
            for (int i = 0; i < 8; i++) TypeSeen[i] = 0;
        }

        // ------------------------------------------------------------------ collision

        /// Sixteen collider slots: 0x180007370 indexes CIndex0..7 from 0xf4 and CIndex8..15 from
        /// 0x144 (landing on 0x154..0x162).
        static int CIndex(ref NativeClothWorking b, int i)
        {
            switch (i)
            {
                case 0: return b.CIndex0; case 1: return b.CIndex1; case 2: return b.CIndex2; case 3: return b.CIndex3;
                case 4: return b.CIndex4; case 5: return b.CIndex5; case 6: return b.CIndex6; case 7: return b.CIndex7;
                case 8: return b.CIndex8; case 9: return b.CIndex9; case 10: return b.CIndex10; case 11: return b.CIndex11;
                case 12: return b.CIndex12; case 13: return b.CIndex13; case 14: return b.CIndex14; default: return b.CIndex15;
            }
        }

        /// Colliders are stored in their root parent's space. 0x180007c3e reads the collider's
        /// ParentWorkIndex (0x40), rotates by WorldRotation (+0x10) and adds WorldPosition (+0x00)
        /// (0x180007d7f, 0x180007db8, 0x180007e07).
        static Vector3 ColliderWorld(Vector3 local, NativeRootParentWork[] parents, int pi)
        {
            if (parents == null || pi < 0 || pi >= parents.Length) return local;
            return Qrot(parents[pi].WorldRotation, local) + parents[pi].WorldPosition;
        }

        /// pos on the sphere of radius R about center, along offset d with |d| = dist:
        /// d / dist * R + center per component (0x180007e63-0x180007e88, 0x180007f2f-0x180007f54).
        static Vector3 OnSphere(Vector3 center, Vector3 d, float dist, float R)
        {
            return new Vector3(d.x / dist * R + center.x, d.y / dist * R + center.y, d.z / dist * R + center.z);
        }

        /// Pull pos onto the sphere of radius InitBoneDistance about anchor:
        /// dir / len * D + anchor per component (0x180009eeb-0x180009f12).
        static void ConstrainLength(ref NativeClothWorking b, Vector3 anchor, ref Vector3 pos)
        {
            Vector3 dir = pos - anchor;
            float dl = Len(dir);
            if (dl > 1e-6f)
            {
                float D = b.InitBoneDistance;
                pos = new Vector3(dir.x / dl * D + anchor.x, dir.y / dl * D + anchor.y, dir.z / dl * D + anchor.z);
            }
        }

        /// One collider (routine at 0x1800072f0). Type dispatch at 0x1800073b6: 0 sphere,
        /// 2 capsule, 3 plane; type 1 is not handled.
        static void ResolveOne(ref NativeClothWorking bne, ref NativeClothCollision c,
                               NativeRootParentWork[] parents, ref Vector3 pos)
        {
            CollisionCalls++;
            if (c.Type >= 0 && c.Type < 8) TypeSeen[c.Type]++;
            if (c.IsEnable == 0) { SkipDisabled++; return; }
            // 0x180007397: a chara collider applies only to bones with CheckCharaCollision set.
            if (bne.CheckCharaCollision == 0 && c.IsCharaCollision != 0) { SkipChara++; return; }

            switch (c.Type)
            {
                case 0:
                {
                    Vector3 center = ColliderWorld(c.Position, parents, c.ParentWorkIndex);
                    Vector3 d = pos - center;
                    float d2 = Len2(d);
                    // Squared-distance tests, inclusive. Inner: R = Radius - CollisionRadius,
                    // skip when d2 < R2 (0x180007e26-0x180007e3b). Outer: R = Radius +
                    // CollisionRadius, skip when R2 < d2 (0x180007ef2-0x180007f07).
                    if (c.IsInner != 0)
                    {
                        SeenInner++;
                        float R = c.Radius - bne.CollisionRadius;
                        if (d2 < R * R) return;
                        pos = OnSphere(center, d, Mathf.Sqrt(d2), R);
                    }
                    else
                    {
                        float R = c.Radius + bne.CollisionRadius;
                        if (R * R < d2) return;
                        pos = OnSphere(center, d, Mathf.Sqrt(d2), R);
                    }
                    HitSphere++;
                    ConstrainLength(ref bne, bne.SelfPosition, ref pos);   // 0x180007e9c-0x180008004
                    return;
                }

                case 2:
                {
                    Vector3 p0 = ColliderWorld(c.Position, parents, c.ParentWorkIndex);
                    Vector3 p1 = ColliderWorld(c.Position2, parents, c.ParentWorkIndex);
                    float R = c.Radius + bne.CollisionRadius;             // 0x1800077c8; no IsInner for capsules
                    Vector3 ab = p1 - p0;
                    float L = Mathf.Sqrt(Len2(ab));                       // 0x1800078de
                    Vector3 abh = new Vector3(ab.x / L, ab.y / L, ab.z / L);
                    Vector3 v = pos - p0;
                    float t = Vector3.Dot(v, abh);                        // length along the axis

                    // Side, for 0 <= t < L (0x180007957, 0x180007964); strict test (0x1800079f9).
                    if (t >= 0f && t < L)
                    {
                        Vector3 along = new Vector3(t * abh.x, t * abh.y, t * abh.z);
                        Vector3 perp = v - along;
                        float d = Mathf.Sqrt(Len2(perp));
                        if (R > d)
                        {
                            float k = R / d;                              // (p0 + along) + perp * (R / d), 0x1800079ff-0x180007a32
                            pos = new Vector3(along.x + p0.x + perp.x * k, along.y + p0.y + perp.y * k, along.z + p0.z + perp.z * k);
                            HitCapsuleMid++;
                            ConstrainLength(ref bne, bne.SelfPosition, ref pos);   // 0x180007a45
                            return;
                        }
                        // A side miss continues into the endpoint tests (0x180007aec).
                    }

                    // Endpoint spheres, p0 then p1, inclusive (0x180007b24, 0x180007bce). Neither
                    // re-constrains; both exit to 0x18000800a.
                    float d0 = Len2(v);
                    if (!(R * R < d0))
                    {
                        pos = OnSphere(p0, v, Mathf.Sqrt(d0), R);
                        HitCapsuleEnd++;
                        return;
                    }
                    Vector3 w1 = pos - p1;
                    float d1 = Len2(w1);
                    if (!(R * R < d1))
                    {
                        pos = OnSphere(p1, w1, Mathf.Sqrt(d1), R);
                        HitCapsuleEnd++;
                    }
                    return;
                }

                case 3:
                {
                    // Half-space; Normal and Distance are used untransformed (0x1800073d3).
                    Vector3 n = c.Normal;
                    float dot = pos.x * n.x + pos.y * n.y + pos.z * n.z - c.Distance;
                    if (dot > bne.CollisionRadius) return;                // 0x180007412
                    pos = pos + n * (bne.CollisionRadius - dot);
                    HitPlane++;
                    ConstrainLength(ref bne, bne.SelfPosition, ref pos);  // 0x18000744e
                    return;
                }
            }
        }

        /// ActiveCollision is the number of occupied slots (loop bound at 0x180008018).
        static void ResolveCollisions(ref NativeClothWorking bne, NativeClothCollision[] col,
                                      NativeRootParentWork[] parents, ref Vector3 pos)
        {
            if (col == null) return;
            int n = bne.ActiveCollision;
            if (n > 16) n = 16;
            for (int i = 0; i < n; i++)
            {
                int idx = CIndex(ref bne, i);
                if (idx < 0 || idx >= col.Length) continue;
                ResolveOne(ref bne, ref col[idx], parents, ref pos);
            }
        }

        // ------------------------------------------------------------------ rotation limit

        /// One axis: fmod(a + 360, 360) (0x18000a4ca/0x18000a4e9/0x18000a506 against 0x18008eca0),
        /// fold above 180 (0x18000a516 against 0x18008ec84), then clamp to [-Min, Max] --
        /// 0x18000a558 negates LimitRotationMin before comparing.
        static float ClampLimitAngle(float a, float lo, float hi)
        {
            ClampCalls++;
            a = a % 360f;
            if (a < 0f) a += 360f;
            if (a > 180f) a -= 360f;
            float min = -lo;
            float r = (min > a) ? min : Mathf.Min(hi, a);
            if (r != a) ClampBites++;
            return r;
        }

        /// Clamp the deflection from the rest pose q into the per-axis box. The euler helper at
        /// 0x180008400 is Unity's ZXY QuaternionToEuler in degrees (FLT_EPSILON gimbal test at
        /// 0x18008ebe8, +-pi/2 lock values at 0x18008ec0c/0x18008ec98, 360/2pi at 0x180008645), so
        /// Quaternion.eulerAngles and Quaternion.Euler reproduce it directly.
        static Quaternion ApplyRotationLimit(ref NativeClothWorking b, Quaternion q, Quaternion final)
        {
            Vector3 e = Qmul(QConj(q), final).eulerAngles;
            e.x = ClampLimitAngle(e.x, b.LimitRotationMin.x, b.LimitRotationMax.x);
            e.y = ClampLimitAngle(e.y, b.LimitRotationMin.y, b.LimitRotationMax.y);
            e.z = ClampLimitAngle(e.z, b.LimitRotationMin.z, b.LimitRotationMax.z);
            return Qmul(q, Quaternion.Euler(e));
        }

        // ------------------------------------------------------------------ per-bone solve

        static float springRate, moveRate, addMoveRate;

        /// Per-bone solve (0x180009760).
        static void SolveCloth(ref NativeClothWorking b, NativeClothCollision[] col,
            NativeRootParentWork[] parents,
            float stiffnessForceRate, float dragForceRate, float gravityRate,
            float windX, float windY, float windZ, float windStrength,
            bool bCollisionSwitch, float timescale, bool is60FPS)
        {
            if (b.IsSkip != 0) return;

            // 1) Verlet delta; halved at 30fps (0x1800097fd, 0x18008ec10) before anything reads it.
            Vector3 delta = b.PrevTargetPosition - b.TargetPosition;
            if (!is60FPS) delta = delta * 0.5f;
            b.PrevTargetPosition = b.TargetPosition;

            // 2) Aim. AnimationRotation is an input here, accumulated down the chain by the caller.
            Quaternion q = Qmul(b.ParentRotation, b.InitLocalRotation);
            b.AimVector = Qrot(q, b.BoneAxis);

            // 3) Forces. StiffnessForce / 1000 (0x18008ec8c), DragForce / 100 (0x18008ec80),
            // Gravity / 10000 (0x18008ec90), then each / 30 (0x180009aed-0x180009c49).
            float stiff = (b.StiffnessForce / 1000f) * stiffnessForceRate / FORCE_DIV;
            float drag  = (b.DragForce / 100f) * dragForceRate / FORCE_DIV;
            float windH = b.HorizontalWindRateSlow + (b.HorizontalWindRateFast - b.HorizontalWindRateSlow) * windStrength;
            float windV = b.VerticalWindRateSlow + (b.VerticalWindRateFast - b.VerticalWindRateSlow) * windStrength;
            float grav  = (gravityRate * b.Gravity / 10000f) / FORCE_DIV;
            Vector3 ef = b.ConnectedForce;
            b.Force = new Vector3(
                stiff * b.AimVector.x + drag * delta.x + windH * windX + ef.x / FORCE_DIV,
                stiff * b.AimVector.y + drag * delta.y + windV * windY - grav + ef.y / FORCE_DIV,
                stiff * b.AimVector.z + drag * delta.z + windH * windZ + ef.z / FORCE_DIV);

            // 4) Integrate (0x180009c59). 60fps: pos = Target - delta*T + Force*1.5*T, the delta term
            // in float (0x180009c67) and the rest in double. 30fps: everything in double,
            // pos = Target - 2*delta*T + Force*3.0*T (0x180009ccc; 0x18008ec30, 0x18008ec40).
            Vector3 pos;
            double td = timescale;
            if (is60FPS)
            {
                float fx = b.TargetPosition.x - delta.x * timescale;
                float fy = b.TargetPosition.y - delta.y * timescale;
                float fz = b.TargetPosition.z - delta.z * timescale;
                pos = new Vector3(
                    (float)(fx + (double)b.Force.x * 1.5 * td),
                    (float)(fy + (double)b.Force.y * 1.5 * td),
                    (float)(fz + (double)b.Force.z * 1.5 * td));
            }
            else
            {
                pos = new Vector3(
                    (float)((double)b.TargetPosition.x - ((double)delta.x + delta.x) * td + (double)b.Force.x * 3.0 * td),
                    (float)((double)b.TargetPosition.y - ((double)delta.y + delta.y) * td + (double)b.Force.y * 3.0 * td),
                    (float)((double)b.TargetPosition.z - ((double)delta.z + delta.z) * td + (double)b.Force.z * 3.0 * td));
            }

            // 5) Spring-apply blend toward the rest position SelfPosition + aim * InitBoneDistance
            // (0x180009d3e-0x180009e85): rate = IsAddSpring ? addMoveRate : moveRate, lerped
            // against MoveSpringApplyRate, times springRate, sqrt at 60fps, applied when < 1.
            float springApply = springRate;
            float rate = (b.IsAddSpring != 0) ? addMoveRate : moveRate;
            if (rate >= 0f)
                springApply = (b.MoveSpringApplyRate + (1f - b.MoveSpringApplyRate) * rate) * springApply;
            if (is60FPS) springApply = Mathf.Sqrt(springApply);
            if (springApply < 1f)
            {
                Vector3 rest = b.SelfPosition + Norm(b.AimVector) * b.InitBoneDistance;
                pos = rest + (pos - rest) * springApply;
            }

            // 6) Length constraint about the anchor.
            Vector3 anchor = b.SelfPosition;
            ConstrainLength(ref b, anchor, ref pos);

            // 7) Colliders (0x180009f48).
            if (bCollisionSwitch && b.ActiveCollision > 0) ResolveCollisions(ref b, col, parents, ref pos);

            // 8) Skirt-knee half-space (0x180009f69-0x18000a089): push out along SkirtKneeNormal
            // when within CollisionRadius of the plane through SkirtNormalPos, then re-constrain.
            if (b.IsCheckSkirtKnee != 0)
            {
                Vector3 n = b.SkirtKneeNormal;
                Vector3 d = pos - b.SkirtNormalPos;
                float dot = d.x * n.x + d.y * n.y + d.z * n.z;
                if (b.CollisionRadius > dot)
                {
                    pos = pos + n * (b.CollisionRadius - dot);
                    SkirtKneeHits++;
                    ConstrainLength(ref b, anchor, ref pos);
                }
            }

            // 9) Commit. Diff is the unit direction from the anchor (0x18000a10d); FinalRotation is
            // the swing from AimVector onto it, composed onto q.
            Vector3 aimTo = Norm(pos - anchor);
            b.Diff = aimTo;
            Quaternion final = Qmul(QFromTo(b.AimVector, aimTo), q);

            // 10) Rotation limit (IsLimit, 0x18000a17e). The position is then rebuilt from the
            // clamped rotation: SelfPosition + aim * InitBoneDistance (0x18000aa5a). Diff keeps the
            // pre-clamp direction.
            if (b.IsLimit != 0)
            {
                final = ApplyRotationLimit(ref b, q, final);
                pos = b.SelfPosition + Norm(Qrot(final, b.BoneAxis)) * b.InitBoneDistance;
            }
            b.FinalRotation = final;
            b.TargetPosition = pos;
        }

        // ------------------------------------------------------------------ entry points

        /// NativeClothUpdate export (0x180009100).
        public static void NativeClothUpdate(NativeClothWorking[] cond, int nCond,
            NativeClothCollision[] collisions, NativeRootParentWork[] parents,
            float stiffnessForceRate, float dragForceRate, float gravityRate,
            float windX, float windY, float windZ, float windStrength,
            bool bCollisionSwitch, float timescale, bool is60FPS,
            float moveRate, float addMoveRate, float springRateArg)
        {
            if (cond == null) return;
            if (nCond > cond.Length) nCond = cond.Length;
            springRate = springRateArg;
            CySpringSolver.moveRate = moveRate;
            CySpringSolver.addMoveRate = addMoveRate;
            for (int i = 0; i < nCond; i++) cond[i].IsCheckSkirtKnee = 0;   // 0x18000917d

            SolveChain(cond, nCond, collisions, parents, stiffnessForceRate, dragForceRate, gravityRate,
                windX, windY, windZ, windStrength, bCollisionSwitch, timescale, is60FPS);
        }

        /// Chain solve shared by both cloth entry points. Bone 0's AnimationRotation is seeded
        /// from rootParent[ParentWorkIndex].WorldRotation (0x1800091ac-0x1800091d2). Each later
        /// bone takes the previous bone's TargetPosition as SelfPosition, its FinalRotation as
        /// ParentRotation, and premultiplies its AnimationRotation by the previous bone's
        /// (0x1800093a1-0x1800093e5), then solves (0x180002c66).
        static void SolveChain(NativeClothWorking[] cond, int nCond,
            NativeClothCollision[] collisions, NativeRootParentWork[] parents,
            float stiffnessForceRate, float dragForceRate, float gravityRate,
            float windX, float windY, float windZ, float windStrength,
            bool bCollisionSwitch, float timescale, bool is60FPS)
        {
            for (int i = 0; i < nCond; i++)
            {
                if (i == 0)
                {
                    int pi = cond[0].ParentWorkIndex;
                    if (parents != null && pi >= 0 && pi < parents.Length)
                        cond[0].AnimationRotation = Qmul(parents[pi].WorldRotation, cond[0].AnimationRotation);
                }
                else
                {
                    cond[i].SelfPosition = cond[i - 1].TargetPosition;
                    cond[i].ParentRotation = cond[i - 1].FinalRotation;
                    cond[i].AnimationRotation = Qmul(cond[i - 1].AnimationRotation, cond[i].AnimationRotation);
                }

                SolveCloth(ref cond[i], collisions, parents, stiffnessForceRate, dragForceRate, gravityRate,
                    windX, windY, windZ, windStrength, bCollisionSwitch, timescale, is60FPS);
            }
        }

        /// NativeClothSkirtUpdate export (0x180008740). Clears IsCheckSkirtKnee, seeds Evaluation
        /// with -360 (0x1800087de), runs the skirt colliders, and if any fired (0x1800087f5)
        /// rotates SkirtInitNormal about RotationAxis by Evaluation - OffsetAngle and stamps that
        /// normal, SkirtRootPos and IsCheckSkirtKnee = 1 onto every bone (0x1800089c4-0x180008a81)
        /// before the chain solve.
        public static void NativeClothSkirtUpdate(NativeClothWorking[] cond, int nCond,
            NativeClothCollision[] collisions, NativeSkirtWorking[] skirt, int skirtIndex, ref NativeSkirtArg arg,
            NativeRootParentWork[] parents,
            float stiffnessForceRate, float dragForceRate, float gravityRate,
            float windX, float windY, float windZ, float windStrength,
            bool bCollisionSwitch, float timescale, bool is60FPS,
            float moveRate, float addMoveRate, float springRateArg)
        {
            if (cond == null) return;
            if (nCond > cond.Length) nCond = cond.Length;
            springRate = springRateArg;
            CySpringSolver.moveRate = moveRate;
            CySpringSolver.addMoveRate = addMoveRate;
            for (int i = 0; i < nCond; i++) cond[i].IsCheckSkirtKnee = 0;   // 0x1800087b0

            if (skirt != null && (uint)skirtIndex < (uint)skirt.Length)
            {
                ref NativeSkirtWorking w = ref skirt[skirtIndex];
                w.Evaluation = -360f;
                NativeSkirtUpdate(ref w, ref arg);
                if (w.Evaluation > -360f)
                {
                    Quaternion q = QAxisDeg(w.RotationAxis, w.Evaluation - w.OffsetAngle);
                    Vector3 n = Qrot(q, w.SkirtInitNormal);
                    for (int i = 0; i < nCond; i++)
                    {
                        cond[i].SkirtKneeNormal = n;
                        cond[i].SkirtNormalPos = w.SkirtRootPos;
                        cond[i].IsCheckSkirtKnee = 1;
                    }
                }
            }

            SolveChain(cond, nCond, collisions, parents, stiffnessForceRate, dragForceRate, gravityRate,
                windX, windY, windZ, windStrength, bCollisionSwitch, timescale, is60FPS);
        }

        /// NativeSkirtUpdate export (0x18000b190): four guarded collider passes, joints relative
        /// to RootPos, each accumulating into Evaluation by max. Does not seed Evaluation.
        public static void NativeSkirtUpdate(ref NativeSkirtWorking w, ref NativeSkirtArg a)
        {
            Vector3 root = a.RootPos;
            Vector3 center = a.CenterPos - root;
            if (w.IsCheckLeftKnee != 0)   ProcessSkirtCollider(ref w, a.KneeLPos - root,  center, root, a.KneeColliderRadius,  a.InfluenceAngle, a.InfluenceMaxAngle);
            if (w.IsCheckRightKnee != 0)  ProcessSkirtCollider(ref w, a.KneeRPos - root,  center, root, a.KneeColliderRadius,  a.InfluenceAngle, a.InfluenceMaxAngle);
            if (w.IsCheckLeftAnkle != 0)  ProcessSkirtCollider(ref w, a.AnkleLPos - root, center, root, a.AnkleColliderRadius, a.InfluenceAngle, a.InfluenceMaxAngle);
            if (w.IsCheckRightAnkle != 0) ProcessSkirtCollider(ref w, a.AnkleRPos - root, center, root, a.AnkleColliderRadius, a.InfluenceAngle, a.InfluenceMaxAngle);
        }

        /// One skirt collider (0x18000b670): the swing angle about RotationAxis, through the skirt
        /// root, that takes the rest child direction to just tangent to the joint sphere,
        /// weighted by how far around the body the swung child lands from the joint.
        static void ProcessSkirtCollider(ref NativeSkirtWorking w, Vector3 J, Vector3 center, Vector3 root,
            float r, float influenceAngle, float influenceMaxAngle)
        {
            Vector3 P = w.SkirtRootPos - root;                   // 0x18000b6ec-0x18000b73a
            Vector3 C = w.SkirtInitChildPos - root;
            Vector3 N = w.SkirtInitNormal;
            Vector3 A = w.RotationAxis;

            Vector3 D = J - P;
            float s = Vector3.Dot(J - C, N) + r;                 // 0x18000b751-0x18000b793
            float dLen = Len(D);
            float t = SafeSqrt(dLen * dLen - r * r);             // tangent length, 0x18000b7c8
            Vector3 T = Vector3.Cross(A, D);                     // 0x18000b821-0x18000b84d
            float tLen = Len(T);
            Vector3 E = C - P;
            float eLen = Len(E);
            Vector3 Eh = E / eLen;
            float ehLen = Len(Eh);

            float theta = 0f;
            if (ehLen >= FLT_EPS)                                // 0x18000b9e3
            {
                // Tangent point: K - P = Dhat*(t^2/|D|) + That*(r*t/|D|) (0x18000b7f5-0x18000b807).
                Vector3 KP = D * (t * t / (dLen * dLen)) + T * (r * t / (dLen * tLen));
                float proj = Vector3.Dot(KP, Eh);                // 0x18000b97c-0x18000b9a7
                Vector3 QP = Eh * proj + N * s;                  // 0x18000ba00-0x18000ba51
                float qLen = Len(QP);
                if (qLen >= FLT_EPS)                             // 0x18000baae
                {
                    double c = Vector3.Dot(QP, Eh) / ehLen / qLen;
                    if (c > 1.0) c = 1.0; else if (c < -1.0) c = -1.0;
                    theta = (float)(System.Math.Acos(c) * 360.0 / 6.283185307179586);   // 0x18000bae0
                }
            }

            // s < 0: negate and write the max directly (0x18000baf9-0x18000bb08).
            if (s < 0f)
            {
                theta = -theta;
                if (theta > w.Evaluation) w.Evaluation = theta;
                return;
            }

            // Swing the rest child by theta; angle at the body centre between it and the joint
            // (0x18000bb0d-0x18000be33).
            Quaternion q = QAxisDeg(A, theta);
            Vector3 Cp = P + Qrot(q, E);
            Vector3 a1 = Cp - center, a2 = J - center;
            double cphi = Vector3.Dot(a1, a2) / (Len(a1) * Len(a2));
            if (cphi > 1.0) cphi = 1.0; else if (cphi < -1.0) cphi = -1.0;
            float phi = (float)(System.Math.Acos(cphi) * 57.29578);

            if (phi > influenceMaxAngle)                         // 0x18000be4b
            {
                if (w.Evaluation < 0f) w.Evaluation = 0f;        // 0x18000be57
                return;
            }
            float wgt = 1f;
            if (phi > influenceAngle)                            // 0x18000be66: linear falloff to 0 at max
                wgt = 1f - (phi - influenceAngle) / (influenceMaxAngle - influenceAngle);
            theta *= wgt;
            if (theta > w.Evaluation) w.Evaluation = theta;      // 0x18000be92
        }
    }
}
