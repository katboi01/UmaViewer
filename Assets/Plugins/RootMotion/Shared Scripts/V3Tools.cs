using UnityEngine;
using System.Collections;

namespace RootMotion {
	
	/// <summary>
	/// Helper methods for dealing with 3-dimensional vectors.
	/// </summary>
	public static class V3Tools {

		/// <summary>
		/// Optimized Vector3.Lerp
		/// </summary>
		public static Vector3 Lerp(Vector3 fromVector, Vector3 toVector, float weight) {
			if (weight <= 0f) return fromVector;
			if (weight >= 1f) return toVector;

			return Vector3.Lerp(fromVector, toVector, weight);
		}

		/// <summary>
		/// Optimized Vector3.Slerp
		/// </summary>
		public static Vector3 Slerp(Vector3 fromVector, Vector3 toVector, float weight) {
			if (weight <= 0f) return fromVector;
			if (weight >= 1f) return toVector;

			return Vector3.Slerp(fromVector, toVector, weight);
		}

		/// <summary>
		/// Returns vector projection on axis multiplied by weight.
		/// </summary>
		public static Vector3 ExtractVertical(Vector3 v, Vector3 verticalAxis, float weight) {
			if (weight == 0f) return Vector3.zero;
			return Vector3.Project(v, verticalAxis) * weight;
		}

		/// <summary>
		/// Returns vector projected to a plane and multiplied by weight.
		/// </summary>
		public static Vector3 ExtractHorizontal(Vector3 v, Vector3 normal, float weight) {
			if (weight == 0f) return Vector3.zero;
			
			Vector3 tangent = v;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return Vector3.Project(v, tangent) * weight;
		}

        /// <summary>
        /// Clamps the direction to clampWeight from normalDirection, clampSmoothing is the number of sine smoothing iterations applied on the result.
        /// </summary>
        public static Vector3 ClampDirection(Vector3 direction, Vector3 normalDirection, float clampWeight, int clampSmoothing)
        {
            if (clampWeight <= 0) return direction;

            if (clampWeight >= 1f) return normalDirection;

            // Getting the angle between direction and normalDirection
            float angle = Vector3.Angle(normalDirection, direction);
            float dot = 1f - (angle / 180f);

            if (dot > clampWeight) return direction;
           
            // Clamping the target
            float targetClampMlp = clampWeight > 0 ? Mathf.Clamp(1f - ((clampWeight - dot) / (1f - dot)), 0f, 1f) : 1f;

            // Calculating the clamp multiplier
            float clampMlp = clampWeight > 0 ? Mathf.Clamp(dot / clampWeight, 0f, 1f) : 1f;

            // Sine smoothing iterations
            for (int i = 0; i < clampSmoothing; i++)
            {
                float sinF = clampMlp * Mathf.PI * 0.5f;
                clampMlp = Mathf.Sin(sinF);
            }

            // Slerping the direction (don't use Lerp here, it breaks it)
            return Vector3.Slerp(normalDirection, direction, clampMlp * targetClampMlp);
        }

        /// <summary>
        /// Clamps the direction to clampWeight from normalDirection, clampSmoothing is the number of sine smoothing iterations applied on the result.
        /// </summary>
        public static Vector3 ClampDirection(Vector3 direction, Vector3 normalDirection, float clampWeight, int clampSmoothing, out bool changed) {
			changed = false;

			if (clampWeight <= 0) return direction;

			if (clampWeight >= 1f) {
				changed = true;
				return normalDirection;
			}
			
			// Getting the angle between direction and normalDirection
			float angle = Vector3.Angle(normalDirection, direction);
			float dot = 1f - (angle / 180f);

			if (dot > clampWeight) return direction;
			changed = true;
			
			// Clamping the target
			float targetClampMlp = clampWeight > 0? Mathf.Clamp(1f - ((clampWeight - dot) / (1f - dot)), 0f, 1f): 1f;
			
			// Calculating the clamp multiplier
			float clampMlp = clampWeight > 0? Mathf.Clamp(dot / clampWeight, 0f, 1f): 1f;
			
			// Sine smoothing iterations
			for (int i = 0; i < clampSmoothing; i++) {
				float sinF = clampMlp * Mathf.PI * 0.5f;
				clampMlp = Mathf.Sin(sinF);
			}
			
			// Slerping the direction (don't use Lerp here, it breaks it)
			return Vector3.Slerp(normalDirection, direction, clampMlp * targetClampMlp);
		}

		/// <summary>
		/// Clamps the direction to clampWeight from normalDirection, clampSmoothing is the number of sine smoothing iterations applied on the result.
		/// </summary>
		public static Vector3 ClampDirection(Vector3 direction, Vector3 normalDirection, float clampWeight, int clampSmoothing, out float clampValue) {
			clampValue = 1f;
			
			if (clampWeight <= 0) return direction;
			
			if (clampWeight >= 1f) {
				return normalDirection;
			}
			
			// Getting the angle between direction and normalDirection
			float angle = Vector3.Angle(normalDirection, direction);
			float dot = 1f - (angle / 180f);
			
			if (dot > clampWeight) {
				clampValue = 0f;
				return direction;
			}

			// Clamping the target
			float targetClampMlp = clampWeight > 0? Mathf.Clamp(1f - ((clampWeight - dot) / (1f - dot)), 0f, 1f): 1f;
			
			// Calculating the clamp multiplier
			float clampMlp = clampWeight > 0? Mathf.Clamp(dot / clampWeight, 0f, 1f): 1f;
			
			// Sine smoothing iterations
			for (int i = 0; i < clampSmoothing; i++) {
				float sinF = clampMlp * Mathf.PI * 0.5f;
				clampMlp = Mathf.Sin(sinF);
			}
			
			// Slerping the direction (don't use Lerp here, it breaks it)
			float slerp = clampMlp * targetClampMlp;
			clampValue = 1f - slerp;
			return Vector3.Slerp(normalDirection, direction, slerp);
		}

		/// <summary>
		/// Get the intersection point of line and plane
		/// </summary>
		public static Vector3 LineToPlane(Vector3 origin, Vector3 direction, Vector3 planeNormal, Vector3 planePoint) {
			float dot = Vector3.Dot(planePoint - origin, planeNormal);
			float normalDot = Vector3.Dot(direction, planeNormal);
			
			if (normalDot == 0.0f) return Vector3.zero;
			
			float dist = dot / normalDot;
			return origin + direction.normalized * dist;
		}

		/// <summary>
		/// Projects a point to a plane.
		/// </summary>
		public static Vector3 PointToPlane(Vector3 point, Vector3 planePosition, Vector3 planeNormal) {
			if (planeNormal == Vector3.up) {
				return new Vector3(point.x, planePosition.y, point.z);
			}

			Vector3 tangent = point - planePosition;
			Vector3 normal = planeNormal;
			Vector3.OrthoNormalize(ref normal, ref tangent);

			return planePosition + Vector3.Project(point - planePosition, tangent);
		}

        /// <summary>
        /// Same as Transform.TransformPoint(), but not using scale.
        /// </summary>
        public static Vector3 TransformPointUnscaled(Transform t, Vector3 point)
        {
            return t.position + t.rotation * point;
        }

        /// <summary>
        /// Same as Transform.InverseTransformPoint(), but not using scale.
        /// </summary>
        public static Vector3 InverseTransformPointUnscaled(Transform t, Vector3 point)
        {
            return Quaternion.Inverse(t.rotation) * (point - t.position);
        }
	}
}
