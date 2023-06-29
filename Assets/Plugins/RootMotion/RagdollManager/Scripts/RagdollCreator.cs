using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	/// <summary>
	/// Contains common functionality and helpers for creating any type of ragdolls.
	/// </summary>
	public abstract class RagdollCreator: MonoBehaviour {
		
		[System.Serializable]
		public enum ColliderType {
			Box,
			Capsule
		}

		[System.Serializable]
		public enum JointType {
			Configurable,
			Character
		}
		
		[System.Serializable]
		public enum Direction {
			X = 0,
			Y = 1,
			Z = 2
		}

		public struct CreateJointParams {
			public Rigidbody rigidbody;
			public Rigidbody connectedBody;
			public Transform child;
			public Vector3 worldSwingAxis;
			public Limits limits;
			public JointType type;
			
			public struct Limits {
				public float minSwing;
				public float maxSwing;
				public float swing2;
				public float twist;
				
				public Limits (float minSwing, float maxSwing, float swing2, float twist) {
					this.minSwing = minSwing;
					this.maxSwing = maxSwing;
					this.swing2 = swing2;
					this.twist = twist;
				}
			}
			
			public CreateJointParams (Rigidbody rigidbody, Rigidbody connectedBody, Transform child, Vector3 worldSwingAxis, Limits limits, JointType type) {
				this.rigidbody = rigidbody;
				this.connectedBody = connectedBody;
				this.child = child;
				this.worldSwingAxis = worldSwingAxis;
				this.limits = limits;
				this.type = type;
			}
		}

		public static void ClearAll(Transform root) {
			if (root == null) return;
			
			// If there is a Humanoid Animator, use it to find the first bone to clear
			Transform r = root;
			
			Animator animator = root.GetComponentInChildren<Animator>();
			if (animator != null && animator.isHuman) {
				// Check if Optimize GameObjects is used
				Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
				if (hips != null) {
					Transform[] hipChildren = hips.GetComponentsInChildren<Transform>();
					if (hipChildren.Length > 2) {
						r = hips;
					}
				}
			}
			
			Transform[] transforms = r.GetComponentsInChildren<Transform>();
			if (transforms.Length < 2) return;
			
			for (int i = animator != null && animator.isHuman? 0: 1; i < transforms.Length; i++) {
				ClearTransform(transforms[i]);
			}
		}
		protected static void ClearTransform(Transform transform) {
			if (transform == null) return;

			Collider[] colliders = transform.GetComponents<Collider>();
			foreach (Collider collider in colliders) {
				if (collider != null && !collider.isTrigger) GameObject.DestroyImmediate(collider);
			}

			var joint = transform.GetComponent<Joint>();
			if (joint != null) GameObject.DestroyImmediate(joint);

			var rigidbody = transform.GetComponent<Rigidbody>();
			if (rigidbody != null) GameObject.DestroyImmediate(rigidbody);
		}

		protected static void CreateCollider(Transform t, Vector3 startPoint, Vector3 endPoint, ColliderType colliderType, float lengthOverlap, float width) {
			Vector3 direction = endPoint - startPoint;
			float height = direction.magnitude * (1f + lengthOverlap);
			Vector3 heightAxis = AxisTools.GetAxisVectorToDirection(t, direction);
			
			t.gameObject.AddComponent<Rigidbody>();
			float scaleF = GetScaleF(t);
			
			switch(colliderType) {
			case ColliderType.Capsule:
				CapsuleCollider capsule = t.gameObject.AddComponent<CapsuleCollider>();
				capsule.height = Mathf.Abs(height / scaleF);
				capsule.radius = Mathf.Abs((width * 0.75f) / scaleF);
				capsule.direction = DirectionVector3ToInt(heightAxis);
				capsule.center = t.InverseTransformPoint(Vector3.Lerp(startPoint, endPoint, 0.5f));
				break;
			case ColliderType.Box:
				Vector3 size = Vector3.Scale(heightAxis, new Vector3(height, height, height));
				if (size.x == 0f) size.x = width;
				if (size.y == 0f) size.y = width;
				if (size.z == 0f) size.z = width;
				
				BoxCollider box = t.gameObject.AddComponent<BoxCollider>();
				
				box.size = size / scaleF;
				box.size = new Vector3(Mathf.Abs(box.size.x), Mathf.Abs(box.size.y), Mathf.Abs(box.size.z));
				box.center = t.InverseTransformPoint(Vector3.Lerp(startPoint, endPoint, 0.5f));
				break;
			}
		}

		protected static void CreateCollider(Transform t, Vector3 startPoint, Vector3 endPoint, ColliderType colliderType, float lengthOverlap, float width, float proportionAspect, Vector3 widthDirection) {
			if (colliderType == ColliderType.Capsule) {
				CreateCollider(t, startPoint, endPoint, colliderType, lengthOverlap, width * proportionAspect);
				return;
			}
			
			Vector3 direction = endPoint - startPoint;
			float height = direction.magnitude * (1f + lengthOverlap);
			
			Vector3 heightAxis = AxisTools.GetAxisVectorToDirection(t, direction);
			Vector3 widthAxis = AxisTools.GetAxisVectorToDirection(t, widthDirection);
			
			if (widthAxis == heightAxis) {
				Debug.LogWarning("Width axis = height axis on " + t.name, t);
				widthAxis = new Vector3(heightAxis.y, heightAxis.z, heightAxis.x);
			}
			
			t.gameObject.AddComponent<Rigidbody>();
			
			Vector3 heightAdd = Vector3.Scale(heightAxis, new Vector3(height, height, height));
			Vector3 widthAdd = Vector3.Scale(widthAxis, new Vector3(width, width, width));
			
			Vector3 size = heightAdd + widthAdd;
			if (size.x == 0f) size.x = width * proportionAspect;
			if (size.y == 0f) size.y = width * proportionAspect;
			if (size.z == 0f) size.z = width * proportionAspect;
			
			BoxCollider box = t.gameObject.AddComponent<BoxCollider>();
			box.size = size / GetScaleF(t);
			box.center = t.InverseTransformPoint(Vector3.Lerp(startPoint, endPoint, 0.5f));
		}

		protected static float GetScaleF(Transform t) {
			Vector3 scale = t.lossyScale;
			return (scale.x + scale.y + scale.z) / 3f;
		}

		protected static Vector3 Abs(Vector3 v) {
			Vector3Abs(ref v);
			return v;
		}

		protected static void Vector3Abs(ref Vector3 v) {
			v.x = Mathf.Abs(v.x);
			v.y = Mathf.Abs(v.y);
			v.z = Mathf.Abs(v.z);
		}

		protected static Vector3 DirectionIntToVector3(int dir) {
			if (dir == 0) return Vector3.right;
			if (dir == 1) return Vector3.up;
			return Vector3.forward;
		}
		
		protected static Vector3 DirectionToVector3(Direction dir) {
			if (dir == Direction.X) return Vector3.right;
			if (dir == Direction.Y) return Vector3.up;
			return Vector3.forward;
		}
		
		protected static int DirectionVector3ToInt(Vector3 dir) {
			float dotX = Vector3.Dot(dir, Vector3.right);
			float dotY = Vector3.Dot(dir, Vector3.up);
			float dotZ = Vector3.Dot(dir, Vector3.forward);
			
			float absDotX = Mathf.Abs(dotX);
			float absDotY = Mathf.Abs(dotY);
			float absDotZ = Mathf.Abs(dotZ);
			
			int rotatedDirection = 0;
			if (absDotY > absDotX && absDotY > absDotZ) rotatedDirection = 1;
			if (absDotZ > absDotX && absDotZ > absDotY) rotatedDirection = 2;
			return rotatedDirection;
		}
		
		protected static Vector3 GetLocalOrthoDirection(Transform transform, Vector3 worldDir) {
			worldDir = worldDir.normalized;
			
			float dotX = Vector3.Dot(worldDir, transform.right);
			float dotY = Vector3.Dot(worldDir, transform.up);
			float dotZ = Vector3.Dot(worldDir, transform.forward);
			
			float absDotX = Mathf.Abs(dotX);
			float absDotY = Mathf.Abs(dotY);
			float absDotZ = Mathf.Abs(dotZ);
			
			Vector3 orthoDirection = Vector3.right;
			if (absDotY > absDotX && absDotY > absDotZ) orthoDirection = Vector3.up;
			if (absDotZ > absDotX && absDotZ > absDotY) orthoDirection = Vector3.forward;
			
			if (Vector3.Dot(worldDir, transform.rotation * orthoDirection) < 0f) orthoDirection = -orthoDirection;
			
			return orthoDirection;
		}

		protected static Rigidbody GetConnectedBody(Transform bone, ref Transform[] bones) {
			if (bone.parent == null) return null;
			
			foreach (Transform bone2 in bones) {
				if (bone.parent == bone2 && bone2.GetComponent<Rigidbody>() != null) return bone2.GetComponent<Rigidbody>();
			}
			
			return GetConnectedBody(bone.parent, ref bones);
		}

		protected static void CreateJoint(CreateJointParams p) {
			Vector3 axis = GetLocalOrthoDirection(p.rigidbody.transform, p.worldSwingAxis);

			Vector3 twistAxis = Vector3.forward;

			if (p.child != null) {
				twistAxis = GetLocalOrthoDirection(p.rigidbody.transform, p.child.position - p.rigidbody.transform.position);
			} else if (p.connectedBody != null) {
				twistAxis = GetLocalOrthoDirection(p.rigidbody.transform, p.rigidbody.transform.position - p.connectedBody.transform.position);
			}

			Vector3 secondaryAxis = Vector3.Cross(axis, twistAxis);
			
			if (p.type == JointType.Configurable) {
				ConfigurableJoint j = p.rigidbody.gameObject.AddComponent<ConfigurableJoint>();
				j.connectedBody = p.connectedBody;
				
				ConfigurableJointMotion linearMotion = p.connectedBody != null? ConfigurableJointMotion.Locked: ConfigurableJointMotion.Free;
				ConfigurableJointMotion angularMotion = p.connectedBody != null? ConfigurableJointMotion.Limited: ConfigurableJointMotion.Free;
				
				j.xMotion = linearMotion;
				j.yMotion = linearMotion;
				j.zMotion = linearMotion;
				
				j.angularXMotion = angularMotion;
				j.angularYMotion = angularMotion;
				j.angularZMotion = angularMotion;
				
				if (p.connectedBody != null) {
					j.axis = axis;
					j.secondaryAxis = secondaryAxis;
					
					j.lowAngularXLimit = ToSoftJointLimit(p.limits.minSwing);
					j.highAngularXLimit = ToSoftJointLimit(p.limits.maxSwing);
					j.angularYLimit = ToSoftJointLimit(p.limits.swing2);
					j.angularZLimit = ToSoftJointLimit(p.limits.twist);
				}

				j.anchor = Vector3.zero;
			} else {
				if (p.connectedBody == null) return;
				CharacterJoint j = p.rigidbody.gameObject.AddComponent<CharacterJoint>();
				j.connectedBody = p.connectedBody;
				
				j.axis = axis;
				j.swingAxis = secondaryAxis;

				j.lowTwistLimit = ToSoftJointLimit(p.limits.minSwing);
				j.highTwistLimit = ToSoftJointLimit(p.limits.maxSwing);
				j.swing1Limit = ToSoftJointLimit(p.limits.swing2);
				j.swing2Limit = ToSoftJointLimit(p.limits.twist);

				j.anchor = Vector3.zero;
			}
		}
		
		private static SoftJointLimit ToSoftJointLimit(float limit) {
			SoftJointLimit s = new SoftJointLimit();
			s.limit = limit;
			return s;
		}

	}
}
