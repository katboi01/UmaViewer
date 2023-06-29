using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	public static class PuppetMasterTools {

		public static void PositionRagdoll(PuppetMaster puppetMaster) {
			Rigidbody[] rigidbodies = puppetMaster.transform.GetComponentsInChildren<Rigidbody>();
			if (rigidbodies.Length == 0) return;
			
			foreach (Muscle m in puppetMaster.muscles) if (m.joint == null || m.target == null) return;

			Vector3[] swingAxes = new Vector3[rigidbodies.Length];
			
			for (int i = 0; i < rigidbodies.Length; i++) {
				if (rigidbodies[i].transform.childCount == 1) {
					swingAxes[i] = rigidbodies[i].transform.InverseTransformDirection(rigidbodies[i].transform.GetChild(0).position - rigidbodies[i].transform.position);
				}
			}
			
			foreach (Rigidbody r in rigidbodies) {
				foreach (Muscle m in puppetMaster.muscles) {
					if (m.joint.GetComponent<Rigidbody>() == r) {
						r.transform.position = m.target.position;
					}
				}
			}
			
			for (int i = 0; i < rigidbodies.Length; i++) {
				if (rigidbodies[i].transform.childCount == 1) {
					Vector3 childPosition = rigidbodies[i].transform.GetChild(0).position;
					
					rigidbodies[i].transform.rotation = Quaternion.FromToRotation(rigidbodies[i].transform.rotation * swingAxes[i], childPosition - rigidbodies[i].transform.position) * rigidbodies[i].transform.rotation;
					rigidbodies[i].transform.GetChild(0).position = childPosition;
				}
			}
		}
		
		public static void RealignRagdoll(PuppetMaster puppetMaster) {
			foreach (Muscle m in puppetMaster.muscles) {
				if (m.joint == null || m.joint.transform == null || m.target == null) {
					Debug.LogWarning("Muscles incomplete, can not realign ragdoll.");
					return;
				}
			}
			
			foreach (Muscle m in puppetMaster.muscles) {
				if (m.target != null) {
					Transform[] children = new Transform[m.joint.transform.childCount];
					for (int c = 0 ; c < children.Length; c++) {
						children[c] = m.joint.transform.GetChild(c);
					}
					
					foreach (Transform c in children) c.parent = null;
					
					BoxCollider box = m.joint.GetComponent<BoxCollider>();
					Vector3 boxSizeWorldSpace = Vector3.zero;
					Vector3 boxCenterWorldSpace = Vector3.zero;
					
					if (box != null) {
						boxSizeWorldSpace = box.transform.TransformVector(box.size);
						boxCenterWorldSpace = box.transform.TransformVector(box.center);
					}
					
					CapsuleCollider capsule = m.joint.GetComponent<CapsuleCollider>();
					Vector3 capsuleCenterWorldSpace = Vector3.zero;
					Vector3 capsuleDirectionWorldSpace = Vector3.zero;
					
					if (capsule != null) {
						capsuleCenterWorldSpace = capsule.transform.TransformVector(capsule.center);
						capsuleDirectionWorldSpace = capsule.transform.TransformVector(DirectionIntToVector3(capsule.direction));
					}
					
					SphereCollider sphere = m.joint.GetComponent<SphereCollider>();
					Vector3 sphereCenterWorldSpace = Vector3.zero;
					
					if (sphere != null) {
						sphereCenterWorldSpace = sphere.transform.TransformVector(sphere.center);
					}
					
					Vector3 jointAxisWorldSpace = m.joint.transform.TransformVector(m.joint.axis);
					Vector3 jointSecondaryAxisWorldSpace = m.joint.transform.TransformVector(m.joint.secondaryAxis);
					
					// Rotate the bone
					m.joint.transform.rotation = m.target.rotation;
					
					if (box != null) {
						box.size = box.transform.InverseTransformVector(boxSizeWorldSpace);
						box.center = box.transform.InverseTransformVector(boxCenterWorldSpace);
					}
					
					if (capsule != null) {
						capsule.center = capsule.transform.InverseTransformVector(capsuleCenterWorldSpace);
						Vector3 directionLocalSpace = capsule.transform.InverseTransformDirection(capsuleDirectionWorldSpace);
						capsule.direction = DirectionVector3ToInt(directionLocalSpace);
					}
					
					if (sphere != null) {
						sphere.center = sphere.transform.InverseTransformVector(sphereCenterWorldSpace);
					}
					
					m.joint.axis = m.joint.transform.InverseTransformVector(jointAxisWorldSpace);
					m.joint.secondaryAxis = m.joint.transform.InverseTransformVector(jointSecondaryAxisWorldSpace);
					
					foreach (Transform c in children) c.parent = m.joint.transform;
				}
			}
		}
		
		private static Vector3 DirectionIntToVector3(int dir) {
			if (dir == 0) return Vector3.right;
			if (dir == 1) return Vector3.up;
			return Vector3.forward;
		}
		
		private static int DirectionVector3ToInt(Vector3 dir) {
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

	}
}
