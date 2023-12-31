using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Posing the children of a Transform to match the children of another Transform
	/// </summary>
	public class HandPoser : Poser {

		public override void AutoMapping() {
			if (poseRoot == null) poseChildren = new Transform[0];
			else poseChildren = (Transform[])poseRoot.GetComponentsInChildren<Transform>();
			
			_poseRoot = poseRoot;
		}

		protected override void InitiatePoser() {
			// Find the children
			children = (Transform[])GetComponentsInChildren<Transform>();
			
			StoreDefaultState();
		}
		
		protected override void FixPoserTransforms() {
			for (int i = 0; i < children.Length; i++) {
				children[i].localPosition = defaultLocalPositions[i];
				children[i].localRotation = defaultLocalRotations[i];
			}
		}

		protected override void UpdatePoser() {
			if (weight <= 0f) return;
			if (localPositionWeight <= 0f && localRotationWeight <= 0f) return;
			
			// Get the children, if we don't have them already
			if (_poseRoot != poseRoot) AutoMapping();
			
			if (poseRoot == null) return;
			
			// Something went wrong
			if (children.Length != poseChildren.Length) {
				Warning.Log("Number of children does not match with the pose", transform);
				return;
			}
			
			// Calculate weights
			float rW = localRotationWeight * weight;
			float pW = localPositionWeight * weight;
			
			// Lerping the localRotation and the localPosition
			for (int i = 0; i < children.Length; i++) {
				if (children[i] != transform) {
					children[i].localRotation = Quaternion.Lerp(children[i].localRotation, poseChildren[i].localRotation, rW);
					children[i].localPosition = Vector3.Lerp(children[i].localPosition, poseChildren[i].localPosition, pW);
				}
			}
		}

		protected Transform[] children;

		private Transform _poseRoot;
		private Transform[] poseChildren;
		private Vector3[] defaultLocalPositions;
		private Quaternion[] defaultLocalRotations;

		protected void StoreDefaultState() {
			defaultLocalPositions = new Vector3[children.Length];
			defaultLocalRotations = new Quaternion[children.Length];

			for (int i = 0; i < children.Length; i++) {
				defaultLocalPositions[i] = children[i].localPosition;
				defaultLocalRotations[i] = children[i].localRotation;
			}
		}
	}
}
