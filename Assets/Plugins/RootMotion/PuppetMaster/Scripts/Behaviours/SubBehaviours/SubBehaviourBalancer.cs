using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

	[System.Serializable]
	public class SubBehaviourBalancer : SubBehaviourBase {

		/// <summary>
		/// Settings for SubBehaviourBalancer.
		/// </summary>
		[System.Serializable]
		public class Settings {

			[Tooltip("Ankle joint damper / spring. Increase to make the balancing effect softer.")]
			/// <summary>
			/// Ankle joint damper / spring. Increase to make the balancing effect softer.
			/// </summary>
			public float damperForSpring = 1f;

			[Tooltip("Multiplier for joint max force.")]
			/// <summary>
			/// Multiplier for joint max force.
			/// </summary>
			public float maxForceMlp = 0.05f;

			[Tooltip("Multiplier for the inertia tensor. Increasing this will increase the balancing forces.")]
			/// <summary>
			/// Multiplier for the inertia tensor. Increasing this will increase the balancing forces.
			/// </summary>
			public float IMlp = 1f;

			[Tooltip("Velocity-based prediction.")]
			/// <summary>
			/// Velocity-based prediction.
			/// </summary>
			public float velocityF = 0.5f;

			[Tooltip("World space offset for the center of pressure. Can be used to make the characer lean in a certain direction.")]
			/// <summary>
			/// World space offset for the center of pressure. Can be used to make the characer lean in a certain direction.
			/// </summary>
			public Vector3 copOffset;

			[Tooltip("The amount of torque applied to the lower legs to help keep the puppet balanced. Note that this is an external force (not coming from the joints themselves) and might make the simulation seem unnatural.")]
			/// <summary>
			/// The amount of torque applied to the lower legs to help keep the puppet balanced. Note that this is an external force (not coming from the joints themselves) and might make the simulation seem unnatural.
			/// </summary>
			public float torqueMlp = 0f;

			[Tooltip("Maximum magnitude of the torque applied to the lower legs if 'Torque Mlp' > 0.")]
			/// <summary>
			/// Maximum magnitude of the torque applied to the lower legs if 'Torque Mlp' > 0.
			/// </summary>
			public float maxTorqueMag = 45f;
		}

		public ConfigurableJoint joint { get; private set; }
		public Vector3 dir { get; private set;} 
		public Vector3 dirVel { get; private set;}
		public Vector3 cop { get; private set; }
		public Vector3 com { get; private set; }
		public Vector3 comV { get; private set; }

		private Settings settings;
		private Rigidbody[] rigidbodies = new Rigidbody[0];
		private Transform[] copPoints = new Transform[0];
		private PressureSensor pressureSensor;
		private Rigidbody Ibody;
		private Vector3 I;
		private Quaternion toJointSpace = Quaternion.identity;

		public void Initiate(BehaviourBase behaviour, Settings settings, Rigidbody Ibody, Rigidbody[] rigidbodies, ConfigurableJoint joint, Transform[] copPoints, PressureSensor pressureSensor) {
			this.behaviour = behaviour;
			this.settings = settings;
			this.Ibody = Ibody;
			this.rigidbodies = rigidbodies;
			this.joint = joint;
			this.copPoints = copPoints;
			this.pressureSensor = pressureSensor;

			toJointSpace = PhysXTools.ToJointSpace(joint);

			behaviour.OnPreFixedUpdate += Solve;
		}

		void Solve() {
			if (copPoints.Length == 0) {
				cop = joint.transform.TransformPoint(joint.anchor);
			} else {
				cop = Vector3.zero;
				foreach (Transform copPoint in copPoints) {
					cop += copPoint.position;
				}
				cop /= copPoints.Length;
			}
			
			cop += settings.copOffset;
			
			com = PhysXTools.GetCenterOfMass(rigidbodies);
			
			comV = PhysXTools.GetCenterOfMassVelocity(rigidbodies);
			
			dir = com - cop;
			dirVel = (com + comV * settings.velocityF) - cop;
			
			Vector3 requiredAcceleration = PhysXTools.GetFromToAcceleration(dirVel, -Physics.gravity);
			requiredAcceleration -= Ibody.angularVelocity;
			
			Vector3 torque = requiredAcceleration / Time.fixedDeltaTime;
			
			PhysXTools.ScaleByInertia(ref torque, Ibody.rotation, Ibody.inertiaTensor * settings.IMlp);
			
			torque = Vector3.ClampMagnitude(torque, settings.maxTorqueMag);
			
			bool pressured = pressureSensor == null || !pressureSensor.enabled || pressureSensor.inContact;
			
			if (pressured) {
				Ibody.AddTorque(torque * settings.torqueMlp, ForceMode.Force);
				joint.targetAngularVelocity = Quaternion.Inverse(toJointSpace) * Quaternion.Inverse(joint.transform.rotation) * torque;
			} else {
				joint.targetAngularVelocity = Vector3.zero;
			}
		}
	}
}
