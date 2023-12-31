using UnityEngine;
using System.Collections;
using System;

namespace RootMotion.FinalIK {
	
	/// <summary>
	/// Posing the children of a Transform to match the children of another Transform
	/// </summary>
	public class GenericPoser : Poser {

		/// <summary>
		/// Mapping a bone to it's target
		/// </summary>
		[System.Serializable]
		public class Map {
			public Transform bone;
			public Transform target;

			private Vector3 defaultLocalPosition;
			private Quaternion defaultLocalRotation;

			// Custom constructor
			public Map(Transform bone, Transform target) {
				this.bone = bone;
				this.target = target;

				StoreDefaultState();
			}

			public void StoreDefaultState() {
				defaultLocalPosition = bone.localPosition;
				defaultLocalRotation = bone.localRotation;
			}

			public void FixTransform() {
				bone.localPosition = defaultLocalPosition;
				bone.localRotation = defaultLocalRotation;
			}

			// Update mapping
			public void Update(float localRotationWeight, float localPositionWeight) {
				bone.localRotation = Quaternion.Lerp(bone.localRotation, target.localRotation, localRotationWeight);
				bone.localPosition = Vector3.Lerp(bone.localPosition, target.localPosition, localPositionWeight);
			}
		}

		public Map[] maps;

		/// <summary>
		/// Finds mapping automatically. This requires for all children of the transform to have unique names. This method is not very memory efficient so try to avoid using it in play mode.
		/// </summary>
		[ContextMenu("Auto-Mapping")]
		public override void AutoMapping() {
			if (poseRoot == null) {
				maps = new Map[0];
				return;
			}

			maps = new Map[0];

			Transform[] children = (Transform[])transform.GetComponentsInChildren<Transform>();
			Transform[] poseChildren = (Transform[])poseRoot.GetComponentsInChildren<Transform>();
			Transform target;

			// Find all the bone to target matches
			for (int i = 1; i < children.Length; i++) {
				target = GetTargetNamed(children[i].name, poseChildren);
				if (target != null) {
					Array.Resize(ref maps, maps.Length + 1);
					maps[maps.Length - 1] = new Map(children[i], target);
				}
			}

			StoreDefaultState();
		}

		protected override void InitiatePoser() {
			StoreDefaultState();
		}

		protected override void UpdatePoser() {
			if (weight <= 0f) return;
			if (localPositionWeight <= 0f && localRotationWeight <= 0f) return;
			if (poseRoot == null) return;
			
			// Calculate weights
			float rW = localRotationWeight * weight;
			float pW = localPositionWeight * weight;
			
			// Lerping the localRotation and the localPosition
			for (int i = 0; i < maps.Length; i++) maps[i].Update(rW, pW);
		}

		
		protected override void FixPoserTransforms() {
			for (int i = 0; i < maps.Length; i++) {
				maps[i].FixTransform();
			}
		}

		private void StoreDefaultState() {
			for (int i = 0; i < maps.Length; i++) {
				maps[i].StoreDefaultState();
			}
		}

		// Returns a Transform from the array that has the specified name
		private Transform GetTargetNamed(string tName, Transform[] array) {
			for (int i = 0; i < array.Length; i++) {
				if (array[i].name == tName) return array[i];
			}
			return null;
		}


	}
}
