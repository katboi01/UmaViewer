using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	[System.Serializable]
	public class SubBehaviourCOM: SubBehaviourBase {

		[System.Serializable]
		public enum Mode {
			FeetCentroid,
			CenterOfPressure
		}

		public Mode mode;
		public float velocityDamper = 1f;
		public float velocityLerpSpeed = 5f;
		public float velocityMax = 1f;
		public float centerOfPressureSpeed = 5f;
		public Vector3 offset;

		public Vector3 position { get; private set; }
		public Vector3 direction { get; private set; }
		public float angle { get; private set; }
		public Vector3 velocity { get; private set; }
		public Vector3 centerOfPressure { get; private set; }
		public Quaternion rotation { get; private set; }
		public Quaternion inverseRotation { get; private set; }
		public bool isGrounded { get; private set; }
		public float lastGroundedTime { get; private set; }

		[HideInInspector] public bool[] groundContacts;
		[HideInInspector] public Vector3[] groundContactPoints;

		private LayerMask groundLayers;

		public void Initiate(BehaviourBase behaviour, LayerMask groundLayers) {
			this.behaviour = behaviour;
			this.groundLayers = groundLayers;

			rotation = Quaternion.identity;
			groundContacts = new bool[behaviour.puppetMaster.muscles.Length];
			groundContactPoints = new Vector3[groundContacts.Length];

			behaviour.OnPreActivate += OnPreActivate;
			behaviour.OnPreLateUpdate += OnPreLateUpdate;
			behaviour.OnPreDeactivate += OnPreDeactivate;
			behaviour.OnPreMuscleCollision += OnPreMuscleCollision;
			behaviour.OnPreMuscleCollisionExit += OnPreMuscleCollisionExit;
			behaviour.OnHierarchyChanged += OnHierarchyChanged;
		}

		#region Behaviour Delegates

		private void OnHierarchyChanged() {
			System.Array.Resize (ref groundContacts, behaviour.puppetMaster.muscles.Length);
			System.Array.Resize (ref groundContactPoints, behaviour.puppetMaster.muscles.Length);
		}

		private void OnPreMuscleCollision(MuscleCollision c) {
			if (!LayerMaskExtensions.Contains(groundLayers, c.collision.gameObject.layer)) return;
			if (c.collision.contacts.Length == 0) return;
			lastGroundedTime = Time.time;

			groundContacts[c.muscleIndex] = true;
			if (mode == Mode.CenterOfPressure) groundContactPoints[c.muscleIndex] = GetCollisionCOP(c.collision);
		}

		private void OnPreMuscleCollisionExit(MuscleCollision c) {
			if (!LayerMaskExtensions.Contains(groundLayers, c.collision.gameObject.layer)) return;
			groundContacts[c.muscleIndex] = false;
			groundContactPoints[c.muscleIndex] = Vector3.zero;
		}

		private void OnPreActivate() {
			position = GetCenterOfMass();
			centerOfPressure = GetFeetCentroid();
			direction = position - centerOfPressure;
			angle = Vector3.Angle(direction, Vector3.up);
			velocity = Vector3.zero;
		}

		private void OnPreLateUpdate() {
			// Ground contact
			isGrounded = IsGrounded();

			// COP
			if (mode == Mode.FeetCentroid || !isGrounded) {
				centerOfPressure = GetFeetCentroid();
			} else {
				Vector3 centerOfPressureTarget = isGrounded? GetCenterOfPressure(): GetFeetCentroid();
				centerOfPressure = centerOfPressureSpeed <= 02? centerOfPressureTarget: Vector3.Lerp(centerOfPressure, centerOfPressureTarget, Time.deltaTime * centerOfPressureSpeed);
			}

			// COM
			position = GetCenterOfMass();

			// COM Velocity
			Vector3 velocityPosition = GetCenterOfMassVelocity();

			Vector3 velocityTarget = velocityPosition - position;
			velocityTarget.y = 0f;
			velocityTarget = Vector3.ClampMagnitude(velocityTarget, velocityMax);

			// Add velocity to position
			velocity = velocityLerpSpeed <= 0f? velocityTarget: Vector3.Lerp(velocity, velocityTarget, Time.deltaTime * velocityLerpSpeed);
			position += velocity * velocityDamper;
			position += behaviour.puppetMaster.targetRoot.rotation * offset;

			// Calculate COM direction, rotation and angle
			direction = position - centerOfPressure;
			rotation = Quaternion.FromToRotation(Vector3.up, direction);
			inverseRotation = Quaternion.Inverse(rotation);
			angle = Quaternion.Angle(Quaternion.identity, rotation);
		}

		private void OnPreDeactivate() {
			velocity = Vector3.zero;
		}

		#endregion Behaviour Delegates

		private Vector3 GetCollisionCOP(Collision collision) {
			Vector3 sum = Vector3.zero;

			for (int i = 0; i < collision.contacts.Length; i++) {
				sum += collision.contacts[i].point;
			}
			
			return sum / (float)collision.contacts.Length;
		}

		private bool IsGrounded() {
			for (int i = 0; i < groundContacts.Length; i++) {
				if (groundContacts[i]) return true;
			}

			return false;
		}

		private Vector3 GetCenterOfMass() {
			Vector3 CoM = Vector3.zero;
			float c = 0f;
			
			foreach (Muscle m in behaviour.puppetMaster.muscles) {
				CoM += m.rigidbody.worldCenterOfMass * m.rigidbody.mass;

				c += m.rigidbody.mass;
			}
			
			return CoM /= c;
		}

		private Vector3 GetCenterOfMassVelocity() {
			Vector3 CoM = Vector3.zero;
			float c = 0f;
			
			foreach (Muscle m in behaviour.puppetMaster.muscles) {
				CoM += m.rigidbody.worldCenterOfMass * m.rigidbody.mass;
				CoM += m.rigidbody.velocity * m.rigidbody.mass;

				c += m.rigidbody.mass;
			}
			
			return CoM /= c;
		}
		
		private Vector3 GetMomentum() {
			Vector3 sum = Vector3.zero;
			for (int i = 0; i < behaviour.puppetMaster.muscles.Length; i++) {
				sum += behaviour.puppetMaster.muscles[i].rigidbody.velocity * behaviour.puppetMaster.muscles[i].rigidbody.mass;
			}
			return sum;
		}

		private Vector3 GetCenterOfPressure() {
			Vector3 sum = Vector3.zero;
			int contacts = 0;

			for (int i = 0; i < groundContacts.Length; i++) {
				if (groundContacts[i]) {
					sum += groundContactPoints[i];
					contacts ++;
				}
			}
			
			sum /= (float)contacts;
			return sum;
		}
		
		private Vector3 GetFeetCentroid() {
			Vector3 sum = Vector3.zero;
			int contacts = 0;

			/*
			int feetGrounded = 0;
			for (int i = 0; i < behaviour.puppetMaster.muscles.Length; i++) {
				if (behaviour.puppetMaster.muscles[i].props.group == Muscle.Group.Foot && groundContacts[i]) feetGrounded ++;
			}

			for (int i = 0; i < behaviour.puppetMaster.muscles.Length; i++) {
				if (behaviour.puppetMaster.muscles[i].props.group == Muscle.Group.Foot) {
					if (feetGrounded == 0 || (feetGrounded > 0 && groundContacts[i])) {
						sum += behaviour.puppetMaster.muscles[i].rigidbody.worldCenterOfMass;
						contacts ++;
					}
				}
			}
			*/

			for (int i = 0; i < behaviour.puppetMaster.muscles.Length; i++) {
				if (behaviour.puppetMaster.muscles[i].props.group == Muscle.Group.Foot) {
					sum += behaviour.puppetMaster.muscles[i].rigidbody.worldCenterOfMass;
					contacts ++;
				}
			}

			sum /= (float)contacts;
			return sum;
		}
	}
}
