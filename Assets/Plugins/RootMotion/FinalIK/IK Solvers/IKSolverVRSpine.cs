using UnityEngine;
using System.Collections;
using System;
using RootMotion;
using UnityEngine.Serialization;

namespace RootMotion.FinalIK {
	
	/// <summary>
	/// Hybrid %IK solver designed for mapping a character to a VR headset and 2 hand controllers 
	/// </summary>
	public partial class IKSolverVR: IKSolver {
		
		/// <summary>
		/// Spine solver for IKSolverVR.
		/// </summary>
		[System.Serializable]
		public class Spine: BodyPart {

			[Tooltip("The head target. This should not be the camera Transform itself, but a child GameObject parented to it so you could adjust it's position/rotation  to match the orientation of the head bone. The best practice for setup would be to move the camera to the avatar's eyes, duplicate the avatar's head bone and parent it to the camera. Then assign the duplicate to this slot.")]
            /// <summary>
            /// The head target. This should not be the camera Transform itself, but a child GameObject parented to it so you could adjust it's position/rotation to match the orientation of the head bone. The best practice for setup would be to move the camera to the avatar's eyes, duplicate the avatar's head bone and parent it to the camera. Then assign the duplicate to this slot.
            /// </summary>
            public Transform headTarget;

			[Tooltip("The pelvis target (optional), useful for seated rigs or if you had an additional tracker on the backpack or belt are. The best practice for setup would be to duplicate the avatar's pelvis bone and parenting it to the pelvis tracker. Then assign the duplicate to this slot.")]
            /// <summary>
            /// The pelvis target (optional), useful for seated rigs or if you had an additional tracker on the backpack or belt are. The best practice for setup would be to duplicate the avatar's pelvis bone and parenting it to the pelvis tracker. Then assign the duplicate to this slot.
            /// </summary>
            public Transform pelvisTarget;

			[Tooltip("Positional weight of the head target. Note that if you have nulled the headTarget, the head will still be pulled to the last position of the headTarget until you set this value to 0.")]
            /// <summary>
            /// Positional weight of the head target. Note that if you have nulled the headTarget, the head will still be pulled to the last position of the headTarget until you set this value to 0.
            /// </summary>
            [Range(0f, 1f)] public float positionWeight = 1f;

			[Tooltip("Rotational weight of the head target. Note that if you have nulled the headTarget, the head will still be rotated to the last rotation of the headTarget until you set this value to 0.")]
            /// <summary>
            /// Rotational weight of the head target. Note that if you have nulled the headTarget, the head will still be rotated to the last rotation of the headTarget until you set this value to 0.
            /// </summary>
            [Range(0f, 1f)] public float rotationWeight = 1f;

			[Tooltip("Positional weight of the pelvis target. Note that if you have nulled the pelvisTarget, the pelvis will still be pulled to the last position of the pelvisTarget until you set this value to 0.")]
            /// <summary>
            /// Positional weight of the pelvis target. Note that if you have nulled the pelvisTarget, the pelvis will still be pulled to the last position of the pelvisTarget until you set this value to 0.
            /// </summary>
            [Range(0f, 1f)] public float pelvisPositionWeight;

			[Tooltip("Rotational weight of the pelvis target. Note that if you have nulled the pelvisTarget, the pelvis will still be rotated to the last rotation of the pelvisTarget until you set this value to 0.")]
            /// <summary>
            /// Rotational weight of the pelvis target. Note that if you have nulled the pelvisTarget, the pelvis will still be rotated to the last rotation of the pelvisTarget until you set this value to 0.
            /// </summary>
            [Range(0f, 1f)] public float pelvisRotationWeight;

			[Tooltip("If 'Chest Goal Weight' is greater than 0, the chest will be turned towards this Transform.")]
			/// <summary>
			/// If chestGoalWeight is greater than 0, the chest will be turned towards this Transform.
			/// </summary>
			public Transform chestGoal;

			[Tooltip("Weight of turning the chest towards the 'Chest Goal'.")]
			/// <summary>
			/// Weight of turning the chest towards the chestGoal.
			/// </summary>
			[Range(0f, 1f)] public float chestGoalWeight;

			[Tooltip("Minimum height of the head from the root of the character.")]
			/// <summary>
			/// Minimum height of the head from the root of the character.
			/// </summary>
			public float minHeadHeight = 0.8f;

			[Tooltip("Determines how much the body will follow the position of the head.")]
			/// <summary>
			/// Determines how much the body will follow the position of the head.
			/// </summary>
			[Range(0f, 1f)] public float bodyPosStiffness = 0.55f;

			[Tooltip("Determines how much the body will follow the rotation of the head.")]
			/// <summary>
			/// Determines how much the body will follow the rotation of the head.
			/// </summary>
			[Range(0f, 1f)] public float bodyRotStiffness = 0.1f;

			[Tooltip("Determines how much the chest will rotate to the rotation of the head.")]
			/// <summary>
			/// Determines how much the chest will rotate to the rotation of the head.
			/// </summary>
			[FormerlySerializedAs("chestRotationWeight")]
			[Range(0f, 1f)] public float neckStiffness = 0.2f;

			[Tooltip("The amount of rotation applied to the chest based on hand positions.")]
			/// <summary>
			/// The amount of rotation applied to the chest based on hand positions.
			/// </summary>
			[Range(0f, 1f)] public float rotateChestByHands = 1f;

			[Tooltip("Clamps chest rotation. Value of 0.5 allows 90 degrees of rotation for the chest relative to the head. Value of 0 allows 180 degrees and value of 1 means the chest will be locked relative to the head.")]
            /// <summary>
            /// Clamps chest rotation. Value of 0.5 allows 90 degrees of rotation for the chest relative to the head. Value of 0 allows 180 degrees and value of 1 means the chest will be locked relative to the head.
            /// </summary>
            [Range(0f, 1f)] public float chestClampWeight = 0.5f;

			[Tooltip("Clamps head rotation. Value of 0.5 allows 90 degrees of rotation for the head relative to the headTarget. Value of 0 allows 180 degrees and value of 1 means head rotation will be locked to the target.")]
            /// <summary>
            /// Clamps head rotation. Value of 0.5 allows 90 degrees of rotation for the head relative to the headTarget. Value of 0 allows 180 degrees and value of 1 means head rotation will be locked to the target.
            /// </summary>
            [Range(0f, 1f)] public float headClampWeight = 0.6f;

			[Tooltip("Moves the body horizontally along -character.forward axis by that value when the player is crouching.")]
			/// <summary>
			/// Moves the body horizontally along -character.forward axis by that value when the player is crouching.
			/// </summary>
			public float moveBodyBackWhenCrouching = 0.5f;

			[Tooltip("How much will the pelvis maintain it's animated position?")]
			/// <summary>
			/// How much will the pelvis maintain it's animated position?
			/// </summary>
			[Range(0f, 1f)] public float maintainPelvisPosition = 0.2f;

			[Tooltip("Will automatically rotate the root of the character if the head target has turned past this angle.")]
			/// <summary>
			/// Will automatically rotate the root of the character if the head target has turned past this angle.
			/// </summary>
			[Range(0f, 180f)] public float maxRootAngle = 25f;

            [Tooltip("Angular offset for root heading. Adjust this value to turn the root relative to the HMD around the vertical axis. Usefulf for fighting or shooting games where you would sometimes want the avatar to stand at an angled stance.")]
            /// <summary>
            /// Angular offset for root heading. Adjust this value to turn the root relative to the HMD around the vertical axis. Usefulf for fighting or shooting games where you would sometimes want the avatar to stand at an angled stance.
            /// </summary>
            [Range(-180f, 180f)] public float rootHeadingOffset;

            /// <summary>
            /// Target position of the head. Will be overwritten if target is assigned.
            /// </summary>
            [NonSerialized][HideInInspector] public Vector3 IKPositionHead;

			/// <summary>
			/// Target rotation of the head. Will be overwritten if target is assigned.
			/// </summary>
			[NonSerialized][HideInInspector] public Quaternion IKRotationHead = Quaternion.identity;

			/// <summary>
			/// Target position of the pelvis. Will be overwritten if target is assigned.
			/// </summary>
			[NonSerialized][HideInInspector] public Vector3 IKPositionPelvis;

			/// <summary>
			/// Target rotation of the pelvis. Will be overwritten if target is assigned.
			/// </summary>
			[NonSerialized][HideInInspector] public Quaternion IKRotationPelvis = Quaternion.identity;

			/// <summary>
			/// The goal position for the chest. If chestGoalWeight > 0, the chest will be turned towards this position.
			/// </summary>
			[NonSerialized][HideInInspector] public Vector3 goalPositionChest;

			/// <summary>
			/// Position offset of the pelvis. Will be applied on top of pelvis target position and reset to Vector3.zero after each update.
			/// </summary>
			[NonSerialized][HideInInspector] public Vector3 pelvisPositionOffset;

			/// <summary>
			/// Position offset of the chest. Will be reset to Vector3.zero after each update.
			/// </summary>
			[NonSerialized][HideInInspector] public Vector3 chestPositionOffset;

			/// <summary>
			/// Position offset of the head. Will be applied on top of head target position and reset to Vector3.zero after each update.
			/// </summary>
			[NonSerialized][HideInInspector] public Vector3 headPositionOffset;

			/// <summary>
			/// Rotation offset of the pelvis. Will be reset to Quaternion.identity after each update.
			/// </summary>
			[NonSerialized][HideInInspector] public Quaternion pelvisRotationOffset = Quaternion.identity;

			/// <summary>
			/// Rotation offset of the chest. Will be reset to Quaternion.identity after each update.
			/// </summary>
			[NonSerialized][HideInInspector] public Quaternion chestRotationOffset = Quaternion.identity;

			/// <summary>
			/// Rotation offset of the head. Will be applied on top of head target rotation and reset to Quaternion.identity after each update.
			/// </summary>
			[NonSerialized][HideInInspector] public Quaternion headRotationOffset = Quaternion.identity;

			public VirtualBone pelvis { get { return bones[pelvisIndex]; }}
			public VirtualBone firstSpineBone { get { return bones[spineIndex]; }}
			public VirtualBone chest { 
				get { 
					if (hasChest) return bones[chestIndex];
					return bones[spineIndex];
				}
			}
			private VirtualBone neck { get { return bones[neckIndex]; }}
			public VirtualBone head { get { return bones[headIndex]; }}

			[NonSerialized][HideInInspector] public Vector3 faceDirection;
			[NonSerialized][HideInInspector] public Vector3 locomotionHeadPositionOffset;
			[NonSerialized][HideInInspector] public Vector3 headPosition;

			public Quaternion anchorRotation { get; private set; }
            public Quaternion anchorRelativeToHead { get; private set; }

            private Quaternion headRotation = Quaternion.identity;
            private Quaternion pelvisRotation = Quaternion.identity;
            private Quaternion anchorRelativeToPelvis = Quaternion.identity;
			private Quaternion pelvisRelativeRotation = Quaternion.identity;
			private Quaternion chestRelativeRotation = Quaternion.identity;
			private Vector3 headDeltaPosition;
			private Quaternion pelvisDeltaRotation = Quaternion.identity;
			private Quaternion chestTargetRotation = Quaternion.identity;
			private int pelvisIndex = 0, spineIndex = 1, chestIndex = -1, neckIndex = -1, headIndex = -1;
			private float length;
			private bool hasChest;
			private bool hasNeck;
            private bool hasLegs;
			private float headHeight;
			private float sizeMlp;
			private Vector3 chestForward;

			protected override void OnRead(Vector3[] positions, Quaternion[] rotations, bool hasChest, bool hasNeck, bool hasShoulders, bool hasToes, bool hasLegs, int rootIndex, int index) {
				Vector3 pelvisPos = positions[index];
				Quaternion pelvisRot = rotations[index];
				Vector3 spinePos = positions[index + 1];
				Quaternion spineRot = rotations[index + 1];
				Vector3 chestPos = positions[index + 2];
				Quaternion chestRot = rotations[index + 2];
				Vector3 neckPos = positions[index + 3];
				Quaternion neckRot = rotations[index + 3];
				Vector3 headPos = positions[index + 4];
				Quaternion headRot = rotations[index + 4];

                this.hasLegs = hasLegs;

				if (!hasChest) {
					chestPos = spinePos;
					chestRot = spineRot;
				}

				if (!initiated) {
					this.hasChest = hasChest;
					this.hasNeck = hasNeck;
					headHeight = V3Tools.ExtractVertical(headPos - positions[0], rotations[0] * Vector3.up, 1f).magnitude;

					int boneCount = 3;
					if (hasChest) boneCount++;
					if (hasNeck) boneCount++;
					bones = new VirtualBone[boneCount];

					chestIndex = hasChest? 2: 1;

					neckIndex = 1;
					if (hasChest) neckIndex++;
					if (hasNeck) neckIndex++;

					headIndex = 2;
					if (hasChest) headIndex++;
					if (hasNeck) headIndex++;

					bones[0] = new VirtualBone(pelvisPos, pelvisRot);
					bones[1] = new VirtualBone(spinePos, spineRot);
					if (hasChest) bones[chestIndex] = new VirtualBone(chestPos, chestRot);
					if (hasNeck) bones[neckIndex] = new VirtualBone(neckPos, neckRot);
					bones[headIndex] = new VirtualBone(headPos, headRot);

					pelvisRotationOffset = Quaternion.identity;
					chestRotationOffset = Quaternion.identity;
					headRotationOffset = Quaternion.identity;

					anchorRelativeToHead = Quaternion.Inverse(headRot) * rotations[0];
                    anchorRelativeToPelvis = Quaternion.Inverse(pelvisRot) * rotations[0];

                    faceDirection = rotations[0] * Vector3.forward;

                    IKPositionHead = headPos;
					IKRotationHead = headRot;
					IKPositionPelvis = pelvisPos;
					IKRotationPelvis = pelvisRot;
					goalPositionChest = chestPos + rotations[0] * Vector3.forward;
				}

                // Forward and up axes
                pelvisRelativeRotation = Quaternion.Inverse(headRot) * pelvisRot;
                chestRelativeRotation = Quaternion.Inverse(headRot) * chestRot;

                chestForward = Quaternion.Inverse(chestRot) * (rotations[0] * Vector3.forward);
                
                bones[0].Read(pelvisPos, pelvisRot);
				bones[1].Read(spinePos, spineRot);
				if (hasChest) bones[chestIndex].Read(chestPos, chestRot);
				if (hasNeck) bones[neckIndex].Read(neckPos, neckRot);
				bones[headIndex].Read(headPos, headRot);

				float spineLength = Vector3.Distance (pelvisPos, headPos);
				sizeMlp = spineLength / 0.7f;
			}

			public override void PreSolve() {
				if (headTarget != null) {
					IKPositionHead = headTarget.position;
					IKRotationHead = headTarget.rotation;
				}

				if (chestGoal != null) {
					goalPositionChest = chestGoal.position;
				}

				if (pelvisTarget != null) {
					IKPositionPelvis = pelvisTarget.position;
					IKRotationPelvis = pelvisTarget.rotation;
				}

				headPosition = V3Tools.Lerp(head.solverPosition, IKPositionHead, positionWeight);
				headRotation = QuaTools.Lerp(head.solverRotation, IKRotationHead, rotationWeight);

                pelvisRotation = QuaTools.Lerp(pelvis.solverRotation, IKRotationPelvis, rotationWeight);
            }

			public override void ApplyOffsets() {
				headPosition += headPositionOffset;

				Vector3 rootUp = rootRotation * Vector3.up;
				if (rootUp == Vector3.up) {
					headPosition.y = Math.Max(rootPosition.y + minHeadHeight, headPosition.y);
				} else {
					Vector3 toHead = headPosition - rootPosition;
					Vector3 hor = V3Tools.ExtractHorizontal(toHead, rootUp, 1f);
					Vector3 ver = toHead - hor;
					float dot = Vector3.Dot(ver, rootUp);
					if (dot > 0f) {
						if (ver.magnitude < minHeadHeight) ver = ver.normalized * minHeadHeight;
					} else {
						ver = -ver.normalized * minHeadHeight;
					}

					headPosition = rootPosition + hor + ver;
				}

				headRotation = headRotationOffset * headRotation;

				headDeltaPosition = headPosition - head.solverPosition;
				pelvisDeltaRotation = QuaTools.FromToRotation(pelvis.solverRotation, headRotation * pelvisRelativeRotation);

                if (pelvisRotationWeight <= 0f) anchorRotation = headRotation * anchorRelativeToHead;
                else if (pelvisRotationWeight > 0f && pelvisRotationWeight < 1f) anchorRotation = Quaternion.Lerp(headRotation * anchorRelativeToHead, pelvisRotation * anchorRelativeToPelvis, pelvisRotationWeight);
                else if (pelvisRotationWeight >= 1f) anchorRotation = pelvisRotation * anchorRelativeToPelvis;
            }

			private void CalculateChestTargetRotation(VirtualBone rootBone, Arm[] arms) {
				chestTargetRotation = headRotation * chestRelativeRotation;

				// Use hands to adjust c
				AdjustChestByHands(ref chestTargetRotation, arms);

				faceDirection = Vector3.Cross(anchorRotation * Vector3.right, rootBone.readRotation * Vector3.up) + anchorRotation * Vector3.forward;
			}

			public void Solve(VirtualBone rootBone, Leg[] legs, Arm[] arms) {
                CalculateChestTargetRotation(rootBone, arms);

                // Root rotation
                if (maxRootAngle < 180f)
                {
                    Vector3 f = faceDirection;
                    if (rootHeadingOffset != 0f) f = Quaternion.AngleAxis(rootHeadingOffset, Vector3.up) * f;
                    Vector3 faceDirLocal = Quaternion.Inverse(rootBone.solverRotation) * f;
                    float angle = Mathf.Atan2(faceDirLocal.x, faceDirLocal.z) * Mathf.Rad2Deg;

                    float rotation = 0f;
                    float maxAngle = maxRootAngle;

                    if (angle > maxAngle)
                    {
                        rotation = angle - maxAngle;
                    }
                    if (angle < -maxAngle)
                    {
                        rotation = angle + maxAngle;
                    }

                    rootBone.solverRotation = Quaternion.AngleAxis(rotation, rootBone.readRotation * Vector3.up) * rootBone.solverRotation;
                }

                Vector3 animatedPelvisPos = pelvis.solverPosition;
				Vector3 rootUp = rootBone.solverRotation * Vector3.up;

                // Translate pelvis to make the head's position & rotation match with the head target
                TranslatePelvis(legs, headDeltaPosition, pelvisDeltaRotation);
                
                FABRIKPass(animatedPelvisPos, rootUp, positionWeight);
                
                // Bend the spine to look towards chest target rotation
                Bend(bones, pelvisIndex, chestIndex, chestTargetRotation, chestRotationOffset, chestClampWeight, false, neckStiffness * rotationWeight);
                
                if (LOD < 1 && chestGoalWeight > 0f) {
					Quaternion c = Quaternion.FromToRotation(bones[chestIndex].solverRotation * chestForward, goalPositionChest - bones[chestIndex].solverPosition) * bones[chestIndex].solverRotation;
					Bend(bones, pelvisIndex, chestIndex, c, chestRotationOffset, chestClampWeight, false, chestGoalWeight * rotationWeight);
				}

				InverseTranslateToHead(legs, false, false, Vector3.zero, positionWeight);

                if (LOD < 1) FABRIKPass(animatedPelvisPos, rootUp, positionWeight);

                Bend(bones, neckIndex, headIndex, headRotation, headClampWeight, true, rotationWeight);                

                SolvePelvis ();                
            }

			private void FABRIKPass(Vector3 animatedPelvisPos, Vector3 rootUp, float weight) {
                Vector3 startPos = Vector3.Lerp(pelvis.solverPosition, animatedPelvisPos, maintainPelvisPosition) + pelvisPositionOffset;// - chestPositionOffset;
                Vector3 endPos = headPosition - chestPositionOffset;
                //Vector3 startOffset = rootUp * (bones[bones.Length - 1].solverPosition - bones[0].solverPosition).magnitude;
                Vector3 startOffset = Vector3.zero;// (bones[bones.Length - 1].solverPosition - bones[0].solverPosition) * weight;

                float dist = Vector3.Distance(bones[0].solverPosition, bones[bones.Length - 1].solverPosition);

				VirtualBone.SolveFABRIK(bones, startPos, endPos, weight, 1f, 1, dist, startOffset);
			}

			private void SolvePelvis() {
				// Pelvis target
				if (pelvisPositionWeight > 0f) {
					Quaternion headSolverRotation = head.solverRotation;
					
					Vector3 delta = ((IKPositionPelvis + pelvisPositionOffset) - pelvis.solverPosition) * pelvisPositionWeight;
					foreach (VirtualBone bone in bones) bone.solverPosition += delta;

                    Vector3 bendNormal = anchorRotation * Vector3.right;
                    
					if (hasChest && hasNeck) {
                        VirtualBone.SolveTrigonometric(bones, pelvisIndex, spineIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight * 0.6f);
                        VirtualBone.SolveTrigonometric(bones, pelvisIndex, chestIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight * 0.6f);
                        VirtualBone.SolveTrigonometric(bones, pelvisIndex, neckIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight * 1f);
                    } else if (hasChest && !hasNeck) {
                        VirtualBone.SolveTrigonometric(bones, pelvisIndex, spineIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight * 0.75f);
						VirtualBone.SolveTrigonometric(bones, pelvisIndex, chestIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight * 1f);
                    }
                    else if (!hasChest && hasNeck) {
						VirtualBone.SolveTrigonometric(bones, pelvisIndex, spineIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight * 0.75f);
						VirtualBone.SolveTrigonometric(bones, pelvisIndex, neckIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight * 1f);
					} else if (!hasNeck && !hasChest) {
						VirtualBone.SolveTrigonometric(bones, pelvisIndex, spineIndex, headIndex, headPosition, bendNormal, pelvisPositionWeight);
					}
					
					head.solverRotation = headSolverRotation;
				}
			}

			public override void Write(ref Vector3[] solvedPositions, ref Quaternion[] solvedRotations) {
				// Pelvis
				solvedPositions[index] = bones[0].solverPosition;
				solvedRotations[index] = bones[0].solverRotation;

				// Spine
				solvedRotations[index + 1] = bones[1].solverRotation;

				// Chest
				if (hasChest) solvedRotations[index + 2] = bones[chestIndex].solverRotation;

				// Neck
				if (hasNeck) solvedRotations[index + 3] = bones[neckIndex].solverRotation;

				// Head
				solvedRotations[index + 4] = bones[headIndex].solverRotation;
			}

			public override void ResetOffsets() {
				// Reset offsets to zero
				pelvisPositionOffset = Vector3.zero;
				chestPositionOffset = Vector3.zero;
				headPositionOffset = locomotionHeadPositionOffset;// Vector3.zero;
				pelvisRotationOffset = Quaternion.identity;
				chestRotationOffset = Quaternion.identity;
				headRotationOffset = Quaternion.identity;
			}

			private void AdjustChestByHands(ref Quaternion chestTargetRotation, Arm[] arms) {
                if (LOD > 0) return;

				Quaternion h = Quaternion.Inverse(anchorRotation);

				Vector3 pLeft = h * (arms[0].position - headPosition) / sizeMlp;
				Vector3 pRight = h * (arms[1].position - headPosition) / sizeMlp;

				Vector3 c = Vector3.forward;
				c.x += pLeft.x * Mathf.Abs(pLeft.x);
				c.x += pLeft.z * Mathf.Abs(pLeft.z);
				c.x += pRight.x * Mathf.Abs(pRight.x);
				c.x -= pRight.z * Mathf.Abs(pRight.z);
				c.x *= 5f * rotateChestByHands;

				float angle = Mathf.Atan2(c.x, c.z) * Mathf.Rad2Deg;
				Quaternion q = Quaternion.AngleAxis(angle, rootRotation * Vector3.up);

				chestTargetRotation = q * chestTargetRotation;

				Vector3 t = Vector3.up;
				t.x += pLeft.y;
				t.x -= pRight.y;
				t.x *= 0.5f * rotateChestByHands;

				angle = Mathf.Atan2(t.x, t.y) * Mathf.Rad2Deg;
				q = Quaternion.AngleAxis(angle, rootRotation * Vector3.back);

				chestTargetRotation = q * chestTargetRotation;
			}

			// Move the pelvis so that the head would remain fixed to the anchor
			public void InverseTranslateToHead(Leg[] legs, bool limited, bool useCurrentLegMag, Vector3 offset, float w) {
				Vector3 delta = (headPosition + offset - head.solverPosition) * w;// * (1f - pelvisPositionWeight); This makes the head lose it's target when pelvisPositionWeight is between 0 and 1.

				Vector3 p = pelvis.solverPosition + delta;
				MovePosition( limited? LimitPelvisPosition(legs, p, useCurrentLegMag): p);
			}

			// Move and rotate the pelvis
			private void TranslatePelvis(Leg[] legs, Vector3 deltaPosition, Quaternion deltaRotation) {
				// Rotation
				Vector3 p = head.solverPosition;

				deltaRotation = QuaTools.ClampRotation(deltaRotation, chestClampWeight, 2);

				Quaternion r = Quaternion.Slerp (Quaternion.identity, deltaRotation, bodyRotStiffness * rotationWeight);
				r = Quaternion.Slerp (r, QuaTools.FromToRotation (pelvis.solverRotation, IKRotationPelvis), pelvisRotationWeight);
				VirtualBone.RotateAroundPoint(bones, 0, pelvis.solverPosition, pelvisRotationOffset * r);

				deltaPosition -= head.solverPosition - p;

				// Position
				// Move the body back when head is moving down
				Vector3 m = rootRotation * Vector3.forward;
				float deltaY = V3Tools.ExtractVertical(deltaPosition, rootRotation * Vector3.up, 1f).magnitude;
				float backOffset = deltaY * -moveBodyBackWhenCrouching * headHeight;
				deltaPosition += m * backOffset;

                MovePosition (LimitPelvisPosition(legs, pelvis.solverPosition + deltaPosition * bodyPosStiffness * positionWeight, false));
			}

			// Limit the position of the pelvis so that the feet/toes would remain fixed
			private Vector3 LimitPelvisPosition(Leg[] legs, Vector3 pelvisPosition, bool useCurrentLegMag, int it = 2) {
                if (!hasLegs) return pelvisPosition;

				// Cache leg current mag
				if (useCurrentLegMag) {
					foreach (Leg leg in legs) {
						leg.currentMag = Vector3.Distance(leg.thigh.solverPosition, leg.lastBone.solverPosition);
					}
				}

				// Solve a 3-point constraint
				for (int i = 0; i < it; i++) {
					foreach (Leg leg in legs) {
						Vector3 delta = pelvisPosition - pelvis.solverPosition;
						Vector3 wantedThighPos = leg.thigh.solverPosition + delta;
						Vector3 toWantedThighPos = wantedThighPos - leg.position;
						float maxMag = useCurrentLegMag? leg.currentMag: leg.mag;
						Vector3 limitedThighPos = leg.position + Vector3.ClampMagnitude(toWantedThighPos, maxMag);
						pelvisPosition += limitedThighPos - wantedThighPos;

						// TODO rotate pelvis to accommodate, rotate the spine back then
					}
				}
				
				return pelvisPosition;
			}

			// Bending the spine to the head effector
			private void Bend(VirtualBone[] bones, int firstIndex, int lastIndex, Quaternion targetRotation, float clampWeight, bool uniformWeight, float w) {
				if (w <= 0f) return;
				if (bones.Length == 0) return;
				int bonesCount = (lastIndex + 1) - firstIndex;
				if (bonesCount < 1) return;

				Quaternion r = QuaTools.FromToRotation(bones[lastIndex].solverRotation, targetRotation);
				r = QuaTools.ClampRotation(r, clampWeight, 2);

				float step = uniformWeight? 1f / bonesCount: 0f;
				
				for (int i = firstIndex; i < lastIndex + 1; i++) {
					if (!uniformWeight) step = Mathf.Clamp(((i - firstIndex) + 1) / bonesCount, 0, 1f);
					VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition, Quaternion.Slerp(Quaternion.identity, r, step * w));
				}
			}

			// Bending the spine to the head effector
			private void Bend(VirtualBone[] bones, int firstIndex, int lastIndex, Quaternion targetRotation, Quaternion rotationOffset, float clampWeight, bool uniformWeight, float w) {
                if (w <= 0f) return;
                if (bones.Length == 0) return;
                int bonesCount = (lastIndex + 1) - firstIndex;
                if (bonesCount < 1) return;

                Quaternion r = QuaTools.FromToRotation(bones[lastIndex].solverRotation, targetRotation);
                r = QuaTools.ClampRotation(r, clampWeight, 2);
                float step = uniformWeight ? 1f / bonesCount : 0f;

                for (int i = firstIndex; i < lastIndex + 1; i++)
                {
                    
                    if (!uniformWeight)
                    {
                        if (bonesCount == 1)
                        {
                            step = 1f;
                        } else if (bonesCount == 2)
                        {
                            step = i == 0 ? 0.2f : 0.8f;
                        } else if (bonesCount == 3)
                        {
                            if (i == 0) step = 0.15f;
                            else if (i == 1) step = 0.4f;
                            else step = 0.45f;
                        } else if (bonesCount > 3)
                        {
                            step = 1f / bonesCount;
                        }
                    }

                    //if (!uniformWeight) step = Mathf.Clamp(((i - firstIndex) + 1) / bonesCount, 0, 1f);
                    VirtualBone.RotateAroundPoint(bones, i, bones[i].solverPosition, Quaternion.Slerp(Quaternion.Slerp(Quaternion.identity, rotationOffset, step), r, step * w));
                }
            }
		}
	}
}