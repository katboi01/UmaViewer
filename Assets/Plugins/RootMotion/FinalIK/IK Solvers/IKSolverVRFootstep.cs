using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace RootMotion.FinalIK {

	public partial class IKSolverVR: IKSolver {

		[System.Serializable]
		public class Footstep {

			public float stepSpeed = 3f;
			public Vector3 characterSpaceOffset;

			public Vector3 position;
			public Quaternion rotation = Quaternion.identity;
			public Quaternion stepToRootRot = Quaternion.identity;
			public bool isStepping { get { return stepProgress < 1f; }}
			public bool isSupportLeg;

			public float stepProgress { get; private set; }
			public Vector3 stepFrom;
			public Vector3 stepTo;
			public Quaternion stepFromRot = Quaternion.identity;
			public Quaternion stepToRot = Quaternion.identity;
			private Quaternion footRelativeToRoot = Quaternion.identity;
			private float supportLegW;
			private float supportLegWV;

			public Footstep (Quaternion rootRotation, Vector3 footPosition, Quaternion footRotation, Vector3 characterSpaceOffset) {
				this.characterSpaceOffset = characterSpaceOffset;
				Reset(rootRotation, footPosition, footRotation);
                footRelativeToRoot = Quaternion.Inverse(rootRotation) * rotation;
            }

			public void Reset(Quaternion rootRotation, Vector3 footPosition, Quaternion footRotation) {
				position = footPosition;
				rotation = footRotation;
				stepFrom = position;
				stepTo = position;
				stepFromRot = rotation;
				stepToRot = rotation;
				stepToRootRot = rootRotation;
				stepProgress = 1f;
			}

			public void StepTo(Vector3 p, Quaternion rootRotation, float stepThreshold) {
                if (Vector3.Magnitude(p - stepTo) < stepThreshold && Quaternion.Angle(rootRotation, stepToRootRot) < 25f) return;
                stepFrom = position;
				stepTo = p;
				stepFromRot = rotation;
				stepToRootRot = rootRotation;
				stepToRot = rootRotation * footRelativeToRoot;
				stepProgress = 0f;
			}

			public void UpdateStepping(Vector3 p, Quaternion rootRotation, float speed) {
				stepTo = Vector3.Lerp (stepTo, p, Time.deltaTime * speed);
				stepToRot = Quaternion.Lerp (stepToRot, rootRotation * footRelativeToRoot, Time.deltaTime * speed);

				stepToRootRot = stepToRot * Quaternion.Inverse(footRelativeToRoot);
			}

			public void UpdateStanding(Quaternion rootRotation, float minAngle, float speed) {
				if (speed <= 0f || minAngle >= 180f) return;

				Quaternion r = rootRotation * footRelativeToRoot;
				float angle = Quaternion.Angle (rotation, r);
				if (angle > minAngle) rotation = Quaternion.RotateTowards (rotation, r, Mathf.Min (Time.deltaTime * speed * (1f - supportLegW), angle -minAngle));
			}

			public void Update(InterpolationMode interpolation, UnityEvent onStep) {
				float supportLegWTarget = isSupportLeg ? 1f : 0f;
				supportLegW = Mathf.SmoothDamp (supportLegW, supportLegWTarget, ref supportLegWV, 0.2f);

				if (!isStepping) return;

				stepProgress = Mathf.MoveTowards(stepProgress, 1f, Time.deltaTime * stepSpeed);

				if (stepProgress >= 1f) onStep.Invoke ();

				float stepProgressSmooth = RootMotion.Interp.Float(stepProgress, interpolation);

				position = Vector3.Lerp(stepFrom, stepTo, stepProgressSmooth);
				rotation = Quaternion.Lerp(stepFromRot, stepToRot, stepProgressSmooth);
			}
		}
	}
}
