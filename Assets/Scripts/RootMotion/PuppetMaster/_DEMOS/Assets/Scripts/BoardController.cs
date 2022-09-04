using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.Dynamics;

namespace RootMotion.Demos
{

    // Simple snowboard controller template.
    public class BoardController : MonoBehaviour
    {

        public int groundLayer = 4;
        public Transform rotationTarget;
        public float torque = 1f;
        public float skidDrag = 0.5f;
        public float turnSensitivity = 15f;

        private Rigidbody r;
        private bool isGrounded;

        void Awake()
        {
            r = GetComponent<Rigidbody>();
        }

        void Update()
        {
            // Turning the board
            float turn = Input.GetAxis("Horizontal");
            rotationTarget.rotation = Quaternion.AngleAxis(turn * turnSensitivity * Mathf.Min(r.velocity.sqrMagnitude * 0.2f, 1f) * Time.deltaTime, Vector3.up) * rotationTarget.rotation;
        }

        private void FixedUpdate()
        {
            // Add torque to the Rigidbody to make it follow the rotation target
            Vector3 angularAcc = PhysXTools.GetAngularAcceleration(r.rotation, rotationTarget.rotation);
            r.AddTorque(angularAcc * torque);

            // Add snowboard-like skid drag
            if (isGrounded)
            {
                Vector3 velocity = r.velocity;
                Vector3 skid = V3Tools.ExtractHorizontal(velocity, r.rotation * Vector3.up, 1f);
                skid = Vector3.Project(velocity, r.rotation * Vector3.right);

                r.velocity = velocity - Vector3.ClampMagnitude(skid * skidDrag * Time.deltaTime, skid.magnitude);
            }
        }

        // Check if the board is grounded
        void OnCollisionEnter(Collision c)
        {
            if (c.collider.gameObject.layer != groundLayer) return;
            isGrounded = true;
        }

        void OnCollisionStay(Collision c)
        {
            if (c.collider.gameObject.layer != groundLayer) return;
            isGrounded = true;
        }

        void OnCollisionExit(Collision c)
        {
            if (c.collider.gameObject.layer != groundLayer) return;
            isGrounded = false;
        }
    }
}
