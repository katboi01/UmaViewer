using UnityEngine;
using System.Collections;
using System;
using RootMotion;

namespace RootMotion.FinalIK {
	
	/// <summary>
	/// Hybrid %IK solver designed for mapping a character to a VR headset and 2 hand controllers 
	/// </summary>
	public partial class IKSolverVR: IKSolver {

		[System.Serializable]
		public enum PositionOffset {
			Pelvis,
			Chest,
			Head,
			LeftHand,
			RightHand,
			LeftFoot,
			RightFoot,
			LeftHeel,
			RightHeel
		}

		[System.Serializable]
		public enum RotationOffset {
			Pelvis,
			Chest,
			Head,
		}

		[System.Serializable]
		public class VirtualBone {

			public Vector3 readPosition;
			public Quaternion readRotation;

			public Vector3 solverPosition;
			public Quaternion solverRotation;

			public float length;
			public float sqrMag;
			public Vector3 axis;

			public VirtualBone(Vector3 position, Quaternion rotation) {
				Read(position, rotation);
			}

			public void Read(Vector3 position, Quaternion rotation) {
				this.readPosition = position;
				this.readRotation = rotation;
				this.solverPosition = position;
				this.solverRotation = rotation;
			}

			public static void SwingRotation(VirtualBone[] bones, int index, Vector3 swingTarget, float weight = 1f) {
				if (weight <= 0f) return;
					
				Quaternion r = Quaternion.FromToRotation(bones[index].solverRotation * bones[index].axis, swingTarget - bones[index].solverPosition);
				if (weight < 1f) r = Quaternion.Lerp(Quaternion.identity, r, weight);

				for (int i = index; i < bones.Length; i++) {
					bones[i].solverRotation = r * bones[i].solverRotation;
				}
			}

			// Calculates bone lengths and axes, returns the length of the entire chain
			public static float PreSolve(ref VirtualBone[] bones) {
				float length = 0;
				
				for (int i = 0; i < bones.Length; i++) {
					if (i < bones.Length - 1) {
						bones[i].sqrMag = (bones[i + 1].solverPosition - bones[i].solverPosition).sqrMagnitude;
						bones[i].length = Mathf.Sqrt(bones[i].sqrMag);
						length += bones[i].length;
						
						bones[i].axis = Quaternion.Inverse(bones[i].solverRotation) * (bones[i + 1].solverPosition - bones[i].solverPosition);
					} else {
						bones[i].sqrMag = 0f;
						bones[i].length = 0f;
					}
				}
				
				return length;
			}

			public static void RotateAroundPoint(VirtualBone[] bones, int index, Vector3 point, Quaternion rotation) {
				for (int i = index; i < bones.Length; i++) {
					if (bones[i] != null) {
						Vector3 dir = bones[i].solverPosition - point;
						bones[i].solverPosition = point + rotation * dir;
						bones[i].solverRotation = rotation * bones[i].solverRotation;
					}
				}
			}

			public static void RotateBy(VirtualBone[] bones, int index, Quaternion rotation) {
				for (int i = index; i < bones.Length; i++) {
					if (bones[i] != null) {
						Vector3 dir = bones[i].solverPosition - bones[index].solverPosition;
						bones[i].solverPosition = bones[index].solverPosition + rotation * dir;
						bones[i].solverRotation = rotation * bones[i].solverRotation;
					}
				}
			}

			public static void RotateBy(VirtualBone[] bones, Quaternion rotation) {
				for (int i = 0; i < bones.Length; i++) {
					if (bones[i] != null) {
						if (i > 0) {
							Vector3 dir = bones[i].solverPosition - bones[0].solverPosition;
							bones[i].solverPosition = bones[0].solverPosition + rotation * dir;
						}

						bones[i].solverRotation = rotation * bones[i].solverRotation;
					}
				}
			}

			public static void RotateTo(VirtualBone[] bones, int index, Quaternion rotation) {
				Quaternion q = QuaTools.FromToRotation(bones[index].solverRotation, rotation);
				
				RotateAroundPoint(bones, index, bones[index].solverPosition, q);
			}

			// TODO Move to IKSolverTrigonometric
			/// <summary>
			/// Solve the bone chain virtually using both solverPositions and SolverRotations. This will work the same as IKSolverTrigonometric.Solve.
			/// </summary>
			public static void SolveTrigonometric(VirtualBone[] bones, int first, int second, int third, Vector3 targetPosition, Vector3 bendNormal, float weight) {
				if (weight <= 0f) return;

				// Direction of the limb in solver
				targetPosition = Vector3.Lerp(bones[third].solverPosition, targetPosition, weight);
				
				Vector3 dir = targetPosition - bones[first].solverPosition;
				
				// Distance between the first and the last transform solver positions
				float sqrMag = dir.sqrMagnitude;
				if (sqrMag == 0f) return;
				float length = Mathf.Sqrt(sqrMag);
				
				float sqrMag1 = (bones[second].solverPosition - bones[first].solverPosition).sqrMagnitude;
				float sqrMag2 = (bones[third].solverPosition - bones[second].solverPosition).sqrMagnitude;
				
				// Get the general world space bending direction
				Vector3 bendDir = Vector3.Cross(dir, bendNormal);
				
				// Get the direction to the trigonometrically solved position of the second transform
				Vector3 toBendPoint = GetDirectionToBendPoint(dir, length, bendDir, sqrMag1, sqrMag2);
				
				// Position the second transform
				Quaternion q1 = Quaternion.FromToRotation(bones[second].solverPosition - bones[first].solverPosition, toBendPoint);
				if (weight < 1f) q1 = Quaternion.Lerp(Quaternion.identity, q1, weight);

				RotateAroundPoint(bones, first, bones[first].solverPosition, q1);

				Quaternion q2 = Quaternion.FromToRotation(bones[third].solverPosition - bones[second].solverPosition, targetPosition - bones[second].solverPosition);
				if (weight < 1f) q2 = Quaternion.Lerp(Quaternion.identity, q2, weight);

				RotateAroundPoint(bones, second, bones[second].solverPosition, q2);
			}

			//Calculates the bend direction based on the law of cosines. NB! Magnitude of the returned vector does not equal to the length of the first bone!
			private static Vector3 GetDirectionToBendPoint(Vector3 direction, float directionMag, Vector3 bendDirection, float sqrMag1, float sqrMag2) {
				float x = ((directionMag * directionMag) + (sqrMag1 - sqrMag2)) / 2f / directionMag;
				float y = (float)Math.Sqrt(Mathf.Clamp(sqrMag1 - x * x, 0, Mathf.Infinity));
				
				if (direction == Vector3.zero) return Vector3.zero;
				return Quaternion.LookRotation(direction, bendDirection) * new Vector3(0f, y, x);
			}

			// TODO Move to IKSolverFABRIK
			// Solves a simple FABRIK pass for a bone hierarchy, not using rotation limits or singularity breaking here
			public static void SolveFABRIK(VirtualBone[] bones, Vector3 startPosition, Vector3 targetPosition, float weight, float minNormalizedTargetDistance, int iterations, float length, Vector3 startOffset) {
				if (weight <= 0f) return;

				if (minNormalizedTargetDistance > 0f) {
					Vector3 targetDirection = targetPosition - startPosition;
					float targetLength = targetDirection.magnitude;
					Vector3 tP = startPosition + (targetDirection / targetLength) * Mathf.Max(length * minNormalizedTargetDistance, targetLength);
                    targetPosition = Vector3.Lerp(targetPosition, tP, weight);
				}
				
				// Iterating the solver
				for (int iteration = 0; iteration < iterations; iteration ++) {
					// Stage 1
					bones[bones.Length - 1].solverPosition = Vector3.Lerp(bones[bones.Length - 1].solverPosition, targetPosition, weight);
					
					for (int i = bones.Length - 2; i > -1; i--) {
						// Finding joint positions
						bones[i].solverPosition = SolveFABRIKJoint(bones[i].solverPosition, bones[i + 1].solverPosition, bones[i].length);
					}
					
					// Stage 2
                    if (iteration == 0) {
                        foreach (VirtualBone bone in bones) bone.solverPosition += startOffset;
                    }
                    
					bones[0].solverPosition = startPosition;
						
					for (int i = 1; i < bones.Length; i++) {
						bones[i].solverPosition = SolveFABRIKJoint(bones[i].solverPosition, bones[i - 1].solverPosition, bones[i - 1].length);
					}
				}
				
				for (int i = 0; i < bones.Length - 1; i++) {
					VirtualBone.SwingRotation(bones, i, bones[i + 1].solverPosition);
				}
			}

			// Solves a FABRIK joint between two bones.
			private static Vector3 SolveFABRIKJoint(Vector3 pos1, Vector3 pos2, float length) {
				return pos2 + (pos1 - pos2).normalized * length;
			}

			public static void SolveCCD(VirtualBone[] bones, Vector3 targetPosition, float weight, int iterations) {
				if (weight <= 0f) return;

				// Iterating the solver
				for (int iteration = 0; iteration < iterations; iteration ++) {
					for (int i = bones.Length - 2; i > -1; i--) {
						Vector3 toLastBone = bones[bones.Length - 1].solverPosition - bones[i].solverPosition;
						Vector3 toTarget = targetPosition - bones[i].solverPosition;


						Quaternion rotation = Quaternion.FromToRotation(toLastBone, toTarget);

						if (weight >= 1) {
							//bones[i].transform.rotation = targetRotation;
							VirtualBone.RotateBy(bones, i, rotation);
						} else {
							VirtualBone.RotateBy(bones, i, Quaternion.Lerp(Quaternion.identity, rotation, weight));
						}
					}
				}
			}
		}
	}
}

