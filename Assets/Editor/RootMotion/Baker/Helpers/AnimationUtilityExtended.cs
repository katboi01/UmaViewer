using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RootMotion
{
    public static class AnimationUtilityExtended
    {

        /// <summary>
        /// Copies the curves with the specified property names from clipFrom to clipTo.
        /// </summary>
        /// <param name="fromClip">copy from clip.</param>
        /// <param name="toClip">paste to clip</param>
        /// <param name="propertyNames">Property names ("Root.T", "Root.Q", "LeftFoot.T"...).</param>
        public static void CopyCurves(AnimationClip fromClip, AnimationClip toClip, string[] propertyNames)
        {
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(fromClip);

            for (int i = 0; i < bindings.Length; i++)
            {
                for (int n = 0; n < propertyNames.Length; n++)
                {
                    if (bindings[i].propertyName == propertyNames[n])
                    {
                        CopyCurve(fromClip, toClip, bindings[i]);
                    }
                }
            }
        }

        public static void CopyCurve(AnimationClip fromClip, AnimationClip toClip, EditorCurveBinding binding)
        {
            AnimationCurve curve = AnimationUtility.GetEditorCurve(fromClip, binding);
            toClip.SetCurve(string.Empty, typeof(Animator), binding.propertyName, curve);
        }
    }
}
