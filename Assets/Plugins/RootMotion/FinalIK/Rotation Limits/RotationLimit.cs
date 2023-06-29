using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK
{

    /// <summary>
    /// The base abstract class for all Rotation limits. Contains common functionality and static helper methods
    /// </summary>
    public abstract class RotationLimit : MonoBehaviour
    {

        #region Main Interface

        /// <summary>
        /// The main axis of the rotation limit.
        /// </summary>
        public Vector3 axis = Vector3.forward;

        /// <summary>
        /// Map the zero rotation point to the current local rotation of this gameobject.
        /// </summary>
        public void SetDefaultLocalRotation()
        {
            defaultLocalRotation = transform.localRotation;
            defaultLocalRotationSet = true;
            defaultLocalRotationOverride = false;
        }

        /// <summary>
		/// Map the zero rotation point to the specified rotation.
		/// </summary>
        public void SetDefaultLocalRotation(Quaternion localRotation)
        {
            defaultLocalRotation = localRotation;
            defaultLocalRotationSet = true;
            defaultLocalRotationOverride = true;
        }

        /// <summary>
        /// Returns the limited local rotation.
        /// </summary>
        public Quaternion GetLimitedLocalRotation(Quaternion localRotation, out bool changed)
        {
            // Making sure the Rotation Limit is initiated
            if (!initiated) Awake();

            // Subtracting defaultLocalRotation
            Quaternion rotation = Quaternion.Inverse(defaultLocalRotation) * localRotation;

            Quaternion limitedRotation = LimitRotation(rotation);
#if UNITY_2018_3_OR_NEWER
            limitedRotation = Quaternion.Normalize(limitedRotation);
#endif
            changed = limitedRotation != rotation;

            if (!changed) return localRotation;

            // Apply defaultLocalRotation back on
            return defaultLocalRotation * limitedRotation;
        }

        /// <summary>
        /// Apply the rotation limit to transform.localRotation. Returns true if the limit has changed the rotation.
        /// </summary>
        public bool Apply()
        {
            bool changed = false;

            transform.localRotation = GetLimitedLocalRotation(transform.localRotation, out changed);

            return changed;
        }

        /// <summary>
        /// Disable this instance making sure it is initiated. Use this if you intend to manually control the updating of this Rotation Limit.
        /// </summary>
        public void Disable()
        {
            if (initiated)
            {
                enabled = false;
                return;
            }

            Awake();
            enabled = false;
        }

#endregion Main Interface

        /*
		 * An arbitrary secondary axis that we get by simply switching the axes
		 * */
        public Vector3 secondaryAxis { get { return new Vector3(axis.y, axis.z, axis.x); } }

        /*
		 * Cross product of axis and secondaryAxis
		 * */
        public Vector3 crossAxis { get { return Vector3.Cross(axis, secondaryAxis); } }

        /*
		 * The default local rotation of the gameobject. By default stored in Awake.
		 * */
        [HideInInspector] public Quaternion defaultLocalRotation;

        public bool defaultLocalRotationOverride { get; private set; }

        protected abstract Quaternion LimitRotation(Quaternion rotation);

        private bool initiated;
        private bool applicationQuit;
        private bool defaultLocalRotationSet;

        /*
		 * Initiating the Rotation Limit
		 * */
        void Awake()
        {
            // Store the local rotation to map the zero rotation point to the current rotation
            if (!defaultLocalRotationSet) SetDefaultLocalRotation();

            if (axis == Vector3.zero) Debug.LogError("Axis is Vector3.zero.");
            initiated = true;
        }

        /*
		 * Using LateUpdate here because you most probably want to apply the limits after animation. 
		 * If you need precise control over the execution order, disable this script and call Apply() whenever you need
		 * */
        void LateUpdate()
        {
            Apply();
        }

        /*
		 * Logs the warning if no other warning has beed logged in this session.
		 * */
        public void LogWarning(string message)
        {
            Warning.Log(message, transform);
        }

#region Static helper methods for all Rotation Limits

        /*
		 * Limits rotation to a single degree of freedom (along axis)
		 * */
        protected static Quaternion Limit1DOF(Quaternion rotation, Vector3 axis)
        {
            return Quaternion.FromToRotation(rotation * axis, axis) * rotation;
        }

        /*
		 * Applies uniform twist limit to the rotation
		 * */
        protected static Quaternion LimitTwist(Quaternion rotation, Vector3 axis, Vector3 orthoAxis, float twistLimit)
        {
            twistLimit = Mathf.Clamp(twistLimit, 0, 180);
            if (twistLimit >= 180) return rotation;

            Vector3 normal = rotation * axis;
            Vector3 orthoTangent = orthoAxis;
            Vector3.OrthoNormalize(ref normal, ref orthoTangent);

            Vector3 rotatedOrthoTangent = rotation * orthoAxis;
            Vector3.OrthoNormalize(ref normal, ref rotatedOrthoTangent);

            Quaternion fixedRotation = Quaternion.FromToRotation(rotatedOrthoTangent, orthoTangent) * rotation;

            if (twistLimit <= 0) return fixedRotation;

            // Rotate from zero twist to free twist by the limited angle
            return Quaternion.RotateTowards(fixedRotation, rotation, twistLimit);
        }

        /*
		 * Returns the angle between two vectors on a plane with the specified normal
		 * */
        protected static float GetOrthogonalAngle(Vector3 v1, Vector3 v2, Vector3 normal)
        {
            Vector3.OrthoNormalize(ref normal, ref v1);
            Vector3.OrthoNormalize(ref normal, ref v2);
            return Vector3.Angle(v1, v2);
        }

#endregion
    }
}
