using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Shoulder rotator is a workaround for FBBIK not rotating the shoulder bones when pulled by hands.
	/// It get's the job done if you need it, but will take 2 solving iterations.
	/// </summary>
	public class ShoulderRotator : MonoBehaviour {

		[Tooltip("Weight of shoulder rotation")]
		public float weight = 1.5f;
		[Tooltip("The greater the offset, the sooner the shoulder will start rotating")]
		public float offset = 0.2f;

		private FullBodyBipedIK ik;
		private bool skip;

		void Start() {
			ik = GetComponent<FullBodyBipedIK>();

			// You can use just LateUpdate, but note that it doesn't work when you have animatePhysics turned on for the character.
			ik.solver.OnPostUpdate += RotateShoulders;
		}

		private void RotateShoulders () {
			if (ik == null) return;
			if (ik.solver.IKPositionWeight <= 0f) return;

			// Skipping the second update cycle
			if (skip) {
				skip = false;
				return;
			}

			RotateShoulder(FullBodyBipedChain.LeftArm, weight, offset); // Rotate the left shoulder
			RotateShoulder(FullBodyBipedChain.RightArm, weight, offset); // Rotate the right shoulder

			skip = true;
			ik.solver.Update(); // Update FBBIK again with the rotated shoulders
		}

		// Rotates a shoulder of a FBBIK character
		private void RotateShoulder(FullBodyBipedChain chain, float weight, float offset) {
			// Get FromToRotation from the current swing direction of the shoulder to the IK target direction
			Quaternion fromTo = Quaternion.FromToRotation(GetParentBoneMap(chain).swingDirection, ik.solver.GetEndEffector(chain).position - GetParentBoneMap(chain).transform.position);

			// Direction to the IK target
			Vector3 toTarget = ik.solver.GetEndEffector(chain).position - ik.solver.GetLimbMapping(chain).bone1.position;

			// Length of the limb
			float limbLength = ik.solver.GetChain(chain).nodes[0].length + ik.solver.GetChain(chain).nodes[1].length;

			// Divide IK Target direction magnitude by limb length to know how much the limb is being pulled
			float delta = (toTarget.magnitude / limbLength) - 1f + offset;
			delta = Mathf.Clamp(delta * weight, 0f, 1f);

			// Calculate the rotation offset for the shoulder
			Quaternion rotationOffset = Quaternion.Lerp(Quaternion.identity, fromTo, delta * ik.solver.GetEndEffector(chain).positionWeight * ik.solver.IKPositionWeight);

			// Rotate the shoulder
			ik.solver.GetLimbMapping(chain).parentBone.rotation = rotationOffset * ik.solver.GetLimbMapping(chain).parentBone.rotation;
		}

		// Get the shoulder BoneMap
		private IKMapping.BoneMap GetParentBoneMap(FullBodyBipedChain chain) {
			return ik.solver.GetLimbMapping(chain).GetBoneMap(IKMappingLimb.BoneMapType.Parent);
		}

		// Remove the delegate when destroyed
		void OnDestroy() {
			if (ik != null) ik.solver.OnPostUpdate -= RotateShoulders;
		}
	}
}
