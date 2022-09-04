using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos
{

    public class PlanetaryGravity : MonoBehaviour
    {

        public Planet planet;
        
        // The gravitational constant (also known as "universal gravitational constant", or as "Newton's constant"),
        // is an empirical physical constant involved in the calculation of gravitational effects in Sir Isaac Newton's law of universal gravitation and in Albert Einstein's general theory of relativity.
        private const float G = 6.672e-11f;

        private Rigidbody r
        {
            get
            {
                if (_r == null) _r = GetComponent<Rigidbody>();
                return _r;
            }
        }

        private Rigidbody _r;

        void FixedUpdate()
        {
            if (r != null && r.gameObject.activeInHierarchy && !r.isKinematic) ApplyGravity(r);
        }

        private void ApplyGravity(Rigidbody r)
        {
            r.useGravity = false;

            Vector3 direction = planet.transform.position - r.position;
            float sqrMag = direction.sqrMagnitude;
            float distance = Mathf.Sqrt(sqrMag);

            r.AddForce((direction / distance) * G * (planet.mass / sqrMag) * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
    }
}
