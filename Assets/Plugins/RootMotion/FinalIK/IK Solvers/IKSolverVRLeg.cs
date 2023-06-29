using UnityEngine;
using System.Collections;
using System;
using RootMotion;

namespace RootMotion.FinalIK {
	
	/// <summary>
	/// Hybrid %IK solver designed for mapping a character to a VR headset and 2 hand controllers 
	/// </summary>
	public partial class IKSolverVR: IKSolver {

		/// <summary>
		/// 4-segmented analytic leg chain.
		/// </summary>
		[System.Serializable]
		public class Leg: BodyPart {

			[Tooltip("The foot/toe target. This should not be the foot tracker itself, but a child GameObject parented to it so you could adjust it's position/rotation to match the orientation of the foot/toe bone. If a toe bone is assigned in the References, the solver will match the toe bone to this target. If no toe bone assigned, foot bone will be used instead.")]
            /// <summary>
            /// The foot/toe target. This should not be the foot tracker itself, but a child GameObject parented to it so you could adjust it's position/rotation to match the orientation of the foot/toe bone. If a toe bone is assigned in the References, the solver will match the toe bone to this target. If no toe bone assigned, foot bone will be used instead.
            /// </summary>
            public Transform target;

			[Tooltip("The knee will be bent towards this Transform if 'Bend Goal Weight' > 0.")]
			/// <summary>
			/// The knee will be bent towards this Transform if 'Bend Goal Weight' > 0.
			/// </summary>
			public Transform bendGoal;

			[Tooltip("Positional weight of the toe/foot target. Note that if you have nulled the target, the foot will still be pulled to the last position of the target until you set this value to 0.")]
            /// <summary>
            /// Positional weight of the toe/foot target. Note that if you have nulled the target, the foot will still be pulled to the last position of the target until you set this value to 0.
            /// </summary>
            [Range(0f, 1f)] public float positionWeight;

			[Tooltip("Rotational weight of the toe/foot target. Note that if you have nulled the target, the foot will still be rotated to the last rotation of the target until you set this value to 0.")]
            /// <summary>
            /// Rotational weight of the toe/foot target. Note that if you have nulled the target, the foot will still be rotated to the last rotation of the target until you set this value to 0.
            /// </summary>
            [Range(0f, 1f)] public float rotationWeight;

			[Tooltip("If greater than 0, will bend the knee towards the 'Bend Goal' Transform.")]
			/// <summary>
			/// If greater than 0, will bend the knee towards the 'Bend Goal' Transform.
			/// </summary>
			[Range(0f, 1f)] public float bendGoalWeight;

			[Tooltip("Angular offset of knee bending direction.")]
			/// <summary>
			/// Angular offset of knee bending direction.
			/// </summary>
			[Range(-180f, 180f)] public float swivelOffset;

			[Tooltip("If 0, the bend plane will be locked to the rotation of the pelvis and rotating the foot will have no effect on the knee direction. If 1, to the target rotation of the leg so that the knee will bend towards the forward axis of the foot. Values in between will be slerped between the two.")]
			/// <summary>
			/// If 0, the bend plane will be locked to the rotation of the pelvis and rotating the foot will have no effect on the knee direction. If 1, to the target rotation of the leg so that the knee will bend towards the forward axis of the foot. Values in between will be slerped between the two.
			/// </summary>
			[Range(0f, 1f)] public float bendToTargetWeight = 0.5f;

			[Tooltip("Use this to make the leg shorter/longer. Works by displacement of foot and calf localPosition.")]
            /// <summary>
            /// Use this to make the leg shorter/longer. Works by displacement of foot and calf localPosition.
            /// </summary>
            [Range(0.01f, 2f)]
			public float legLengthMlp = 1f;

			[Tooltip("Evaluates stretching of the leg by target distance relative to leg length. Value at time 1 represents stretching amount at the point where distance to the target is equal to leg length. Value at time 1 represents stretching amount at the point where distance to the target is double the leg length. Value represents the amount of stretching. Linear stretching would be achieved with a linear curve going up by 45 degrees. Increase the range of stretching by moving the last key up and right at the same amount. Smoothing in the curve can help reduce knee snapping (start stretching the arm slightly before target distance reaches leg length). To get a good optimal value for this curve, please go to the 'VRIK (Basic)' demo scene and copy the stretch curve over from the Pilot character.")]
            /// <summary>
            /// Evaluates stretching of the leg by target distance relative to leg length. Value at time 1 represents stretching amount at the point where distance to the target is equal to leg length. Value at time 1 represents stretching amount at the point where distance to the target is double the leg length. Value represents the amount of stretching. Linear stretching would be achieved with a linear curve going up by 45 degrees. Increase the range of stretching by moving the last key up and right at the same amount. Smoothing in the curve can help reduce knee snapping (start stretching the arm slightly before target distance reaches leg length). To get a good optimal value for this curve, please go to the 'VRIK (Basic)' demo scene and copy the stretch curve over from the Pilot character.
            /// </summary>
            public AnimationCurve stretchCurve = new AnimationCurve();

			/// <summary>
			/// Target position of the toe/foot. Will be overwritten if target is assigned.
			/// </summary>
			[NonSerialized][HideInInspector] public Vector3 IKPosition;

			/// <summary>
			/// Target rotation of the toe/foot. Will be overwritten if target is assigned.
			/// </summary>
			[NonSerialized][HideInInspector] public Quaternion IKRotation = Quaternion.identity;

			/// <summary>
			/// Position offset of the toe/foot. Will be applied on top of target position and reset to Vector3.zero after each update.
			/// </summary>
			[NonSerialized][HideInInspector] public Vector3 footPositionOffset;

			/// <summary>
			/// Position offset of the heel. Will be reset to Vector3.zero after each update.
			/// </summary>
			[NonSerialized][HideInInspector] public Vector3 heelPositionOffset;

			/// <summary>
			/// Rotation offset of the toe/foot. Will be reset to Quaternion.identity after each update.
			/// </summary>
			[NonSerialized][HideInInspector] public Quaternion footRotationOffset = Quaternion.identity;

			/// <summary>
			/// The length of the leg (calculated in last read).
			/// </summary>
			[NonSerialized][HideInInspector] public float currentMag;

            /// <summary>
            /// If true, will sample the leg bend angle each frame from the animation.
            /// </summary>
            [HideInInspector] public bool useAnimatedBendNormal;

			public Vector3 position { get; private set; }
			public Quaternion rotation { get; private set; }
			public bool hasToes { get; private set; }
			public VirtualBone thigh { get { return bones[0]; }}
			private VirtualBone calf { get { return bones[1]; }}
			private VirtualBone foot { get { return bones[2]; }}
			private VirtualBone toes { get { return bones[3]; }}
			public VirtualBone lastBone { get { return bones[bones.Length - 1]; }}
			public Vector3 thighRelativeToPelvis { get; private set; }

			private Vector3 footPosition;
			private Quaternion footRotation = Quaternion.identity;
			private Vector3 bendNormal;
			private Quaternion calfRelToThigh = Quaternion.identity;
			private Quaternion thighRelToFoot = Quaternion.identity;
			private Vector3 bendNormalRelToPelvis;
			private Vector3 bendNormalRelToTarget;

			protected override void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, bool hasLegs, int rootIndex, int index) {
				Vector3 thighPos = positions[index];
				Quaternion thighRot = rotations[index];
				Vector3 calfPos = positions[index + 1];
				Quaternion calfRot = rotations[index + 1];
				Vector3 footPos = positions[index + 2];
				Quaternion footRot = rotations[index + 2];
				Vector3 toePos = positions[index + 3];
				Quaternion toeRot = rotations[index + 3];

				if (!initiated) {
					this.hasToes = hasToes;
					bones = new VirtualBone[hasToes? 4: 3];

					if (hasToes) {
						bones[0] = new VirtualBone(thighPos, thighRot);
						bones[1] = new VirtualBone(calfPos, calfRot);
						bones[2] = new VirtualBone(footPos, footRot);
						bones[3] = new VirtualBone(toePos, toeRot);

						IKPosition = toePos;
						IKRotation = toeRot;
					} else {
						bones[0] = new VirtualBone(thighPos, thighRot);
						bones[1] = new VirtualBone(calfPos, calfRot);
						bones[2] = new VirtualBone(footPos, footRot);

						IKPosition = footPos;
						IKRotation = footRot;
					}

					bendNormal = Vector3.Cross(calfPos - thighPos, footPos - calfPos);
					bendNormalRelToPelvis = Quaternion.Inverse(rootRotation) * bendNormal;
					bendNormalRelToTarget = Quaternion.Inverse(IKRotation) * bendNormal;

					rotation = IKRotation;
				}

				if (hasToes) {
					bones[0].Read(thighPos, thighRot);
					bones[1].Read(calfPos, calfRot);
					bones[2].Read(footPos, footRot);
					bones[3].Read(toePos, toeRot);
				} else {
					bones[0].Read(thighPos, thighRot);
					bones[1].Read(calfPos, calfRot);
					bones[2].Read(footPos, footRot);
				}
			}

			public override void PreSolve() {
				if (target != null) {
					IKPosition = target.position;
					IKRotation = target.rotation;
				}

				footPosition = foot.solverPosition;
				footRotation = foot.solverRotation;

				position = lastBone.solverPosition;
				rotation = lastBone.solverRotation;

				if (rotationWeight > 0f) {
					ApplyRotationOffset(RootMotion.QuaTools.FromToRotation(rotation, IKRotation), rotationWeight);
				}

				if (positionWeight > 0f) {
					ApplyPositionOffset(IKPosition - position, positionWeight);
				}

				thighRelativeToPelvis = Quaternion.Inverse(rootRotation) * (thigh.solverPosition - rootPosition);
				calfRelToThigh = Quaternion.Inverse(thigh.solverRotation) * calf.solverRotation;
				thighRelToFoot = Quaternion.Inverse(lastBone.solverRotation) * thigh.solverRotation;

                // Calculate bend plane normal
                if (useAnimatedBendNormal)
                {
                    // This was used before version 1.8
                    bendNormal = Vector3.Cross(calf.solverPosition - thigh.solverPosition, foot.solverPosition - calf.solverPosition);
                }
                else
                {
                    if (bendToTargetWeight <= 0f)
                    {
                        bendNormal = rootRotation * bendNormalRelToPelvis;
                    }
                    else if (bendToTargetWeight >= 1f)
                    {
                        bendNormal = rotation * bendNormalRelToTarget;
                    }
                    else
                    {
                        bendNormal = Vector3.Slerp(rootRotation * bendNormalRelToPelvis, rotation * bendNormalRelToTarget, bendToTargetWeight);
                    }
                }
			}

			public override void ApplyOffsets() {
				ApplyPositionOffset(footPositionOffset, 1f);
				ApplyRotationOffset(footRotationOffset, 1f);

				// Heel position offset
				Quaternion fromTo = Quaternion.FromToRotation(footPosition - position, footPosition + heelPositionOffset - position);
				footPosition = position + fromTo * (footPosition - position);
				footRotation = fromTo * footRotation;

				// Bend normal offset
				float bAngle = 0f;

				if (bendGoal != null && bendGoalWeight > 0f) {
					Vector3 b = Vector3.Cross(bendGoal.position - thigh.solverPosition, position - thigh.solverPosition);
					Quaternion l = Quaternion.LookRotation(bendNormal, thigh.solverPosition - foot.solverPosition);
					Vector3 bRelative = Quaternion.Inverse(l) * b;
					bAngle = Mathf.Atan2(bRelative.x, bRelative.z) * Mathf.Rad2Deg * bendGoalWeight;
				}

				float sO = swivelOffset + bAngle;

				if (sO != 0f) {
					bendNormal = Quaternion.AngleAxis(sO, thigh.solverPosition - lastBone.solverPosition) * bendNormal;
					thigh.solverRotation = Quaternion.AngleAxis(-sO, thigh.solverRotation * thigh.axis) * thigh.solverRotation;
				}
			}

			// Foot position offset
			private void ApplyPositionOffset(Vector3 offset, float weight) {
				if (weight <= 0f) return;
				offset *= weight;

				// Foot position offset
				footPosition += offset;
				position += offset;
			}

			// Foot rotation offset
			private void ApplyRotationOffset(Quaternion offset, float weight) {
				if (weight <= 0f) return;
				if (weight < 1f) {
					offset = Quaternion.Lerp(Quaternion.identity, offset, weight);
				}

				footRotation = offset * footRotation;
				rotation = offset * rotation;
				bendNormal = offset * bendNormal;
				footPosition = position + offset * (footPosition - position);
			}

			public void Solve(bool stretch) {
				if (stretch && LOD < 1) Stretching();

				// Foot pass
				VirtualBone.SolveTrigonometric(bones, 0, 1, 2, footPosition, bendNormal, 1f);

				// Rotate foot back to where it was before the last solving
				RotateTo(foot, footRotation);

                // Toes pass
                if (!hasToes)
                {
                    FixTwistRotations();
                    return;
                }
				
				Vector3 b = Vector3.Cross(foot.solverPosition - thigh.solverPosition, toes.solverPosition - foot.solverPosition);

				VirtualBone.SolveTrigonometric(bones, 0, 2, 3, position, b, 1f);

                // Fix thigh twist relative to target rotation
                FixTwistRotations();

				// Keep toe rotation fixed
				toes.solverRotation = rotation;
			}

            private void FixTwistRotations()
            {
                if (LOD < 1)
                {
                    if (bendToTargetWeight > 0f)
                    {
                        // Fix thigh twist relative to target rotation
                        Quaternion thighRotation = rotation * thighRelToFoot;
                        Quaternion f = Quaternion.FromToRotation(thighRotation * thigh.axis, calf.solverPosition - thigh.solverPosition);
                        if (bendToTargetWeight < 1f)
                        {
                            thigh.solverRotation = Quaternion.Slerp(thigh.solverRotation, f * thighRotation, bendToTargetWeight);
                        }
                        else
                        {
                            thigh.solverRotation = f * thighRotation;
                        }
                    }

                    // Fix calf twist relative to thigh
                    Quaternion calfRotation = thigh.solverRotation * calfRelToThigh;
                    Quaternion fromTo = Quaternion.FromToRotation(calfRotation * calf.axis, foot.solverPosition - calf.solverPosition);
                    calf.solverRotation = fromTo * calfRotation;
                }
            }

			private void Stretching() {
				// Adjusting leg length
				float legLength = thigh.length + calf.length;
				Vector3 kneeAdd = Vector3.zero;
				Vector3 footAdd = Vector3.zero;

				if (legLengthMlp != 1f) {
					legLength *= legLengthMlp;
					kneeAdd = (calf.solverPosition - thigh.solverPosition) * (legLengthMlp - 1f) * positionWeight;
					footAdd = (foot.solverPosition - calf.solverPosition) * (legLengthMlp - 1f) * positionWeight;
					calf.solverPosition += kneeAdd;
					foot.solverPosition += kneeAdd + footAdd;
					if (hasToes) toes.solverPosition += kneeAdd + footAdd;
				}

				// Stretching
				float distanceToTarget = Vector3.Distance(thigh.solverPosition, footPosition);
				float stretchF = distanceToTarget / legLength;

				float m = stretchCurve.Evaluate(stretchF) * positionWeight;
				//m *= positionWeight;

				kneeAdd = (calf.solverPosition - thigh.solverPosition) * m;
				footAdd = (foot.solverPosition - calf.solverPosition) * m;

				calf.solverPosition += kneeAdd;
				foot.solverPosition += kneeAdd + footAdd;
				if (hasToes) toes.solverPosition += kneeAdd + footAdd;
			}

			public override void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations) {
				solvedRotations[index] = thigh.solverRotation;
				solvedRotations[index + 1] = calf.solverRotation;
				solvedRotations[index + 2] = foot.solverRotation;

				solvedPositions[index] = thigh.solverPosition;
				solvedPositions[index + 1] = calf.solverPosition;
				solvedPositions[index + 2] = foot.solverPosition;

				if (hasToes) {
					solvedRotations[index + 3] = toes.solverRotation;
					solvedPositions[index + 3] = toes.solverPosition;
				}
			}

			public override void ResetOffsets() {
				footPositionOffset = Vector3.zero;
				footRotationOffset = Quaternion.identity;
				heelPositionOffset = Vector3.zero;
			}
		}
	}
}