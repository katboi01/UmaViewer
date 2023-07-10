using UnityEngine;
using UnityEditor;
using System.Collections;
using RootMotion;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Custom inspector for the Aim Poser for visualizing pose range
	/// </summary>
	[CustomEditor(typeof(AimPoser))]
	public class AimPoserInspector : Editor {

		[System.Serializable]
		public struct ColorDirection {
			public Vector3 direction;
			public Vector3 color;
			public float dot;
			
			public ColorDirection(Vector3 direction, Vector3 color) {
				this.direction = direction.normalized;
				this.color = color;
				this.dot = 0;
			}
		}

		private AimPoser script { get { return target as AimPoser; }}
		private ColorDirection[] colorDirections;
		private static Vector3[] poly = new Vector3[36];

		void OnSceneGUI() {
			for (int i = 0; i < script.poses.Length; i++) {
				script.poses[i].yaw = Mathf.Clamp(script.poses[i].yaw, 0, 180);
				script.poses[i].pitch = Mathf.Clamp(script.poses[i].pitch, 0, 180);
			}

			if (colorDirections == null) {
				colorDirections = new ColorDirection[6] {
					new ColorDirection(Vector3.right, Vector3.right),
					new ColorDirection(Vector3.up, Vector3.up),
					new ColorDirection(Vector3.forward, Vector3.forward),
					new ColorDirection(Vector3.left, new Vector3(0f, 1f, 1f)),
					new ColorDirection(Vector3.down, new Vector3(1f, 0f, 1f)),
					new ColorDirection(Vector3.back, new Vector3(1f, 1f, 0f))
				};
			}

			for (int i = 0; i < script.poses.Length; i++) {
				if (script.poses[i].visualize) {
					DrawPose(script.poses[i], script.transform.position, script.transform.rotation, GetDirectionColor(script.poses[i].direction));
				}
			}
		}

		private Color GetDirectionColor(Vector3 localDirection) {
			localDirection = localDirection.normalized;

			// Calculating dot products for all AxisDirections
			for (int i = 0; i < colorDirections.Length; i++) {
				colorDirections[i].dot = Mathf.Clamp(Vector3.Dot(colorDirections[i].direction, localDirection), 0f, 1f);
			}
			
			// Summing up the arm bend axis
			Vector3 sum = Vector3.zero;
			
			for (int i = 0; i < colorDirections.Length; i++) {
				sum = Vector3.Lerp(sum, colorDirections[i].color, colorDirections[i].dot * colorDirections[i].dot);
			}

			return new Color(sum.x, sum.y, sum.z);
		}

		private static void DrawPose(AimPoser.Pose pose, Vector3 position, Quaternion rotation, Color color) {
			if (pose.pitch <= 0f || pose.yaw <= 0f) return;
			if (pose.direction == Vector3.zero) return;

			Handles.color = color;
			GUI.color = color;

			Vector3 up = rotation * Vector3.up;
			Vector3 normalizedPoseDirection = pose.direction.normalized;
			Vector3 direction = rotation * normalizedPoseDirection;
			
			// Direction and label
			Handles.DrawLine(position, position + direction);
			Inspector.ConeCap(0, position + direction, Quaternion.LookRotation(direction), 0.05f);
			Handles.Label(position + direction.normalized * 1.1f, pose.name);

			if (pose.yaw >= 180f && pose.pitch >= 180f) {
				Handles.color = Color.white;
				GUI.color = Color.white;
				return;
			}
		
			Quaternion halfYaw = Quaternion.AngleAxis(pose.yaw, up);

			float directionPitch = Vector3.Angle(up, direction);
			Vector3 crossRight = halfYaw * Vector3.Cross(up, direction);
			Vector3 crossLeft = Quaternion.Inverse(halfYaw) * Vector3.Cross(up, direction);

			bool isVertical = normalizedPoseDirection == Vector3.up || normalizedPoseDirection == Vector3.down;

			if (isVertical) {
				crossRight = halfYaw * Vector3.right;
				crossLeft = Quaternion.Inverse(halfYaw) * Vector3.right;
			}

			float minPitch = Mathf.Clamp(directionPitch - pose.pitch, 0f, 180f);
			float maxPitch = Mathf.Clamp(directionPitch + pose.pitch, 0f, 180f);

			Quaternion upToCornerUpperRight = Quaternion.AngleAxis(minPitch, crossRight);
			Quaternion upToCornerLowerRight = Quaternion.AngleAxis(maxPitch, crossRight);
			Quaternion upToCornerUpperLeft = Quaternion.AngleAxis(minPitch, crossLeft);
			Quaternion upToCornerLowerLeft = Quaternion.AngleAxis(maxPitch, crossLeft);

			Vector3 toCornerUpperRight = upToCornerUpperRight * up;
			Vector3 toCornerLowerRight = upToCornerLowerRight * up;
			Vector3 toCornerUpperLeft = upToCornerUpperLeft * up;
			Vector3 toCornerLowerLeft = upToCornerLowerLeft * up;

			if (pose.yaw < 180f) {
				Handles.DrawLine(position, position + toCornerUpperRight);
				Handles.DrawLine(position, position + toCornerUpperLeft);

				Handles.DrawLine(position, position + toCornerLowerRight);
				Handles.DrawLine(position, position + toCornerLowerLeft);
			}

			Vector3 d = direction;
			if (isVertical) d = Vector3.forward;

			if (pose.pitch < 180f) {
				DrawPolyLineOnSphere(position, toCornerUpperLeft, toCornerUpperRight, d, Vector3.up, color);
				DrawPolyLineOnSphere(position, toCornerLowerLeft, toCornerLowerRight, d, Vector3.up, color);
			}

			if (pose.yaw < 180f) {
				DrawPolyLineOnSphere(position, toCornerUpperLeft, toCornerLowerLeft, Quaternion.Inverse(halfYaw) * d, crossLeft, color);
				DrawPolyLineOnSphere(position, toCornerUpperRight, toCornerLowerRight, halfYaw * d, crossRight, color);
			}

			Handles.color = Color.white;
			GUI.color = Color.white;
		}

		private static void DrawPolyLineOnSphere(Vector3 center, Vector3 d1, Vector3 d2, Vector3 direction, Vector3 axis, Color color) {
			Handles.color = color;

			Vector3 normal = axis;
			Vector3 d1Ortho = d1;
			Vector3.OrthoNormalize(ref normal, ref d1Ortho);

			normal = axis;
			Vector3 d2Ortho = d2;
			Vector3.OrthoNormalize(ref normal, ref d2Ortho);

			normal = axis;
			Vector3 directionOrtho = direction;
			Vector3.OrthoNormalize(ref normal, ref directionOrtho);

			float angle = Vector3.Angle(d1Ortho, d2Ortho);

			float dot = Vector3.Dot(directionOrtho, d1Ortho);
			if (dot < 0) {
				angle = 180 + (180 - angle);
			}

			int segments = Mathf.Clamp(Mathf.RoundToInt(angle / 36f) * 5, 3, 36);

			float segmentF = angle / (float)(segments - 1);

			for (int i = 0; i < segments; i++) {
				poly[i] = center + Quaternion.AngleAxis(i * segmentF, axis) * d1;
			}

			Handles.color = new Color(color.r, color.g, color.b, color.a * 0.1f);

			for (int i = 0; i < segments; i++) {
				Handles.DrawLine(center, poly[i]);
			}

			Handles.color = color;

			for (int i = 0; i < segments - 1; i++) {
				Handles.DrawLine(poly[i], poly[i + 1]);
			}
		}
	}
}
