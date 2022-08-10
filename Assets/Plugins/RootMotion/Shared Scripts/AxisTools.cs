using UnityEngine;
using System.Collections;

namespace RootMotion {
	
	/// <summary>
	/// The Cartesian axes.
	/// </summary>
	[System.Serializable]
	public enum Axis {
		X,
		Y,
		Z
	}

	/// <summary>
	/// Contains tools for working with Axes that have no positive/negative directions.
	/// </summary>
	public class AxisTools {

		/// <summary>
		/// Converts an Axis to Vector3.
		/// </summary>
		public static Vector3 ToVector3(Axis axis) {
			if (axis == Axis.X) return Vector3.right;
			if (axis == Axis.Y) return Vector3.up;
			return Vector3.forward;
		}

		/// <summary>
		/// Converts a Vector3 to Axis.
		/// </summary>
		public static Axis ToAxis(Vector3 v) {
			float absX = Mathf.Abs(v.x);
			float absY = Mathf.Abs(v.y);
			float absZ = Mathf.Abs(v.z);
			
			Axis d = Axis.X;
			if (absY > absX && absY > absZ) d = Axis.Y;
			if (absZ > absX && absZ > absY) d = Axis.Z;
			return d;
		}

		/// <summary>
		/// Returns the Axis of the Transform towards a world space position.
		/// </summary>
		public static Axis GetAxisToPoint(Transform t, Vector3 worldPosition) {
			Vector3 axis = GetAxisVectorToPoint(t, worldPosition);
			if (axis == Vector3.right) return Axis.X;
			if (axis == Vector3.up) return Axis.Y;
			return Axis.Z;
		}

		/// <summary>
		/// Returns the Axis of the Transform towards a world space direction.
		/// </summary>
		public static Axis GetAxisToDirection(Transform t, Vector3 direction) {
			Vector3 axis = GetAxisVectorToDirection(t, direction);
			if (axis == Vector3.right) return Axis.X;
			if (axis == Vector3.up) return Axis.Y;
			return Axis.Z;
		}

		/// <summary>
		/// Returns the local axis of the Transform towards a world space position.
		/// </summary>
		public static Vector3 GetAxisVectorToPoint(Transform t, Vector3 worldPosition) {
			return GetAxisVectorToDirection(t, worldPosition - t.position);
		}

		/// <summary>
		/// Returns the local axis of the Transform that aligns the most with a direction.
		/// </summary>
		public static Vector3 GetAxisVectorToDirection(Transform t, Vector3 direction) {
            return GetAxisVectorToDirection(t.rotation, direction);
		}

        /// <summary>
		/// Returns the local axis of a rotation space that aligns the most with a direction.
		/// </summary>
		public static Vector3 GetAxisVectorToDirection(Quaternion r, Vector3 direction)
        {
            direction = direction.normalized;
            Vector3 axis = Vector3.right;

            float dotX = Mathf.Abs(Vector3.Dot(Vector3.Normalize(r * Vector3.right), direction));
            float dotY = Mathf.Abs(Vector3.Dot(Vector3.Normalize(r * Vector3.up), direction));
            if (dotY > dotX) axis = Vector3.up;
            float dotZ = Mathf.Abs(Vector3.Dot(Vector3.Normalize(r * Vector3.forward), direction));
            if (dotZ > dotX && dotZ > dotY) axis = Vector3.forward;

            return axis;
        }
    }
}
