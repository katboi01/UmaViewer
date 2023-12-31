using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	/// <summary>
	/// Dedicated abstrac base component for the Grounding solver.
	/// </summary>
	public abstract class Grounder: MonoBehaviour {
		
		#region Main Interface

		/// <summary>
		/// The master weight. Use this to fade in/out the grounding effect.
		/// </summary>
		[Tooltip("The master weight. Use this to fade in/out the grounding effect.")]
		[Range(0f, 1f)] public float weight = 1f;
		/// <summary>
		/// The %Grounding solver. Not to confuse with IK solvers.
		/// </summary>
		[Tooltip("The Grounding solver. Not to confuse with IK solvers.")]
		public Grounding solver = new Grounding();
		
		/// <summary>
		/// Delegate for Grounder events.
		/// </summary>
		public delegate void GrounderDelegate();
		/// <summary>
		/// Called before the Grounder updates it's solver.
		/// </summary>
		public GrounderDelegate OnPreGrounder;
		/// <summary>
		/// Called after the Grounder has updated it's solver and before the IK is applied.
		/// </summary>
		public GrounderDelegate OnPostGrounder;

		/// <summary>
		/// Resets this Grounder so characters can be teleported instananeously.
		/// </summary>
		public abstract void ResetPosition();
		
		#endregion Main Interface

		public bool initiated { get; protected set; }

		// Gets the spine bend direction
		protected Vector3 GetSpineOffsetTarget() {
			Vector3 sum = Vector3.zero;
			for (int i = 0; i < solver.legs.Length; i++) {
				sum += GetLegSpineBendVector(solver.legs[i]);
			}
			return sum;
		}

		// Logs the warning if no other warning has beed logged in this session.
		protected void LogWarning(string message) {
			Warning.Log(message, transform);
		}

		// Gets the bend direction for a foot
		private Vector3 GetLegSpineBendVector(Grounding.Leg leg) {
			Vector3 spineTangent = GetLegSpineTangent(leg);
			float dotF = (Vector3.Dot(solver.root.forward, spineTangent.normalized) + 1) * 0.5f;
			float w = (leg.IKPosition - leg.transform.position).magnitude;
			return spineTangent * w * dotF;
		}
		
		// Gets the direction from the root to the foot (ortho-normalized to root.up)
		private Vector3 GetLegSpineTangent(Grounding.Leg leg) {
			Vector3 tangent = leg.transform.position - solver.root.position;
			
			if (!solver.rotateSolver || solver.root.up == Vector3.up) return new Vector3(tangent.x, 0f, tangent.z);
			
			Vector3 normal = solver.root.up;
			Vector3.OrthoNormalize(ref normal, ref tangent);
			return tangent;
		}

		// Open the User Manual url
		protected abstract void OpenUserManual();
		
		// Open the Script Reference url
		protected abstract void OpenScriptReference();
	}
}

