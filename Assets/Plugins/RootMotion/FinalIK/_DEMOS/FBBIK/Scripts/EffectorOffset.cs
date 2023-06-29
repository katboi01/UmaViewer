using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.Demos {

	/// <summary>
	/// Demo for offsetting Effectors.
	/// </summary>
	public class EffectorOffset : OffsetModifier {

		// If 1, The hand effectors will maintain their position relative to their parent triangle's rotation {root node, left shoulder, right shoulder} 
		[Range(0f, 1f)]
		public float handsMaintainRelativePositionWeight;

		// The offset vectors for each effector
		public Vector3 bodyOffset, leftShoulderOffset, rightShoulderOffset, leftThighOffset, rightThighOffset, leftHandOffset, rightHandOffset, leftFootOffset, rightFootOffset;

		protected override void OnModifyOffset() {
			// How much will the hand effectors maintain their position relative to their parent triangle's rotation {root node, left shoulder, right shoulder} ?
			ik.solver.leftHandEffector.maintainRelativePositionWeight = handsMaintainRelativePositionWeight;
			ik.solver.rightHandEffector.maintainRelativePositionWeight = handsMaintainRelativePositionWeight;

			// Apply position offsets relative to this GameObject's rotation.
			ik.solver.bodyEffector.positionOffset += transform.rotation * bodyOffset * weight;
			ik.solver.leftShoulderEffector.positionOffset += transform.rotation * leftShoulderOffset * weight;
			ik.solver.rightShoulderEffector.positionOffset += transform.rotation * rightShoulderOffset * weight;
			ik.solver.leftThighEffector.positionOffset += transform.rotation * leftThighOffset * weight;
			ik.solver.rightThighEffector.positionOffset += transform.rotation * rightThighOffset * weight;
			ik.solver.leftHandEffector.positionOffset += transform.rotation * leftHandOffset * weight;
			ik.solver.rightHandEffector.positionOffset += transform.rotation * rightHandOffset * weight;
			ik.solver.leftFootEffector.positionOffset += transform.rotation * leftFootOffset * weight;
			ik.solver.rightFootEffector.positionOffset += transform.rotation * rightFootOffset * weight;

			// NB! effector position offsets are reset to Vector3.zero after FBBIK update is complete. 
			// This enables to have more than one script modifying the position offset of effectors.
			// Therefore instead of writing effector.positionOffset = value, write effector.positionOffset += value instead.
		}
	}
}
