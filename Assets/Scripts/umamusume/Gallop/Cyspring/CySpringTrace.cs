using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace Gallop
{
    /// Per-bone, per-call trace of the CySpring solve, inputs and outputs, for comparing two
    /// free-running runs (plugin and managed) call-for-call. Rows are aligned by call sequence
    /// number; the spring groups are static, so the Nth call of one run is the Nth of the other.
    /// A row whose inputs match while its outputs differ localises a difference to that call.
    public static class CySpringTrace
    {
        public static bool Armed;

        static readonly StringBuilder sb = new StringBuilder();
        static int callSeq;
        static bool header;

        public static void Reset() { sb.Length = 0; callSeq = 0; header = false; }

        static readonly CultureInfo CI = CultureInfo.InvariantCulture;
        static void F(float v) { sb.Append(v.ToString("G9", CI)); sb.Append(','); }
        static void V(Vector3 v) { F(v.x); F(v.y); F(v.z); }
        static void Q(Quaternion q) { F(q.x); F(q.y); F(q.z); F(q.w); }

        public static void Record(NativeClothWorking[] pre, NativeClothWorking[] post, int n)
        {
            if (!Armed || post == null) return;
            if (!header)
            {
                header = true;
                sb.Append("call,bone,n,isLimit,activeColl,isSkip,");
                sb.Append("inParentRot.x,inParentRot.y,inParentRot.z,inParentRot.w,");
                sb.Append("inInitLocal.x,inInitLocal.y,inInitLocal.z,inInitLocal.w,");
                sb.Append("inAnimRot.x,inAnimRot.y,inAnimRot.z,inAnimRot.w,");
                sb.Append("inTarget.x,inTarget.y,inTarget.z,");
                sb.Append("inPrev.x,inPrev.y,inPrev.z,");
                sb.Append("inSelf.x,inSelf.y,inSelf.z,");
                sb.Append("inAxis.x,inAxis.y,inAxis.z,");
                sb.Append("limMin.x,limMin.y,limMin.z,limMax.x,limMax.y,limMax.z,");
                sb.Append("outAim.x,outAim.y,outAim.z,");
                sb.Append("outForce.x,outForce.y,outForce.z,");
                sb.Append("outDiff.x,outDiff.y,outDiff.z,");
                sb.Append("outTarget.x,outTarget.y,outTarget.z,");
                sb.Append("outPrev.x,outPrev.y,outPrev.z,");
                sb.Append("outFinal.x,outFinal.y,outFinal.z,outFinal.w,");
                sb.Append("outAnim.x,outAnim.y,outAnim.z,outAnim.w");
                sb.Append(Environment.NewLine);
            }

            int c = callSeq++;
            int m = Mathf.Min(n, post.Length);
            for (int i = 0; i < m; i++)
            {
                var s0 = (pre != null && i < pre.Length) ? pre[i] : post[i];
                var a = post[i];
                sb.Append(c); sb.Append(','); sb.Append(i); sb.Append(','); sb.Append(m); sb.Append(',');
                sb.Append(s0.IsLimit); sb.Append(','); sb.Append(s0.ActiveCollision); sb.Append(',');
                sb.Append(s0.IsSkip); sb.Append(',');
                Q(s0.ParentRotation); Q(s0.InitLocalRotation); Q(s0.AnimationRotation);
                V(s0.TargetPosition); V(s0.PrevTargetPosition); V(s0.SelfPosition); V(s0.BoneAxis);
                V(s0.LimitRotationMin); V(s0.LimitRotationMax);
                V(a.AimVector); V(a.Force); V(a.Diff); V(a.TargetPosition); V(a.PrevTargetPosition);
                Q(a.FinalRotation);
                // last field, no trailing comma
                sb.Append(a.AnimationRotation.x.ToString("G9", CI)); sb.Append(',');
                sb.Append(a.AnimationRotation.y.ToString("G9", CI)); sb.Append(',');
                sb.Append(a.AnimationRotation.z.ToString("G9", CI)); sb.Append(',');
                sb.Append(a.AnimationRotation.w.ToString("G9", CI));
                sb.Append(Environment.NewLine);
            }
        }

        public static void Write(string path)
        {
            try
            {
                File.WriteAllText(path, sb.ToString());
                Debug.Log($"CLI_EXPORT: wrote solver trace {path} ({callSeq} calls)");
            }
            catch (Exception ex) { Debug.LogError("CLI_EXPORT_FAIL: solver trace write threw: " + ex); }
        }
    }
}
