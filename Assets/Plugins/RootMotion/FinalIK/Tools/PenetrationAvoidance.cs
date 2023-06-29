using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;

namespace RootMotion.FinalIK {
	
	/// <summary>
	/// Prevents body parts from penetrating scene geometry by offsetting effectors away from the colliders.
	/// </summary>
	public class PenetrationAvoidance : OffsetModifier {
		
		/// <summary>
		/// Definition of avoidance and raycasting info.
		/// </summary>
		[System.Serializable]
		public class Avoider {
			
			/// <summary>
			/// Linking this to an effector
			/// </summary>
			[System.Serializable]
			public class EffectorLink {
				[Tooltip("Effector to apply the offset to.")] public FullBodyBipedEffector effector;
				[Tooltip("Multiplier of the offset value, can be negative.")] public float weight;
			}
			
			[Tooltip("Bones to start the raycast from. Multiple raycasts can be used by assigning more than 1 bone.")] public Transform[] raycastFrom;
			[Tooltip("The Transform to raycast towards. Usually the body part that you want to keep from penetrating.")] public Transform raycastTo;
			[Tooltip("If 0, will use simple raycasting, if > 0, will use sphere casting (better, but slower).")] [Range(0f, 1f)] public float raycastRadius;
			[Tooltip("Linking this to FBBIK effectors.")] public EffectorLink[] effectors;
			[Tooltip("The time of smooth interpolation of the offset value to avoid penetration.")] public float smoothTimeIn = 0.1f;
			[Tooltip("The time of smooth interpolation of the offset value blending out of penetration avoidance.")] public float smoothTimeOut = 0.3f;
			[Tooltip("Layers to keep penetrating from.")] public LayerMask layers;
			
			private Vector3 offset;
			private Vector3 offsetTarget;
			private Vector3 offsetV;
			
			public void Solve(IKSolverFullBodyBiped solver, float weight) {
				// Get the offset to interpolate to
				offsetTarget = GetOffsetTarget(solver);
				
				// Interpolating the offset value
				float smoothDampTime = offsetTarget.sqrMagnitude > offset.sqrMagnitude? smoothTimeIn: smoothTimeOut;
				offset = Vector3.SmoothDamp(offset, offsetTarget, ref offsetV, smoothDampTime);
				
				// Apply offset to the FBBIK effectors
				foreach (EffectorLink link in effectors) {
					solver.GetEffector(link.effector).positionOffset += offset * weight * link.weight;
				}
			}
			
			// Multiple raycasting to accumulate the offset
			private Vector3 GetOffsetTarget(IKSolverFullBodyBiped solver) {
				Vector3 t = Vector3.zero;
				
				foreach (Transform from in raycastFrom) {
					t += Raycast(from.position, raycastTo.position + t);
				}
				
				return t;
			}
			
			// Raycast, return the offset that would not penetrate any colliders
			private Vector3 Raycast(Vector3 from, Vector3 to) {
				Vector3 direction = to - from;
				float distance = direction.magnitude;
				RaycastHit hit;
				
				if (raycastRadius <= 0f) {
					Physics.Raycast(from, direction, out hit, distance, layers);
				} else {
					Physics.SphereCast(from, raycastRadius, direction, out hit, distance, layers);
				}
				
				if (hit.collider == null) return Vector3.zero;
				
				return Vector3.Project(-direction.normalized * (distance - hit.distance), hit.normal);
			}
		}
		
		[Tooltip("Definitions of penetration avoidances.")] public Avoider[] avoiders;
		
		// Called by IKSolverFullBody before updating
		protected override void OnModifyOffset() {
			foreach (Avoider avoider in avoiders) avoider.Solve(ik.solver, weight);
		}
	}
}
