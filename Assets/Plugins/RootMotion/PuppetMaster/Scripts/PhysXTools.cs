using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics {

    /// <summary>
    /// Library of static helper methods for working with PhysX Rigidbodies.
    /// </summary>
	public static class PhysXTools {

        /// <summary>
        /// Predicts unconstrained Rigidbody (ignoring collisions and joints) position and rotation after specified PhysX steps.
        /// </summary>
        public static void Predict(Rigidbody r, int steps, out Vector3 position, out Quaternion rotation)
        {
            Predict(r, steps, out position, out rotation, Physics.gravity, r.drag, r.angularDrag);
        }

        /// <summary>
        /// Predicts unconstrained Rigidbody (ignoring collisions and joints) position and rotation after specified PhysX steps using custom gravity, drag and angularDrag settings.
        /// </summary>
        public static void Predict(Rigidbody r, int steps, out Vector3 position, out Quaternion rotation, Vector3 gravity, float drag, float angularDrag)
        {
            position = r.position;
            rotation = r.rotation;
            Vector3 velocity = r.velocity;
            Vector3 angularVelocity = r.angularVelocity;

            for (int i = 0; i < steps; i++)
            {
                Predict(ref position, ref rotation, ref velocity, ref angularVelocity, gravity, drag, angularDrag);
            }
        }

        /// <summary>
        /// Predicts a single PhysX step of an unconstrained Rigidbody (ignoring collisions and joints) position and rotation using custom gravity, drag and angularDrag settings.
        /// </summary>
        public static void Predict(ref Vector3 position, ref Quaternion rotation, ref Vector3 velocity, ref Vector3 angularVelocity, Vector3 gravity, float drag, float angularDrag)
        {
            velocity += gravity * Time.fixedDeltaTime;

            velocity -= velocity * drag * Time.fixedDeltaTime;
            angularVelocity -= angularVelocity * angularDrag * Time.fixedDeltaTime;

            Vector3 deltaPos = velocity * Time.fixedDeltaTime;
            Vector3 deltaRot = angularVelocity * Time.fixedDeltaTime * Mathf.Rad2Deg;

            position += deltaPos;
            rotation *= Quaternion.Euler(deltaRot);
        }

        /// <summary>
        /// Gets the center of mass of a puppet.
        /// </summary>
        public static Vector3 GetCenterOfMass(PuppetMaster puppet) {
			Vector3 CoM = Vector3.zero;
			float c = 0f;
			
			for (int i = 0; i < puppet.muscles.Length; i++) {
				if (puppet.muscles[i].joint.gameObject.activeInHierarchy) {
					CoM += puppet.muscles[i].rigidbody.worldCenterOfMass * puppet.muscles[i].rigidbody.mass;
					
					c += puppet.muscles[i].rigidbody.mass;
				}
			}
			
			return CoM / c;
		}

		/// <summary>
		/// Gets the center of mass of an array of Rigidbodies.
		/// </summary>
		public static Vector3 GetCenterOfMass(Rigidbody[] rigidbodies) {
			Vector3 CoM = Vector3.zero;
			float c = 0f;
			
			for (int i = 0; i < rigidbodies.Length; i++) {
				if (rigidbodies[i].gameObject.activeInHierarchy) {
					CoM += rigidbodies[i].worldCenterOfMass * rigidbodies[i].mass;
					
					c += rigidbodies[i].mass;
				}
			}
			
			return CoM / c;
		}

		/// <summary>
		/// Gets the velocity of the center of mass of an array of Rigidbodies.
		/// </summary>
		public static Vector3 GetCenterOfMassVelocity(Rigidbody[] rigidbodies) {
			Vector3 CoM = Vector3.zero;
			float c = 0f;
			
			for (int i = 0; i < rigidbodies.Length; i++) {
				if (rigidbodies[i].gameObject.activeInHierarchy) {
					CoM += rigidbodies[i].velocity * rigidbodies[i].mass;
					
					c += rigidbodies[i].mass;
				}
			}
			
			return CoM / c;
		}

		/// <summary>
		/// Divides an angular acceleration by an inertia tensor.
		/// </summary>
		public static void DivByInertia(ref Vector3 v, Quaternion rotation, Vector3 inertiaTensor) {
			v = rotation * Div(Quaternion.Inverse(rotation) * v, inertiaTensor);
		}
		
		/// <summary>
		/// Scales an angular acceleration by an inertia tensor
		/// </summary>
		public static void ScaleByInertia(ref Vector3 v, Quaternion rotation, Vector3 inertiaTensor) {
			v = rotation * Vector3.Scale(Quaternion.Inverse(rotation) * v, inertiaTensor);
		}
		
		/// <summary>
		/// Returns the angular acceleration from one vector to another.
		/// </summary>
		public static Vector3 GetFromToAcceleration(Vector3 fromV, Vector3 toV) {
			Quaternion fromTo = Quaternion.FromToRotation(fromV, toV);
			float requiredAccelerationDeg = 0f;
			Vector3 axis = Vector3.zero;
			fromTo.ToAngleAxis(out requiredAccelerationDeg, out axis);
			
			Vector3 requiredAcceleration = requiredAccelerationDeg * axis * Mathf.Deg2Rad;
			
			return requiredAcceleration / Time.fixedDeltaTime;
		}
		
		/// <summary>
		/// Returns the angular acceleration from "fromR" to "toR." 
		/// Does not guarantee full accuracy with rotations around multiple axes).
		/// </summary>
		public static Vector3 GetAngularAcceleration(Quaternion fromR, Quaternion toR) {
			Vector3 axis = Vector3.Cross(fromR * Vector3.forward, toR * Vector3.forward);
			Vector3 axis2 = Vector3.Cross(fromR * Vector3.up, toR * Vector3.up);
			float angle = Quaternion.Angle(fromR, toR);
			Vector3 acc = Vector3.Normalize(axis + axis2) * angle * Mathf.Deg2Rad;
			
			return acc / Time.fixedDeltaTime;
		}

        /// <summary>
		/// Adds torque to the Ridigbody that accelerates it from it's current rotation to another using any force mode.
		/// </summary>
        public static void AddFromToTorque(Rigidbody r, Quaternion toR, ForceMode forceMode) {
            Vector3 requiredAcceleration = GetAngularAcceleration(r.rotation, toR); // Acceleration required for a single solver step
            requiredAcceleration -= r.angularVelocity; // Compensate for angular velocity

            switch (forceMode) {
                case ForceMode.Acceleration:
                    r.AddTorque(requiredAcceleration / Time.fixedDeltaTime, forceMode);
                    break;

                case ForceMode.Force:
                    Vector3 force = requiredAcceleration / Time.fixedDeltaTime;
                    ScaleByInertia(ref force, r.rotation, r.inertiaTensor);
                    r.AddTorque(force, forceMode);
                    break;

                case ForceMode.Impulse:
                    Vector3 impulse = requiredAcceleration;
                    ScaleByInertia(ref impulse, r.rotation, r.inertiaTensor);
                    r.AddTorque(impulse, forceMode);
                    break;

                case ForceMode.VelocityChange:
                    r.AddTorque(requiredAcceleration, forceMode);
                    break;
            }
        }
		
		/// <summary>
		/// Adds torque to the Ridigbody that accelerates it from one direction to another using any force mode.
		/// </summary>
		public static void AddFromToTorque(Rigidbody r, Vector3 fromV, Vector3 toV, ForceMode forceMode) {
			Vector3 requiredAcceleration = GetFromToAcceleration(fromV, toV); // Acceleration required for a single solver step
			requiredAcceleration -= r.angularVelocity; // Compensate for angular velocity
			
			switch(forceMode) {
			case ForceMode.Acceleration:
				r.AddTorque(requiredAcceleration / Time.fixedDeltaTime, forceMode);
				break;
				
			case ForceMode.Force:
				Vector3 force = requiredAcceleration / Time.fixedDeltaTime;
				ScaleByInertia(ref force, r.rotation, r.inertiaTensor);
				r.AddTorque(force, forceMode);
				break;
				
			case ForceMode.Impulse:
				Vector3 impulse = requiredAcceleration;
				ScaleByInertia(ref impulse, r.rotation, r.inertiaTensor);
				r.AddTorque(impulse, forceMode);
				break;
				
			case ForceMode.VelocityChange:
				r.AddTorque(requiredAcceleration, forceMode);
				break;
			}
		}
		
		/// <summary>
		/// Adds a force to a Rigidbody that gets it from one place to another within a single simulation step using any force mode.
		/// </summary>
		public static void AddFromToForce(Rigidbody r, Vector3 fromV, Vector3 toV, ForceMode forceMode) {
			Vector3 requiredAcceleration = GetLinearAcceleration(fromV, toV);
			requiredAcceleration -= r.velocity;
			
			switch(forceMode) {
			case ForceMode.Acceleration:
				r.AddForce(requiredAcceleration / Time.fixedDeltaTime, forceMode);
				break;
				
			case ForceMode.Force:
				Vector3 force = requiredAcceleration / Time.fixedDeltaTime;
				force *= r.mass;
				r.AddForce(force, forceMode);
				break;
				
			case ForceMode.Impulse:
				Vector3 impulse = requiredAcceleration;
				impulse *= r.mass;
				r.AddForce(impulse, forceMode);
				break;
				
			case ForceMode.VelocityChange:
				r.AddForce(requiredAcceleration, forceMode);
				break;
			}
		}

        /// <summary>
        /// Returns the linear acceleration from one point to another.
        /// </summary>
        public static Vector3 GetLinearAcceleration(Vector3 fromPoint, Vector3 toPoint) {
            return (toPoint - fromPoint) / Time.fixedDeltaTime;
        }

        /// <summary>
        /// The rotation expressed by the joint's axis and secondary axis
        /// </summary>
        public static Quaternion ToJointSpace(ConfigurableJoint joint) {
			Vector3 forward = Vector3.Cross (joint.axis, joint.secondaryAxis);
			Vector3 up = Vector3.Cross (forward, joint.axis);
			return Quaternion.LookRotation (forward, up);
		}

		/// <summary>
		/// Calculates the inertia tensor for a cuboid.
		/// </summary>
		public static Vector3 CalculateInertiaTensorCuboid(Vector3 size, float mass) {
            float x2 = size.x * size.x;
            float y2 = size.y * size.y;
            float z2 = size.z * size.z;
			
			float mlp = 1f/12f * mass;
			
			return new Vector3(
				mlp * (y2 + z2),
				mlp * (x2 + z2),
				mlp * (x2 + y2)); 
		}

		/// <summary>
		/// Divide all the values in v by their respective values in v2.
		/// </summary>
		public static Vector3 Div(Vector3 v, Vector3 v2) {
			return new Vector3(v.x / v2.x, v.y / v2.y, v.z / v2.z);
		}
	}
}
