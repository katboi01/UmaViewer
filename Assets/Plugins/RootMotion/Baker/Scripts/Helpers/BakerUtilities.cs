using UnityEngine;
using System.Collections;

namespace RootMotion
{
    public static class BakerUtilities
    {

        public static void ReduceKeyframes(AnimationCurve curve, float maxError)
        {
            if (maxError <= 0f) return;

            curve.keys = GetReducedKeyframes(curve, maxError);

            // TODO Flatten outTangent for keys that have the next key and testAfter sampled to the same value in the original clip. Same thing for the inTangent
        }

        public static Keyframe[] GetReducedKeyframes(AnimationCurve curve, float maxError)
        {
            Keyframe[] keys = curve.keys;

            int i = 1;
            while (i < keys.Length - 1 && keys.Length > 2)
            {
                Keyframe[] testKeys = new Keyframe[keys.Length - 1];
                int c = 0;
                for (int n = 0; n < keys.Length; n++)
                {
                    if (i != n)
                    {
                        testKeys[c] = new Keyframe(keys[n].time, keys[n].value, keys[n].inTangent, keys[n].outTangent);
                        c++;
                    }
                }

                AnimationCurve testCurve = new AnimationCurve();
                testCurve.keys = testKeys;

                float test0 = Mathf.Abs(testCurve.Evaluate(keys[i].time) - keys[i].value);
                float beforeTime = keys[i].time + (keys[i - 1].time - keys[i].time) * 0.5f;
                float afterTime = keys[i].time + (keys[i + 1].time - keys[i].time) * 0.5f;

                float testBefore = Mathf.Abs(testCurve.Evaluate(beforeTime) - curve.Evaluate(beforeTime));
                float testAfter = Mathf.Abs(testCurve.Evaluate(afterTime) - curve.Evaluate(afterTime));

                if (test0 < maxError && testBefore < maxError && testAfter < maxError)
                {
                    keys = testKeys;
                }
                else
                {
                    i++;
                }
            }

            return keys;
        }

        public static void SetLoopFrame(float time, AnimationCurve curve)
        {
            Keyframe[] keys = curve.keys;
            keys[keys.Length - 1].value = keys[0].value;

            float inTangent = Mathf.Lerp(keys[0].inTangent, keys[keys.Length - 1].inTangent, 0.5f);
            keys[0].inTangent = inTangent;
            keys[keys.Length - 1].inTangent = inTangent;

            float outTangent = Mathf.Lerp(keys[0].outTangent, keys[keys.Length - 1].outTangent, 0.5f);
            keys[0].outTangent = outTangent;
            keys[keys.Length - 1].outTangent = outTangent;

            keys[keys.Length - 1].time = time;
            curve.keys = keys;
        }

        public static void SetTangentMode(AnimationCurve curve)
        {
#if UNITY_EDITOR

            if (curve.length < 2) return;

            for (int i = 1; i < curve.length - 1; i++)
            {
                UnityEditor.AnimationUtility.SetKeyLeftTangentMode(curve, i, UnityEditor.AnimationUtility.TangentMode.ClampedAuto);
                UnityEditor.AnimationUtility.SetKeyRightTangentMode(curve, i, UnityEditor.AnimationUtility.TangentMode.ClampedAuto);
            }

#endif
        }

        // Realigns quaternion keys to ensure shortest interpolation paths.
        public static Quaternion EnsureQuaternionContinuity(Quaternion lastQ, Quaternion q)
        {
            Quaternion flipped = new Quaternion(-q.x, -q.y, -q.z, -q.w);

            Quaternion midQ = new Quaternion(
                Mathf.Lerp(lastQ.x, q.x, 0.5f),
                Mathf.Lerp(lastQ.y, q.y, 0.5f),
                Mathf.Lerp(lastQ.z, q.z, 0.5f),
                Mathf.Lerp(lastQ.w, q.w, 0.5f)
                );

            Quaternion midQFlipped = new Quaternion(
                Mathf.Lerp(lastQ.x, flipped.x, 0.5f),
                Mathf.Lerp(lastQ.y, flipped.y, 0.5f),
                Mathf.Lerp(lastQ.z, flipped.z, 0.5f),
                Mathf.Lerp(lastQ.w, flipped.w, 0.5f)
                );

            float angle = Quaternion.Angle(lastQ, midQ);
            float angleFlipped = Quaternion.Angle(lastQ, midQFlipped);

            return angleFlipped < angle ? flipped : q;
        }
    }

    //Manages the Animation Curves for Humanoid Q/T channels.
    [System.Serializable]
    public class BakerHumanoidQT
    {

        private Transform transform;
        private string Qx, Qy, Qz, Qw;
        private string Tx, Ty, Tz;

        // Animation curves for each channel of the Transform
        public AnimationCurve rotX, rotY, rotZ, rotW;
        public AnimationCurve posX, posY, posZ;

        private AvatarIKGoal goal;
        private Quaternion lastQ;
        private bool lastQSet;

        // The custom constructor
        public BakerHumanoidQT(string name)
        {
            Qx = name + "Q.x";
            Qy = name + "Q.y";
            Qz = name + "Q.z";
            Qw = name + "Q.w";

            Tx = name + "T.x";
            Ty = name + "T.y";
            Tz = name + "T.z";

            Reset();
        }

        public BakerHumanoidQT(Transform transform, AvatarIKGoal goal, string name)
        {
            this.transform = transform;
            this.goal = goal;

            Qx = name + "Q.x";
            Qy = name + "Q.y";
            Qz = name + "Q.z";
            Qw = name + "Q.w";

            Tx = name + "T.x";
            Ty = name + "T.y";
            Tz = name + "T.z";

            Reset();
        }

        // Clear all curves
        public void Reset()
        {
            rotX = new AnimationCurve();
            rotY = new AnimationCurve();
            rotZ = new AnimationCurve();
            rotW = new AnimationCurve();

            posX = new AnimationCurve();
            posY = new AnimationCurve();
            posZ = new AnimationCurve();

            lastQ = Quaternion.identity;
            lastQSet = false;
        }

        public void SetIKKeyframes(float time, Avatar avatar, float humanScale, Vector3 bodyPosition, Quaternion bodyRotation)
        {
            // TODO Use character scale
            TQ IKTQ = AvatarUtility.GetIKGoalTQ(avatar, humanScale, goal, new TQ(bodyPosition, bodyRotation), new TQ(transform.position, transform.rotation));

            Quaternion rot = IKTQ.q;
            if (lastQSet) rot = BakerUtilities.EnsureQuaternionContinuity(lastQ, IKTQ.q);

            //rot.Normalize();
            
            lastQ = rot;
            lastQSet = true;

            rotX.AddKey(time, rot.x);
            rotY.AddKey(time, rot.y);
            rotZ.AddKey(time, rot.z);
            rotW.AddKey(time, rot.w);

            Vector3 pos = IKTQ.t;
            posX.AddKey(time, pos.x);
            posY.AddKey(time, pos.y);
            posZ.AddKey(time, pos.z);
        }

        public void SetKeyframes(float time, Vector3 pos, Quaternion rot)
        {
            // Rotation flipping already prevented in HumanoidBaker.UpdateHumanPose().
            rotX.AddKey(time, rot.x);
            rotY.AddKey(time, rot.y);
            rotZ.AddKey(time, rot.z);
            rotW.AddKey(time, rot.w);

            posX.AddKey(time, pos.x);
            posY.AddKey(time, pos.y);
            posZ.AddKey(time, pos.z);
        }

        public void MoveLastKeyframes(float time)
        {
            MoveLastKeyframe(time, rotX);
            MoveLastKeyframe(time, rotY);
            MoveLastKeyframe(time, rotZ);
            MoveLastKeyframe(time, rotW);

            MoveLastKeyframe(time, posX);
            MoveLastKeyframe(time, posY);
            MoveLastKeyframe(time, posZ);
        }

        // Add a copy of the first frame to the specified time
        public void SetLoopFrame(float time)
        {
            BakerUtilities.SetLoopFrame(time, rotX);
            BakerUtilities.SetLoopFrame(time, rotY);
            BakerUtilities.SetLoopFrame(time, rotZ);
            BakerUtilities.SetLoopFrame(time, rotW);

            BakerUtilities.SetLoopFrame(time, posX);
            BakerUtilities.SetLoopFrame(time, posY);
            BakerUtilities.SetLoopFrame(time, posZ);
        }

        private void MoveLastKeyframe(float time, AnimationCurve curve)
        {
            Keyframe[] keys = curve.keys;
            keys[keys.Length - 1].time = time;
            curve.keys = keys;
        }

        public void MultiplyLength(AnimationCurve curve, float mlp)
        {
            Keyframe[] keys = curve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].time *= mlp;
            }
            curve.keys = keys;
        }

        // Add curves to the AnimationClip for each channel
        public void SetCurves(ref AnimationClip clip, float maxError, float lengthMlp)
        {
            MultiplyLength(rotX, lengthMlp);
            MultiplyLength(rotY, lengthMlp);
            MultiplyLength(rotZ, lengthMlp);
            MultiplyLength(rotW, lengthMlp);

            MultiplyLength(posX, lengthMlp);
            MultiplyLength(posY, lengthMlp);
            MultiplyLength(posZ, lengthMlp);

            BakerUtilities.ReduceKeyframes(rotX, maxError);
            BakerUtilities.ReduceKeyframes(rotY, maxError);
            BakerUtilities.ReduceKeyframes(rotZ, maxError);
            BakerUtilities.ReduceKeyframes(rotW, maxError);

            BakerUtilities.ReduceKeyframes(posX, maxError);
            BakerUtilities.ReduceKeyframes(posY, maxError);
            BakerUtilities.ReduceKeyframes(posZ, maxError);

            BakerUtilities.SetTangentMode(rotX);
            BakerUtilities.SetTangentMode(rotY);
            BakerUtilities.SetTangentMode(rotZ);
            BakerUtilities.SetTangentMode(rotW);

            /*
            BakerUtilities.SetTangentMode(posX);
            BakerUtilities.SetTangentMode(posY);
            BakerUtilities.SetTangentMode(posZ);
            */

            clip.SetCurve(string.Empty, typeof(Animator), Qx, rotX);
            clip.SetCurve(string.Empty, typeof(Animator), Qy, rotY);
            clip.SetCurve(string.Empty, typeof(Animator), Qz, rotZ);
            clip.SetCurve(string.Empty, typeof(Animator), Qw, rotW);

            clip.SetCurve(string.Empty, typeof(Animator), Tx, posX);
            clip.SetCurve(string.Empty, typeof(Animator), Ty, posY);
            clip.SetCurve(string.Empty, typeof(Animator), Tz, posZ);
        }
    }

    // Manages the Animation Curves for a single Transform that is a child of the root Transform.
    [System.Serializable]
    public class BakerMuscle
    {

        // Animation curves for each channel of the Transform
        public AnimationCurve curve;

        private int muscleIndex = -1;
        private string propertyName;

        // The custom constructor
        public BakerMuscle(int muscleIndex)
        {
            this.muscleIndex = muscleIndex;
            this.propertyName = MuscleNameToPropertyName(HumanTrait.MuscleName[muscleIndex]);

            Reset();
        }

        private string MuscleNameToPropertyName(string n)
        {
            // Left fingers
            if (n == "Left Index 1 Stretched") return "LeftHand.Index.1 Stretched";
            if (n == "Left Index 2 Stretched") return "LeftHand.Index.2 Stretched";
            if (n == "Left Index 3 Stretched") return "LeftHand.Index.3 Stretched";

            if (n == "Left Middle 1 Stretched") return "LeftHand.Middle.1 Stretched";
            if (n == "Left Middle 2 Stretched") return "LeftHand.Middle.2 Stretched";
            if (n == "Left Middle 3 Stretched") return "LeftHand.Middle.3 Stretched";

            if (n == "Left Ring 1 Stretched") return "LeftHand.Ring.1 Stretched";
            if (n == "Left Ring 2 Stretched") return "LeftHand.Ring.2 Stretched";
            if (n == "Left Ring 3 Stretched") return "LeftHand.Ring.3 Stretched";

            if (n == "Left Little 1 Stretched") return "LeftHand.Little.1 Stretched";
            if (n == "Left Little 2 Stretched") return "LeftHand.Little.2 Stretched";
            if (n == "Left Little 3 Stretched") return "LeftHand.Little.3 Stretched";

            if (n == "Left Thumb 1 Stretched") return "LeftHand.Thumb.1 Stretched";
            if (n == "Left Thumb 2 Stretched") return "LeftHand.Thumb.2 Stretched";
            if (n == "Left Thumb 3 Stretched") return "LeftHand.Thumb.3 Stretched";

            if (n == "Left Index Spread") return "LeftHand.Index.Spread";
            if (n == "Left Middle Spread") return "LeftHand.Middle.Spread";
            if (n == "Left Ring Spread") return "LeftHand.Ring.Spread";
            if (n == "Left Little Spread") return "LeftHand.Little.Spread";
            if (n == "Left Thumb Spread") return "LeftHand.Thumb.Spread";

            // Right fingers
            if (n == "Right Index 1 Stretched") return "RightHand.Index.1 Stretched";
            if (n == "Right Index 2 Stretched") return "RightHand.Index.2 Stretched";
            if (n == "Right Index 3 Stretched") return "RightHand.Index.3 Stretched";

            if (n == "Right Middle 1 Stretched") return "RightHand.Middle.1 Stretched";
            if (n == "Right Middle 2 Stretched") return "RightHand.Middle.2 Stretched";
            if (n == "Right Middle 3 Stretched") return "RightHand.Middle.3 Stretched";

            if (n == "Right Ring 1 Stretched") return "RightHand.Ring.1 Stretched";
            if (n == "Right Ring 2 Stretched") return "RightHand.Ring.2 Stretched";
            if (n == "Right Ring 3 Stretched") return "RightHand.Ring.3 Stretched";

            if (n == "Right Little 1 Stretched") return "RightHand.Little.1 Stretched";
            if (n == "Right Little 2 Stretched") return "RightHand.Little.2 Stretched";
            if (n == "Right Little 3 Stretched") return "RightHand.Little.3 Stretched";

            if (n == "Right Thumb 1 Stretched") return "RightHand.Thumb.1 Stretched";
            if (n == "Right Thumb 2 Stretched") return "RightHand.Thumb.2 Stretched";
            if (n == "Right Thumb 3 Stretched") return "RightHand.Thumb.3 Stretched";

            if (n == "Right Index Spread") return "RightHand.Index.Spread";
            if (n == "Right Middle Spread") return "RightHand.Middle.Spread";
            if (n == "Right Ring Spread") return "RightHand.Ring.Spread";
            if (n == "Right Little Spread") return "RightHand.Little.Spread";
            if (n == "Right Thumb Spread") return "RightHand.Thumb.Spread";

            return n;
        }

        public void MultiplyLength(AnimationCurve curve, float mlp)
        {
            Keyframe[] keys = curve.keys;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].time *= mlp;
            }
            curve.keys = keys;
        }

        // Add curves to the AnimationClip for each channel
        public void SetCurves(ref AnimationClip clip, float maxError, float lengthMlp)
        {
            MultiplyLength(curve, lengthMlp);
            BakerUtilities.ReduceKeyframes(curve, maxError);

            // BakerUtilities.SetTangentMode(curve);

            clip.SetCurve(string.Empty, typeof(Animator), propertyName, curve);
        }

        // Clear all curves
        public void Reset()
        {
            curve = new AnimationCurve();
        }

        // Record a keyframe for each channel
        public void SetKeyframe(float time, float[] muscles)
        {
            curve.AddKey(time, muscles[muscleIndex]);
        }

        // Add a copy of the first frame to the specified time
        public void SetLoopFrame(float time)
        {
            BakerUtilities.SetLoopFrame(time, curve);
        }
    }

    //Manages the Animation Curves for a single Transform that is a child of the root Transform.
    [System.Serializable]
    public class BakerTransform
    {

        public Transform transform; // The Transform component to record

        // Animation curves for each channel of the Transform
        public AnimationCurve
            posX, posY, posZ,
            rotX, rotY, rotZ, rotW;

        private string relativePath; // Path relative to the root
        private bool recordPosition; // Should we record the localPosition if the transform?
        private Vector3 relativePosition;
        private bool isRootNode;
        private Quaternion relativeRotation;

        // The custom constructor
        public BakerTransform(Transform transform, Transform root, bool recordPosition, bool isRootNode)
        {
            this.transform = transform;
            this.recordPosition = recordPosition || isRootNode;
            this.isRootNode = isRootNode;

            relativePath = string.Empty;
#if UNITY_EDITOR
            relativePath = UnityEditor.AnimationUtility.CalculateTransformPath(transform, root);
#endif

            Reset();
        }

        public void SetRelativeSpace(Vector3 position, Quaternion rotation)
        {
            relativePosition = position;
            relativeRotation = rotation;
        }

        // Add curves to the AnimationClip for each channel
        public void SetCurves(ref AnimationClip clip)
        {
            if (recordPosition)
            {
                clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", posX);
                clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", posY);
                clip.SetCurve(relativePath, typeof(Transform), "localPosition.z", posZ);
            }

            clip.SetCurve(relativePath, typeof(Transform), "localRotation.x", rotX);
            clip.SetCurve(relativePath, typeof(Transform), "localRotation.y", rotY);
            clip.SetCurve(relativePath, typeof(Transform), "localRotation.z", rotZ);
            clip.SetCurve(relativePath, typeof(Transform), "localRotation.w", rotW);

            if (isRootNode) AddRootMotionCurves(ref clip);

            // @todo probably only need to do it once for the clip
            clip.EnsureQuaternionContinuity(); // DOH!
        }

        private void AddRootMotionCurves(ref AnimationClip clip)
        {
            if (recordPosition)
            {
                clip.SetCurve("", typeof(Animator), "MotionT.x", posX);
                clip.SetCurve("", typeof(Animator), "MotionT.y", posY);
                clip.SetCurve("", typeof(Animator), "MotionT.z", posZ);
            }

            clip.SetCurve("", typeof(Animator), "MotionQ.x", rotX);
            clip.SetCurve("", typeof(Animator), "MotionQ.y", rotY);
            clip.SetCurve("", typeof(Animator), "MotionQ.z", rotZ);
            clip.SetCurve("", typeof(Animator), "MotionQ.w", rotW);
        }

        // Clear all curves
        public void Reset()
        {
            posX = new AnimationCurve();
            posY = new AnimationCurve();
            posZ = new AnimationCurve();

            rotX = new AnimationCurve();
            rotY = new AnimationCurve();
            rotZ = new AnimationCurve();
            rotW = new AnimationCurve();
        }

        public void ReduceKeyframes(float maxError)
        {
            BakerUtilities.ReduceKeyframes(rotX, maxError);
            BakerUtilities.ReduceKeyframes(rotY, maxError);
            BakerUtilities.ReduceKeyframes(rotZ, maxError);
            BakerUtilities.ReduceKeyframes(rotW, maxError);

            BakerUtilities.ReduceKeyframes(posX, maxError);
            BakerUtilities.ReduceKeyframes(posY, maxError);
            BakerUtilities.ReduceKeyframes(posZ, maxError);
        }

        // Record a keyframe for each channel
        public void SetKeyframes(float time)
        {
            if (recordPosition)
            {
                Vector3 pos = transform.localPosition;

                if (isRootNode)
                {
                    pos = transform.position - relativePosition;
                }

                posX.AddKey(time, pos.x);
                posY.AddKey(time, pos.y);
                posZ.AddKey(time, pos.z);
            }

            Quaternion rot = transform.localRotation;

            if (isRootNode)
            {
                rot = Quaternion.Inverse(relativeRotation) * transform.rotation;
            }

            rotX.AddKey(time, rot.x);
            rotY.AddKey(time, rot.y);
            rotZ.AddKey(time, rot.z);
            rotW.AddKey(time, rot.w);
        }

        // Add a copy of the first frame to the specified time
        public void AddLoopFrame(float time)
        {
            // TODO change to SetLoopFrame
            if (recordPosition && !isRootNode)
            {
                posX.AddKey(time, posX.keys[0].value);
                posY.AddKey(time, posY.keys[0].value);
                posZ.AddKey(time, posZ.keys[0].value);
            }

            rotX.AddKey(time, rotX.keys[0].value);
            rotY.AddKey(time, rotY.keys[0].value);
            rotZ.AddKey(time, rotZ.keys[0].value);
            rotW.AddKey(time, rotW.keys[0].value);
        }
    }
}

