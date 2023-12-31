using UnityEngine;
using System.Collections;

namespace RootMotion.FinalIK {

	public partial class Grounding {

		/// <summary>
		/// The %Grounding %Leg.
		/// </summary>
		public class Leg {

			/// <summary>
			/// Returns true distance from foot to ground is less that maxStep
			/// </summary>
			public bool isGrounded { get; private set; }
			/// <summary>
			/// Gets the current IK position of the foot.
			/// </summary>
			public Vector3 IKPosition { get; private set; }
			/// <summary>
			/// Gets the current rotation offset of the foot.
			/// </summary>
			public Quaternion rotationOffset = Quaternion.identity;
			/// <summary>
			/// Returns true, if the leg is valid and initiated
			/// </summary>
			public bool initiated { get; private set; }
			/// <summary>
			/// The height of foot from ground.
			/// </summary>
			public float heightFromGround { get; private set; }
			/// <summary>
			/// Velocity of the foot
			/// </summary>
			public Vector3 velocity { get; private set; }
			/// <summary>
			/// Gets the foot Transform.
			/// </summary>
			public Transform transform { get; private set; }
			/// <summary>
			/// Gets the current IK offset.
			/// </summary>
			public float IKOffset { get; private set; }

			public bool invertFootCenter;

            public RaycastHit heelHit { get; private set; }
            public RaycastHit capsuleHit { get; private set; }

            /// <summary>
            /// Gets the RaycastHit last used by the Grounder to get ground height at foot position.
            /// </summary>
            public RaycastHit GetHitPoint {
                get
                {
                    if (grounding.quality == Quality.Best) return capsuleHit;
                    return heelHit;
                }
            }

            /// <summary>
            /// Overrides the animated position of the foot.
            /// </summary>
            public void SetFootPosition(Vector3 position)
            {
                doOverrideFootPosition = true;
                overrideFootPosition = position;
            }
            
            private Grounding grounding;
			private float lastTime, deltaTime;
			private Vector3 lastPosition;
			private Quaternion toHitNormal, r;
			private Vector3 up = Vector3.up;
            private bool doOverrideFootPosition;
            private Vector3 overrideFootPosition;
            private Vector3 transformPosition;
			
			// Initiates the Leg
			public void Initiate(Grounding grounding, Transform transform) {
				initiated = false;
				this.grounding = grounding;
				this.transform = transform;
				up = Vector3.up;
				IKPosition = transform.position;
				rotationOffset = Quaternion.identity;
				
				initiated = true;
				OnEnable();
			}

			// Should be called each time the leg is (re)activated
			public void OnEnable() {
				if (!initiated) return;
				
				lastPosition = transform.position;
				lastTime = Time.deltaTime;
			}

			// Set everything to 0
			public void Reset() {
				lastPosition = transform.position;
				lastTime = Time.deltaTime;
				IKOffset = 0f;
				IKPosition = transform.position;
				rotationOffset = Quaternion.identity;
			}

			// Raycasting, processing the leg's position
			public void Process() {
				if (!initiated) return;
				if (grounding.maxStep <= 0) return;

                transformPosition = doOverrideFootPosition ? overrideFootPosition : transform.position;
                doOverrideFootPosition = false;

				deltaTime = Time.time - lastTime;
				lastTime = Time.time;
				if (deltaTime == 0f) return;

				up = grounding.up;
				heightFromGround = Mathf.Infinity;
				
				// Calculating velocity
				velocity = (transformPosition - lastPosition) / deltaTime;
				velocity = grounding.Flatten(velocity);
				lastPosition = transformPosition;

				Vector3 prediction = velocity * grounding.prediction;
				
				if (grounding.footRadius <= 0) grounding.quality = Grounding.Quality.Fastest;
				
				// Raycasting
				switch(grounding.quality) {

				// The fastest, single raycast
				case Grounding.Quality.Fastest:

					RaycastHit predictedHit = GetRaycastHit(prediction);
					SetFootToPoint(predictedHit.normal, predictedHit.point);
					break;

				// Medium, 3 raycasts
				case Grounding.Quality.Simple:

					heelHit = GetRaycastHit(Vector3.zero);
					Vector3 f = grounding.GetFootCenterOffset();
					if (invertFootCenter) f = -f;
					RaycastHit toeHit = GetRaycastHit(f + prediction);
					RaycastHit sideHit = GetRaycastHit(grounding.root.right * grounding.footRadius * 0.5f);
					
					Vector3 planeNormal = Vector3.Cross(toeHit.point - heelHit.point, sideHit.point - heelHit.point).normalized;
					if (Vector3.Dot(planeNormal, up) < 0) planeNormal = -planeNormal;
					
					SetFootToPlane(planeNormal, heelHit.point, heelHit.point);
					break;
				
				// The slowest, raycast and a capsule cast
				case Grounding.Quality.Best:
					heelHit = GetRaycastHit(invertFootCenter? -grounding.GetFootCenterOffset(): Vector3.zero);
					capsuleHit = GetCapsuleHit(prediction);

					SetFootToPlane(capsuleHit.normal, capsuleHit.point, heelHit.point);
					break;
				}

				// Is the foot grounded?
				isGrounded = heightFromGround < grounding.maxStep;

				float offsetTarget = stepHeightFromGround;
				if (!grounding.rootGrounded) offsetTarget = 0f;

				IKOffset = Interp.LerpValue(IKOffset, offsetTarget, grounding.footSpeed, grounding.footSpeed);
				IKOffset = Mathf.Lerp(IKOffset, offsetTarget, deltaTime * grounding.footSpeed);

				float legHeight = grounding.GetVerticalOffset(transformPosition, grounding.root.position);
				float currentMaxOffset = Mathf.Clamp(grounding.maxStep - legHeight, 0f, grounding.maxStep);

				IKOffset = Mathf.Clamp(IKOffset, -currentMaxOffset, IKOffset);

				RotateFoot();

				// Update IK values
				IKPosition = transformPosition - up * IKOffset;

				float rW = grounding.footRotationWeight;
				rotationOffset = rW >= 1? r: Quaternion.Slerp(Quaternion.identity, r, rW);
			}

			// Gets the height from ground clamped between min and max step height
			public float stepHeightFromGround {
				get {
					return Mathf.Clamp(heightFromGround, -grounding.maxStep, grounding.maxStep);
				}
			}

            // Get predicted Capsule hit from the middle of the foot
            private RaycastHit GetCapsuleHit(Vector3 offsetFromHeel)
            {
                RaycastHit hit = new RaycastHit();
                Vector3 f = grounding.GetFootCenterOffset();
                if (invertFootCenter) f = -f;
                Vector3 origin = transformPosition + f;

                if (grounding.overstepFallsDown)
                {
                    hit.point = origin - up * grounding.maxStep;
                }
                else
                {
                    hit.point = new Vector3(origin.x, grounding.root.position.y, origin.z);
                }
                hit.normal = up;

                // Start point of the capsule
                Vector3 capsuleStart = origin + grounding.maxStep * up;
                // End point of the capsule depending on the foot's velocity.
                Vector3 capsuleEnd = capsuleStart + offsetFromHeel;

                if (Physics.CapsuleCast(capsuleStart, capsuleEnd, grounding.footRadius, -up, out hit, grounding.maxStep * 2, grounding.layers))
                {
                    // Safeguarding from a CapsuleCast bug in Unity that might cause it to return NaN for hit.point when cast against large colliders.
                    if (float.IsNaN(hit.point.x))
                    {
                        hit.point = origin - up * grounding.maxStep * 2f;
                        hit.normal = up;
                    }
                }

                // Since Unity2017 Raycasts will return Vector3.zero when starting from inside a collider
                if (hit.point == Vector3.zero && hit.normal == Vector3.zero)
                {
                    if (grounding.overstepFallsDown)
                    {
                        hit.point = origin - up * grounding.maxStep;
                    }
                    else
                    {
                        hit.point = new Vector3(origin.x, grounding.root.position.y, origin.z);
                    }
                }

                return hit;
            }

            // Get simple Raycast from the heel
            private RaycastHit GetRaycastHit(Vector3 offsetFromHeel)
            {
                RaycastHit hit = new RaycastHit();
                Vector3 origin = transformPosition + offsetFromHeel;

                if (grounding.overstepFallsDown)
                {
                    hit.point = origin - up * grounding.maxStep;
                }
                else
                {
                    hit.point = new Vector3(origin.x, grounding.root.position.y, origin.z);
                }
                hit.normal = up;

                if (grounding.maxStep <= 0f) return hit;

                Physics.Raycast(origin + grounding.maxStep * up, -up, out hit, grounding.maxStep * 2, grounding.layers);

                // Since Unity2017 Raycasts will return Vector3.zero when starting from inside a collider
                if (hit.point == Vector3.zero && hit.normal == Vector3.zero)
                {
                    if (grounding.overstepFallsDown)
                    {
                        hit.point = origin - up * grounding.maxStep;
                    }
                    else
                    {
                        hit.point = new Vector3(origin.x, grounding.root.position.y, origin.z);
                    }
                }

                return hit;
            }

            // Rotates ground normal with respect to maxFootRotationAngle
            private Vector3 RotateNormal(Vector3 normal) {
				if (grounding.quality == Grounding.Quality.Best) return normal;
				return Vector3.RotateTowards(up, normal, grounding.maxFootRotationAngle * Mathf.Deg2Rad, deltaTime);
			}
			
			// Set foot height from ground relative to a point
			private void SetFootToPoint(Vector3 normal, Vector3 point) {
				toHitNormal = Quaternion.FromToRotation(up, RotateNormal(normal));
				
				heightFromGround = GetHeightFromGround(point);
			}
			
			// Set foot height from ground relative to a plane
			private void SetFootToPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 heelHitPoint) {
				planeNormal = RotateNormal(planeNormal);
				toHitNormal = Quaternion.FromToRotation(up, planeNormal);
				
				Vector3 pointOnPlane = V3Tools.LineToPlane(transformPosition + up * grounding.maxStep, -up, planeNormal, planePoint);
				
				// Get the height offset of the point on the plane
				heightFromGround = GetHeightFromGround(pointOnPlane);
				
				// Making sure the heel doesn't penetrate the ground
				float heelHeight = GetHeightFromGround(heelHitPoint);
				heightFromGround = Mathf.Clamp(heightFromGround, -Mathf.Infinity, heelHeight);
			}

			// Calculate height offset of a point
			private float GetHeightFromGround(Vector3 hitPoint) {
				return grounding.GetVerticalOffset(transformPosition, hitPoint) - rootYOffset;
			}
			
			// Adding ground normal offset to the foot's rotation
			private void RotateFoot() {
				// Getting the full target rotation
				Quaternion rotationOffsetTarget = GetRotationOffsetTarget();
				
				// Slerping the rotation offset
				r = Quaternion.Slerp(r, rotationOffsetTarget, deltaTime * grounding.footRotationSpeed);
			}
			
			// Gets the target hit normal offset as a Quaternion
			private Quaternion GetRotationOffsetTarget() {
				if (grounding.maxFootRotationAngle <= 0f) return Quaternion.identity;
				if (grounding.maxFootRotationAngle >= 180f) return toHitNormal;
				return Quaternion.RotateTowards(Quaternion.identity, toHitNormal, grounding.maxFootRotationAngle);
			}
			
			// The foot's height from ground in the animation
			private float rootYOffset {
				get {
					return grounding.GetVerticalOffset(transformPosition, grounding.root.position - up * grounding.heightOffset);
				}
			}		
		}
	}
}
