using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Gallop
{
    /// Single-step differential between CySpringPlugin.dll and CySpringSolver.
    ///
    /// Every call runs the managed solver on a copy of the pre-step state, lets the plugin
    /// advance the real state, and compares the two outputs. Each residual is attributable to
    /// one step from identical input, and the simulation continues on the plugin's result.
    ///
    /// Fields compared, and the solve stage each one isolates:
    ///   AimVector          aim from the chained parent rotation
    ///   Force              stiffness / drag / wind / gravity
    ///   Diff               integrate, blend, constraint, collision
    ///   TargetPosition     committed position
    ///   FinalRotation      swing, rotation limit
    ///   AnimationRotation  chain accumulation
    public static class CySpringDiff
    {
        public static bool Armed;

        // --solver-diff-collide <scale>: scale every collider's Radius and force-enable it for the
        // duration of each call, for both solvers alike, so the collision path is exercised.
        // Whole-struct snapshot, restored afterwards.
        public static float CollideScale = 1f;

        // --solver-diff-collide-mode inner|plane|chara: synthesise collider kinds the rig may not
        // have. "inner" marks every sphere IsInner; "plane" turns every collider into a horizontal
        // Type 3 half-space at world y = 0.8; "chara" marks every collider IsCharaCollision and
        // clears CheckCharaCollision on every bone.
        public static string CollideMode = "";

        public static NativeClothCollision[] ScaleColliders(NativeClothCollision[] col)
        {
            if (!Armed || col == null || (CollideScale == 1f && string.IsNullOrEmpty(CollideMode))) return null;
            var saved = (NativeClothCollision[])col.Clone();
            for (int i = 0; i < col.Length; i++)
            {
                col[i].Radius = saved[i].Radius * CollideScale;
                col[i].IsEnable = 1;
                if (CollideMode == "inner" && col[i].Type == 0) col[i].IsInner = 1;
                else if (CollideMode == "chara") col[i].IsCharaCollision = 1;
                else if (CollideMode == "plane")
                {
                    col[i].Type = 3;
                    col[i].Normal = new Vector3(0f, 1f, 0f);
                    col[i].Distance = 0.8f;   // Normal/Distance are read untransformed, so this is a world height
                }
            }
            return saved;
        }

        public static void RestoreColliders(NativeClothCollision[] col, NativeClothCollision[] saved)
        {
            if (saved == null || col == null) return;
            System.Array.Copy(saved, col, System.Math.Min(saved.Length, col.Length));
        }

        public static int[] ScaleBones(NativeClothWorking[] cond, int n)
        {
            if (!Armed || cond == null || CollideMode != "chara") return null;
            n = System.Math.Min(n, cond.Length);
            var saved = new int[n];
            for (int i = 0; i < n; i++) { saved[i] = cond[i].CheckCharaCollision; cond[i].CheckCharaCollision = 0; }
            return saved;
        }

        public static void RestoreBones(NativeClothWorking[] cond, int[] saved)
        {
            if (saved == null || cond == null) return;
            for (int i = 0; i < saved.Length && i < cond.Length; i++) cond[i].CheckCharaCollision = saved[i];
        }

        const int FIELDS = 7;
        static readonly string[] FieldNames =
            { "AimVector", "Force", "Diff", "TargetPosition", "PrevTargetPosition", "FinalRotation",
              "AnimationRotation" };

        // Vector fields in world units; rotations as an angle in degrees.
        const float EPS = 1e-6f;
        const float EPS_ANGLE = 1e-2f;

        static float EpsFor(int field) => (field == 5 || field == 6) ? EPS_ANGLE : EPS;

        static readonly double[] maxDiff = new double[FIELDS];
        static readonly double[] sumDiff = new double[FIELDS];
        static readonly long[] countOver = new long[FIELDS];
        static readonly long[] samples = new long[FIELDS];
        static readonly int[] firstFrame = new int[FIELDS];
        static readonly int[] firstBone = new int[FIELDS];
        static readonly string[] worstWhere = new string[FIELDS];

        // Attributes of the diverging bone-steps.
        static long divLimit, divCollision, divSkip, divTotal, bonesSeen;
        static readonly long[] divWithParent = new long[FIELDS];
        static readonly long[] divNoParent = new long[FIELDS];
        static long seenWithParent, seenNoParent;

        // Integer state the solve writes.
        static long skirtNat, skirtMan, skirtBoth, skirtNeither, collMismatch;

        static int frame;

        // Skirt solver (NativeSkirtUpdate).
        static long skSamples, skOverEval, skOverAngle, skBranchMismatch;
        static double skMaxEval, skSumEval, skMaxAngle, skSumAngle;
        static string skWorst = "";
        static readonly StringBuilder skDetail = new StringBuilder();
        static int skDetailCount;

        // Full dumps: the first two divergences, and the worst one scored in multiples of each
        // field's own epsilon.
        const int MAX_DETAIL = 2;
        static readonly StringBuilder detail = new StringBuilder();
        static int detailCount;
        static readonly StringBuilder worstDetail = new StringBuilder();
        static double worstSeverity;
        static double curSeverity;

        public static void Reset()
        {
            for (int i = 0; i < FIELDS; i++)
            {
                maxDiff[i] = 0; sumDiff[i] = 0; countOver[i] = 0; samples[i] = 0;
                firstFrame[i] = -1; firstBone[i] = -1; worstWhere[i] = "";
                divWithParent[i] = 0; divNoParent[i] = 0;
            }
            divLimit = divCollision = divSkip = divTotal = bonesSeen = 0;
            seenWithParent = seenNoParent = 0;
            skirtNat = skirtMan = skirtBoth = skirtNeither = collMismatch = 0;
            skSamples = skOverEval = skOverAngle = skBranchMismatch = 0;
            skMaxEval = skSumEval = skMaxAngle = skSumAngle = 0; skWorst = ""; skDetail.Length = 0; skDetailCount = 0;
            detail.Length = 0; detailCount = 0;
            worstDetail.Length = 0; worstSeverity = 0; curSeverity = 0;
            frame = 0;
        }

        /// Value copy of the pre-step state (blittable struct array).
        public static NativeClothWorking[] Snapshot(NativeClothWorking[] src)
        {
            return (NativeClothWorking[])src.Clone();
        }

        static string QS(Quaternion q) => string.Format(CultureInfo.InvariantCulture,
            "({0:G9}, {1:G9}, {2:G9}, {3:G9})", q.x, q.y, q.z, q.w);
        static string VS(Vector3 v) => string.Format(CultureInfo.InvariantCulture,
            "({0:G9}, {1:G9}, {2:G9})", v.x, v.y, v.z);

        public static void Compare(NativeClothWorking[] nativeOut, NativeClothWorking[] managedOut,
                                   NativeClothWorking[] pre, int n)
        {
            if (nativeOut == null || managedOut == null) return;
            n = Mathf.Min(n, Mathf.Min(nativeOut.Length, managedOut.Length));
            for (int i = 0; i < n; i++)
            {
                var a = nativeOut[i];
                var b = managedOut[i];
                bool anyDiverged = false;
                bonesSeen++;
                curSeverity = 0;

                bool hasParent = a.ParentWorkIndex >= 0;
                if (hasParent) seenWithParent++; else seenNoParent++;

                bool sn = a.IsCheckSkirtKnee != 0, sm = b.IsCheckSkirtKnee != 0;
                if (sn && sm) skirtBoth++; else if (sn) skirtNat++; else if (sm) skirtMan++; else skirtNeither++;
                if (a.ActiveCollision != b.ActiveCollision) collMismatch++;

                anyDiverged |= TakeP(0, i, V(a.AimVector, b.AimVector), hasParent);
                anyDiverged |= TakeP(1, i, V(a.Force, b.Force), hasParent);
                anyDiverged |= TakeP(2, i, V(a.Diff, b.Diff), hasParent);
                anyDiverged |= TakeP(3, i, V(a.TargetPosition, b.TargetPosition), hasParent);
                anyDiverged |= TakeP(4, i, V(a.PrevTargetPosition, b.PrevTargetPosition), hasParent);
                anyDiverged |= TakeP(5, i, Q(a.FinalRotation, b.FinalRotation), hasParent);
                anyDiverged |= TakeP(6, i, Q(a.AnimationRotation, b.AnimationRotation), hasParent);

                if (anyDiverged && detailCount < MAX_DETAIL)
                {
                    detailCount++;
                    Describe(detail, $"--- divergence {detailCount}: frame {frame}, bone {i} of {n} ---", a, b, pre, i);
                }
                if (anyDiverged && curSeverity > worstSeverity)
                {
                    worstSeverity = curSeverity;
                    worstDetail.Length = 0;
                    Describe(worstDetail, $"--- worst: frame {frame}, bone {i} of {n} ({curSeverity:G4}x eps) ---", a, b, pre, i);
                }
                if (anyDiverged)
                {
                    divTotal++;
                    if (a.IsLimit != 0) divLimit++;
                    if (a.ActiveCollision != 0) divCollision++;
                    if (a.IsSkip != 0) divSkip++;
                }
            }
            frame++;
        }

        public static void CompareSkirt(NativeSkirtWorking pre, NativeSkirtWorking nat,
                                        NativeSkirtWorking man, NativeSkirtArg arg)
        {
            skSamples++;
            double de = System.Math.Abs((double)nat.Evaluation - man.Evaluation);
            double da = System.Math.Abs((double)nat.OffsetAngle - man.OffsetAngle);
            skSumEval += de; skSumAngle += da;
            if (de > EPS) skOverEval++;
            if (da > EPS) skOverAngle++;
            if (nat.IsCheckLeftKnee != man.IsCheckLeftKnee || nat.IsCheckRightKnee != man.IsCheckRightKnee
                || nat.IsCheckLeftAnkle != man.IsCheckLeftAnkle || nat.IsCheckRightAnkle != man.IsCheckRightAnkle)
                skBranchMismatch++;

            double sev = System.Math.Max(de, da);
            if (sev > skMaxEval + skMaxAngle || skDetailCount == 0)
            {
                skWorst = "sample " + skSamples;
                if (skDetailCount < 3)
                {
                    skDetailCount++;
                    skDetail.AppendLine($"--- skirt divergence {skDetailCount} (sample {skSamples}) ---");
                    skDetail.AppendLine($"  in  checks LK{pre.IsCheckLeftKnee} RK{pre.IsCheckRightKnee} LA{pre.IsCheckLeftAnkle} RA{pre.IsCheckRightAnkle}");
                    skDetail.AppendLine($"  in  SkirtRootPos      {VS(pre.SkirtRootPos)}");
                    skDetail.AppendLine($"  in  SkirtInitChildPos {VS(pre.SkirtInitChildPos)}");
                    skDetail.AppendLine($"  in  SkirtInitNormal   {VS(pre.SkirtInitNormal)}");
                    skDetail.AppendLine($"  in  RotationAxis      {VS(pre.RotationAxis)}");
                    skDetail.AppendLine($"  in  Evaluation {pre.Evaluation:G9}  OffsetAngle {pre.OffsetAngle:G9}");
                    skDetail.AppendLine($"  arg KneeL {VS(arg.KneeLPos)} KneeR {VS(arg.KneeRPos)}");
                    skDetail.AppendLine($"  arg AnkleL {VS(arg.AnkleLPos)} AnkleR {VS(arg.AnkleRPos)}");
                    skDetail.AppendLine($"  arg Center {VS(arg.CenterPos)} Root {VS(arg.RootPos)}");
                    skDetail.AppendLine($"  arg kneeR {arg.KneeColliderRadius:G9} ankleR {arg.AnkleColliderRadius:G9} infl {arg.InfluenceAngle:G9} inflMax {arg.InfluenceMaxAngle:G9}");
                    skDetail.AppendLine($"  nat Evaluation {nat.Evaluation:G9}   man Evaluation {man.Evaluation:G9}");
                    skDetail.AppendLine($"  nat OffsetAngle {nat.OffsetAngle:G9}  man OffsetAngle {man.OffsetAngle:G9}");
                }
            }
            if (de > skMaxEval) skMaxEval = de;
            if (da > skMaxAngle) skMaxAngle = da;
        }

        static void Describe(StringBuilder d, string header, NativeClothWorking a,
                             NativeClothWorking b, NativeClothWorking[] pre, int i)
        {
            d.AppendLine(header);
            if (pre != null && i < pre.Length)
            {
                var s0 = pre[i];
                d.AppendLine($"  in  ParentWorkIndex {s0.ParentWorkIndex}  IsLimit {s0.IsLimit}  ActiveCollision {s0.ActiveCollision}  IsCheckSkirtKnee {s0.IsCheckSkirtKnee}");
                d.AppendLine($"  in  InitLocalRotation {QS(s0.InitLocalRotation)}");
                d.AppendLine($"  in  ParentRotation    {QS(s0.ParentRotation)}");
                d.AppendLine($"  in  AnimationRotation {QS(s0.AnimationRotation)}");
                d.AppendLine($"  in  BoneAxis          {VS(s0.BoneAxis)}");
                d.AppendLine($"  in  TargetPosition    {VS(s0.TargetPosition)}");
                d.AppendLine($"  in  SelfPosition      {VS(s0.SelfPosition)}");
                d.AppendLine($"  in  PrevTargetPos     {VS(s0.PrevTargetPosition)}");
                d.AppendLine($"  in  InitBoneDistance  {s0.InitBoneDistance:G9}  CollisionRadius {s0.CollisionRadius:G9}");
                d.AppendLine($"  in  IsAddSpring {s0.IsAddSpring}  MoveSpringApplyRate {s0.MoveSpringApplyRate:G9}  DynamicRatio {s0.DynamicRatio:G9}");
                d.AppendLine($"  in  LimitRotMin      {VS(s0.LimitRotationMin)}");
                d.AppendLine($"  in  LimitRotMax      {VS(s0.LimitRotationMax)}");
                d.AppendLine($"  in  CIndex 0-7        {s0.CIndex0},{s0.CIndex1},{s0.CIndex2},{s0.CIndex3},{s0.CIndex4},{s0.CIndex5},{s0.CIndex6},{s0.CIndex7}");
            }
            d.AppendLine($"  nat AimVector         {VS(a.AimVector)}");
            d.AppendLine($"  man AimVector         {VS(b.AimVector)}");
            d.AppendLine($"  nat Force             {VS(a.Force)}");
            d.AppendLine($"  man Force             {VS(b.Force)}");
            d.AppendLine($"  nat AnimationRotation {QS(a.AnimationRotation)}");
            d.AppendLine($"  man AnimationRotation {QS(b.AnimationRotation)}");
            d.AppendLine($"  nat FinalRotation     {QS(a.FinalRotation)}");
            d.AppendLine($"  man FinalRotation     {QS(b.FinalRotation)}");
            d.AppendLine($"  nat Diff              {VS(a.Diff)}");
            d.AppendLine($"  man Diff              {VS(b.Diff)}");
            d.AppendLine($"  nat TargetPosition    {VS(a.TargetPosition)}");
            d.AppendLine($"  man TargetPosition    {VS(b.TargetPosition)}");
        }

        static bool TakeP(int f, int bone, double d, bool hasParent)
        {
            bool over = Take(f, bone, d);
            if (over) { if (hasParent) divWithParent[f]++; else divNoParent[f]++; }
            double sev = d / EpsFor(f);
            if (sev > curSeverity) curSeverity = sev;
            return over;
        }

        static bool Take(int f, int bone, double d)
        {
            samples[f]++;
            sumDiff[f] += d;
            if (d > maxDiff[f]) { maxDiff[f] = d; worstWhere[f] = "frame " + frame + " bone " + bone; }
            if (d > EpsFor(f))
            {
                countOver[f]++;
                if (firstFrame[f] < 0) { firstFrame[f] = frame; firstBone[f] = bone; }
                return true;
            }
            return false;
        }

        static double V(Vector3 a, Vector3 b)
        {
            double dx = a.x - b.x, dy = a.y - b.y, dz = a.z - b.z;
            return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// Angle between two rotations in degrees, sign-insensitive, via the chord: for unit
        /// quaternions |a - b| = 2 sin(t/4), so t = 4 asin(|a - b| / 2). asin is well conditioned
        /// near zero where acos(dot) is not.
        static double Q(Quaternion a, Quaternion b)
        {
            double sm = Chord(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
            double sp = Chord(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
            double d = System.Math.Min(sm, sp) * 0.5;
            if (d > 1.0) d = 1.0;
            return 4.0 * System.Math.Asin(d) * Mathf.Rad2Deg;
        }

        static double Chord(double x, double y, double z, double w)
            => System.Math.Sqrt(x * x + y * y + z * z + w * w);

        public static string Report()
        {
            var ci = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.AppendLine("CySpring native vs managed -- single-step differential");
            sb.AppendLine($"frames compared: {frame}   bone-steps: {bonesSeen}");
            sb.AppendLine();
            sb.AppendLine($"{"field",-20}{"max",14}{"mean",14}{"over eps",12}{"first",22}");
            for (int i = 0; i < FIELDS; i++)
            {
                if (samples[i] == 0) continue;
                string first = firstFrame[i] < 0 ? "-" : $"frame {firstFrame[i]} bone {firstBone[i]}";
                sb.AppendLine(string.Format(ci, "{0,-20}{1,14:G6}{2,14:G6}{3,12}{4,22}",
                    FieldNames[i], maxDiff[i], sumDiff[i] / samples[i], countOver[i], first));
            }
            sb.AppendLine();
            sb.AppendLine("worst case per field:");
            for (int i = 0; i < FIELDS; i++)
                if (samples[i] > 0 && maxDiff[i] > 0) sb.AppendLine($"  {FieldNames[i],-20}{worstWhere[i]}");
            sb.AppendLine();
            sb.AppendLine($"divergence split by root parent (bone-steps: {seenWithParent} with, {seenNoParent} without):");
            sb.AppendLine($"  {"field",-20}{"with parent",14}{"without",12}");
            for (int i = 0; i < FIELDS; i++)
            {
                if (samples[i] == 0) continue;
                sb.AppendLine($"  {FieldNames[i],-20}{divWithParent[i],14}{divNoParent[i],12}");
            }
            sb.AppendLine();
            sb.AppendLine("attributes of diverging bone-steps:");
            sb.AppendLine($"  diverged           : {divTotal} of {bonesSeen}");
            sb.AppendLine($"  of those IsLimit   : {divLimit}");
            sb.AppendLine($"  of those Collision : {divCollision}");
            sb.AppendLine($"  of those IsSkip    : {divSkip}");
            if (divTotal == 0) sb.AppendLine("\nno divergence above eps.");
            sb.AppendLine();
            sb.AppendLine("SKIRT solver (NativeSkirtUpdate):");
            sb.AppendLine($"  samples                    : {skSamples}");
            if (skSamples > 0)
            {
                sb.AppendLine(string.Format(ci, "  Evaluation  max {0:G6}  mean {1:G6}  over eps {2}", skMaxEval, skSumEval / skSamples, skOverEval));
                sb.AppendLine(string.Format(ci, "  OffsetAngle max {0:G6}  mean {1:G6}  over eps {2}", skMaxAngle, skSumAngle / skSamples, skOverAngle));
                sb.AppendLine($"  collider-branch mismatches : {skBranchMismatch}");
                sb.AppendLine($"  worst                      : {skWorst}");
            }
            if (skDetail.Length > 0) { sb.AppendLine(); sb.Append(skDetail); }

            sb.AppendLine();
            sb.AppendLine("integer state written during the solve:");
            sb.AppendLine($"  IsCheckSkirtKnee set by both      : {skirtBoth}");
            sb.AppendLine($"  IsCheckSkirtKnee set by PLUGIN only: {skirtNat}");
            sb.AppendLine($"  IsCheckSkirtKnee set by PORT only  : {skirtMan}");
            sb.AppendLine($"  IsCheckSkirtKnee set by neither    : {skirtNeither}");
            sb.AppendLine($"  ActiveCollision mismatches         : {collMismatch}");
            if (worstDetail.Length > 0) { sb.AppendLine(); sb.Append(worstDetail); }
            if (detail.Length > 0) { sb.AppendLine(); sb.Append(detail); }
            return sb.ToString();
        }

        public static void Write(string path)
        {
            try { File.WriteAllText(path, Report()); Debug.Log("CLI_EXPORT: wrote solver diff " + path); }
            catch (Exception ex) { Debug.LogError("CLI_EXPORT_FAIL: solver diff write threw: " + ex); }
        }
    }
}
