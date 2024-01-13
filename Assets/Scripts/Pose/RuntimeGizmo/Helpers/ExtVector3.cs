using System;
using UnityEngine;

namespace RuntimeGizmos
{
	public static class ExtVector3
	{
		public static float MagnitudeInDirection(Vector3 vector, Vector3 direction, bool normalizeParameters = true)
		{
			if(normalizeParameters) direction.Normalize();
			return Vector3.Dot(vector, direction);
		}

		public static Vector3 Abs(this Vector3 vector)
		{
			return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
		}

		public static bool IsParallel(Vector3 direction, Vector3 otherDirection, float precision = .0001f)
		{
			return Vector3.Cross(direction, otherDirection).sqrMagnitude < precision;
		}

		public static bool IsInDirection(Vector3 direction, Vector3 otherDirection)
		{
			return Vector3.Dot(direction, otherDirection) > 0f;
		}
	}
}