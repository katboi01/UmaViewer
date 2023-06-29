using UnityEngine;
using System.Collections;
using RootMotion;

namespace RootMotion.FinalIK {

	public partial class Grounding {

		/// <summary>
		/// The %Grounding %Pelvis.
		/// </summary>
		public class Pelvis {
		
			/// <summary>
			/// Offset of the pelvis as a Vector3.
			/// </summary>
			public Vector3 IKOffset { get; private set; }
			/// <summary>
			/// Scalar vertical offset of the pelvis.
			/// </summary>
			public float heightOffset { get; private set; }

			private Grounding grounding;
			private Vector3 lastRootPosition;
			private float damperF;
			private bool initiated;
			private float lastTime;

			// Initiating the pelvis
			public void Initiate(Grounding grounding) {
				this.grounding = grounding;
				
				initiated = true;
				OnEnable();
			}

			// Set everything to 0
			public void Reset() {
				this.lastRootPosition = grounding.root.transform.position;
				lastTime = Time.deltaTime;
				IKOffset = Vector3.zero;
				heightOffset = 0f;
			}
			
			// Should be called each time the pelvis is (re)activated
			public void OnEnable() {
				if (!initiated) return;
				this.lastRootPosition = grounding.root.transform.position;
				lastTime = Time.time;
			}
			
			// Updates the pelvis position offset
			public void Process(float lowestOffset, float highestOffset, bool isGrounded) {
				if (!initiated) return;

				float deltaTime = Time.time - lastTime;
				lastTime = Time.time;
				if (deltaTime <= 0f) return;
				
				float offsetTarget = lowestOffset + highestOffset;
				if (!grounding.rootGrounded) offsetTarget = 0f;
				
				// Interpolating the offset
				heightOffset = Mathf.Lerp(heightOffset, offsetTarget, deltaTime * grounding.pelvisSpeed);

				// Damper
				Vector3 rootDelta = (grounding.root.position - lastRootPosition);
				lastRootPosition = grounding.root.position;
				
				// Fading out damper when ungrounded
				damperF = Interp.LerpValue(damperF, isGrounded? 1f: 0f, 1f, 10f);
				
				// Calculating the final damper
				heightOffset -= grounding.GetVerticalOffset(rootDelta, Vector3.zero) * grounding.pelvisDamper * damperF;

				// Update IK value
				IKOffset = grounding.up * heightOffset;
			}
		}
	}
}

