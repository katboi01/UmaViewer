using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RootMotion.Dynamics {

	// Tools for symmetric operations
	public class SymmetryTools: Editor {

		public static Rigidbody GetSymmetric(Rigidbody r, Rigidbody[] R, Transform root) {
			Vector3 localPos = root.InverseTransformPoint(r.position);

			Rigidbody closest = null;
			float closestDistance = 0.01f;

			foreach (Rigidbody r2 in R) {
				if (r != r2) {
					Vector3 localPos2 = root.InverseTransformPoint(r2.position);
					localPos2.x = -localPos2.x;

					float dist = Vector3.Distance(localPos, localPos2);
					if (dist < closestDistance) {
						closest = r2;
						closestDistance = dist;
					}
				}
			}

			return closest;
		}

		public static Collider GetSymmetric(Collider c, Collider[] C, Transform root) {
			Vector3 localPos = root.InverseTransformPoint(c.transform.position);

			Collider closest = null;
			float closestDistance = 0.01f;

			foreach (Collider c2 in C) {
				if (c != c2) {
					Vector3 localPos2 = root.InverseTransformPoint(c2.transform.position);
					localPos2.x = -localPos2.x;

					float dist = Vector3.Distance(localPos, localPos2);
					if (dist < closestDistance) {
						closest = c2;
						closestDistance = dist;
					}
				}
			}

			return closest;
		}

		public static Vector3 MirrorDelta(Transform r, Transform s, Transform root, Vector3 delta) {
			Vector3 dXW = r.right * delta.x;
			Vector3 dYW = r.up * delta.y;
			Vector3 dZW = r.forward * delta.z;

			Vector3 dXWM = Mirror(dXW, root);
			Vector3 dYWM = Mirror(dYW, root);
			Vector3 dZWM = Mirror(dZW, root);

			Vector3 deltaS = Vector3.zero;
			deltaS = DeltaSize(deltaS, s, dXWM, delta.x);
			deltaS = DeltaSize(deltaS, s, dYWM, delta.y);
			deltaS = DeltaSize(deltaS, s, dZWM, delta.z);

			return deltaS;
		}

		public static Vector3 Mirror(Vector3 v, Transform root) {
			Vector3 local = root.InverseTransformVector(v);
			return root.TransformVector(-local.x, local.y, local.z);
		}

		public static Vector3 DeltaSize(Vector3 size, Transform t, Vector3 direction, float delta) {
			Axis axis = AxisTools.GetAxisToDirection(t, direction);

			switch(axis) {
			case Axis.X: size.x += delta; break;
			case Axis.Y: size.y += delta; break;
			case Axis.Z: size.z += delta; break;
			}

			return size;
		}

		public static Collider[] GetColliderPair(Collider r, Collider s) {
			if (s == null) return new Collider[1] {r};
			return new Collider[2] {r, s};
		}

	}
}
