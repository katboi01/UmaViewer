using UnityEngine;
using System.Collections;
using UnityEditor;

namespace RootMotion.Dynamics {

	// Tools for editing Colliders.
	public class ColliderTools: Editor {

		public static void ConvertToSphereCollider(CapsuleCollider capsule) {
			SphereCollider sphere = Undo.AddComponent(capsule.gameObject, typeof(SphereCollider)) as SphereCollider;

			sphere.center = capsule.center;
			sphere.radius = capsule.radius;

			Undo.DestroyObjectImmediate(capsule);
		}

		public static void ConvertToSphereCollider(BoxCollider box) {
			SphereCollider sphere = Undo.AddComponent(box.gameObject, typeof(SphereCollider)) as SphereCollider;

			sphere.center = box.center;
			sphere.radius = Mathf.Max(new float[3] { box.size.x, box.size.y, box.size.z }) * 0.5f;

			Undo.DestroyObjectImmediate(box);
		}

		public static void ConvertToBoxCollider(SphereCollider sphere) {
			BoxCollider box = Undo.AddComponent(sphere.gameObject, typeof(BoxCollider)) as BoxCollider;

			box.center = sphere.center;
			box.size = Vector3.one * sphere.radius * 2f;

			Undo.DestroyObjectImmediate(sphere);
		}

		public static void ConvertToCapsuleCollider(SphereCollider sphere) {
			CapsuleCollider capsule = Undo.AddComponent(sphere.gameObject, typeof(CapsuleCollider)) as CapsuleCollider;

			capsule.center = sphere.center;
			capsule.radius = sphere.radius;
			capsule.height = sphere.radius;

			Undo.DestroyObjectImmediate(sphere);
		}

		public static void ConvertToBoxCollider(CapsuleCollider capsule) {
			BoxCollider box = Undo.AddComponent(capsule.gameObject, typeof(BoxCollider)) as BoxCollider;

			box.center = capsule.center;

			float diameter = capsule.radius * 2f;
			float height = Mathf.Max(diameter, capsule.height);

			if (capsule.direction == 0) box.size = new Vector3(height, diameter, diameter);
			else if (capsule.direction == 1) box.size = new Vector3(diameter, height, diameter);
			else if (capsule.direction == 2) box.size = new Vector3(diameter, diameter, height);

			Undo.DestroyObjectImmediate(capsule);
		}

		public static void ConvertToCapsuleCollider(BoxCollider box) {
			float absX = Mathf.Abs(box.size.x);
			float absY = Mathf.Abs(box.size.y);
			float absZ = Mathf.Abs(box.size.z);

			CapsuleCollider capsule = Undo.AddComponent(box.gameObject, typeof(CapsuleCollider)) as CapsuleCollider;

			capsule.center = box.center;

			capsule.direction = 0;
			if (absY > absX && absY > absZ) capsule.direction = 1;
			if (absZ > absX && absZ > absY) capsule.direction = 2;

			if (capsule.direction == 0) {
				capsule.height = absX;
				capsule.radius = Mathf.Max(absY, absZ) * 0.5f;
			} else if (capsule.direction == 1) {
				capsule.height = absY;
				capsule.radius = Mathf.Max(absX, absZ) * 0.5f;
			} else if (capsule.direction == 2) {
				capsule.height = absZ;
				capsule.radius = Mathf.Max(absX, absY) * 0.5f;
			}

			Undo.DestroyObjectImmediate(box);
		}

		public static void SetColliderCenterWorld(Collider r, Collider s, Vector3 value, Transform root) {
			Vector3 newCenter = r.transform.InverseTransformPoint(value);
			Vector3 centerWorld = Vector3.zero;
			Vector3 deltaWorld = Vector3.zero;

			if (r is BoxCollider) {
				var box = r as BoxCollider;
				centerWorld = r.transform.TransformPoint(box.center);
				deltaWorld = value - centerWorld;
				box.center = newCenter;

				if (s != null && s is BoxCollider) s.transform.GetComponent<BoxCollider>().center += s.transform.InverseTransformVector(SymmetryTools.Mirror(deltaWorld, root));
				return;
			}

			if (r is CapsuleCollider) {
				var capsule = r as CapsuleCollider;
				centerWorld = r.transform.TransformPoint(capsule.center);
				deltaWorld = value - centerWorld;
				capsule.center = newCenter;

				if (s != null && s is CapsuleCollider) s.transform.GetComponent<CapsuleCollider>().center += s.transform.InverseTransformVector(SymmetryTools.Mirror(deltaWorld, root));
				return;
			}

			if (r is SphereCollider) {
				var sphere = r as SphereCollider;
				centerWorld = r.transform.TransformPoint(sphere.center);
				deltaWorld = value - centerWorld;
				sphere.center = newCenter;

				if (s != null && s is SphereCollider) s.transform.GetComponent<SphereCollider>().center += s.transform.InverseTransformVector(SymmetryTools.Mirror(deltaWorld, root));
				return;
			}
		}

		public static Vector3 GetColliderCenterWorld(Collider r) {
			if (r == null) return Vector3.zero;

			if (r is BoxCollider) {
				return r.transform.TransformPoint((r as BoxCollider).center);
			}

			if (r is CapsuleCollider) {
				return r.transform.TransformPoint((r as CapsuleCollider).center);
			}

			if (r is SphereCollider) {
				return r.transform.TransformPoint((r as SphereCollider).center);
			}

			return r.transform.position;
		}

		public static void SetColliderSize(Collider r, Collider s, Vector3 value, Vector3 lastValue, Vector3 lastValueS, Transform root) {
			Vector3 delta = Vector3.zero;
			Vector3 deltaM = Vector3.zero;

			if (r is BoxCollider) {
				var box = r as BoxCollider;
				delta = value - box.size;
				box.size = value;

				if (s != null && s is BoxCollider) {
					var boxS = s as BoxCollider;

					deltaM = SymmetryTools.MirrorDelta(r.transform, s.transform, root, delta);
					boxS.size += deltaM;
				}
			}

			if (r is CapsuleCollider) {
				var capsule = r as CapsuleCollider;

				delta = value - GetCapsuleSize(capsule);
				SetCapsuleSize(capsule, value, lastValue);

				if (s != null && s is CapsuleCollider) {
					var capsuleS = s as CapsuleCollider;

					if (capsuleS != null) {
						deltaM = SymmetryTools.MirrorDelta(r.transform, s.transform, root, delta);
						Vector3 capsuleSSize = GetCapsuleSize(capsuleS);
						capsuleSSize += deltaM;

						SetCapsuleSize(capsuleS, capsuleSSize, lastValueS);
					}
				}
			}

			if (r is SphereCollider) {
				var sphere = r as SphereCollider;

				delta = value - GetColliderSize(r);
				SetSphereSize(sphere, value, lastValue);

				if (s != null && s is SphereCollider) {
					var sphereS = s as SphereCollider;
					Vector3 sphereSSize = GetColliderSize(s);
					sphereSSize += delta;

					if (sphereS != null) {
						SetSphereSize(sphereS, sphereSSize, lastValueS);
					}
				}
			}
		}

		public static Vector3 GetColliderSize(Collider r) {
			if (r == null) return Vector3.zero;

			if (r is BoxCollider) return (r as BoxCollider).size;
			if (r is CapsuleCollider) return GetCapsuleSize(r as CapsuleCollider);
			if (r is SphereCollider) return Vector3.one * (r as SphereCollider).radius;

			return r.transform.localScale;
		}

		public static void SetCapsuleSize(CapsuleCollider capsule, Vector3 size, Vector3 lastSize) {
			if (capsule.direction == 0) {
				capsule.height = size.x;

				float absY = Mathf.Abs(size.y - lastSize.y);
				float absZ = Mathf.Abs(size.z - lastSize.z);
				capsule.radius = absY > absZ? size.y: size.z;
			}

			if (capsule.direction == 1) {
				capsule.height = size.y;

				float absX = Mathf.Abs(size.x - lastSize.x);
				float absZ = Mathf.Abs(size.z - lastSize.z);
				capsule.radius = absX > absZ? size.x: size.z;
			}

			if (capsule.direction == 2) {
				capsule.height = size.z;

				float absX = Mathf.Abs(size.x - lastSize.x);
				float absY = Mathf.Abs(size.y - lastSize.y);
				capsule.radius = absX > absY? size.x: size.y;
			}

		}

		public static void SetSphereSize(SphereCollider sphere, Vector3 size, Vector3 lastSize) {
			if (size.x != lastSize.z) {
				sphere.radius += size.x - lastSize.x;
			} else if (size.y != lastSize.y) {
				sphere.radius += size.y - lastSize.y;
			} else if (size.z != lastSize.z) {
				sphere.radius += size.z - lastSize.z;
			}
		}

		public static Vector3 GetCapsuleSize(CapsuleCollider capsule) {
			if (capsule.direction == 0) return new Vector3(capsule.height, capsule.radius, capsule.radius);
			if (capsule.direction == 1) return new Vector3(capsule.radius, capsule.height, capsule.radius);
			return new Vector3(capsule.radius, capsule.radius, capsule.height);
		}
	}
}
