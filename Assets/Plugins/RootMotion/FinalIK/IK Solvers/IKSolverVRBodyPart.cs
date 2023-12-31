using UnityEngine;
using System.Collections;
using System;
using RootMotion;

namespace RootMotion.FinalIK {
	
	/// <summary>
	/// Hybrid %IK solver designed for mapping a character to a VR headset and 2 hand controllers.
	/// </summary>
	public partial class IKSolverVR: IKSolver {

		/// <summary>
		/// A base class for all IKSolverVR body parts.
		/// </summary>
		[System.Serializable]
		public abstract class BodyPart {

			protected abstract void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, bool hasLegs, int rootIndex, int index);
			public abstract void PreSolve();
			public abstract void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations);
			public abstract void ApplyOffsets();
			public abstract void ResetOffsets();

			public float sqrMag { get; private set; }
			public float mag { get; private set; }

			[HideInInspector] public VirtualBone[] bones = new VirtualBone[0];
			protected bool initiated;
			protected Vector3 rootPosition;
			protected Quaternion rootRotation = Quaternion.identity;
			protected int index = -1;
            protected int LOD;

            public void SetLOD(int LOD)
            {
                this.LOD = LOD;
            }

			public void Read(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, bool hasLegs, int rootIndex, int index) {
				this.index = index;

				rootPosition = positions[rootIndex];
				rootRotation = rotations[rootIndex];

				OnRead(positions, rotations, hasChest, hasNeck, hasShoulders, hasToes, hasLegs, rootIndex, index);

				mag = VirtualBone.PreSolve(ref bones);
				sqrMag = mag * mag;

				initiated = true;
			}

			public void MovePosition(Vector3 position) {
				Vector3 delta = position - bones[0].solverPosition;
				foreach (VirtualBone bone in bones) bone.solverPosition += delta;
			}

			public void MoveRotation(Quaternion rotation) {
				Quaternion delta = QuaTools.FromToRotation(bones[0].solverRotation, rotation);
				VirtualBone.RotateAroundPoint(bones, 0, bones[0].solverPosition, delta);
			}

			public void Translate(Vector3 position, Quaternion rotation) {
				MovePosition(position);
				MoveRotation(rotation);
			}

			public void TranslateRoot(Vector3 newRootPos, Quaternion newRootRot) {
				Vector3 deltaPosition = newRootPos - rootPosition;
				rootPosition = newRootPos;
				foreach (VirtualBone bone in bones) bone.solverPosition += deltaPosition;

				Quaternion deltaRotation = QuaTools.FromToRotation(rootRotation, newRootRot);
				rootRotation = newRootRot;
				VirtualBone.RotateAroundPoint(bones, 0, newRootPos, deltaRotation);
			}

			public void RotateTo(VirtualBone bone, Quaternion rotation, float weight = 1f) {
				if (weight <= 0f) return;

				Quaternion q = QuaTools.FromToRotation(bone.solverRotation, rotation);

				if (weight < 1f) q = Quaternion.Slerp(Quaternion.identity, q, weight);

				for (int i = 0; i < bones.Length; i++) {
					if (bones[i] == bone) {
						VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition, q);
						return;
					}
				}
			}

			public void Visualize(Color color) {
				for (int i = 0; i < bones.Length - 1; i++) {
					Debug.DrawLine(bones[i].solverPosition, bones[i + 1].solverPosition, color);
				}
			}

			public void Visualize() {
				Visualize(Color.white);
			}
		}
	}
}