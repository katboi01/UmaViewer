using UnityEngine;
using System.Collections;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	public class Planet : MonoBehaviour {

		public float mass = 1000;

        void Awake() {
		    Rigidbody[] rigidbodies = (Rigidbody[])GameObject.FindObjectsOfType<Rigidbody>();

			foreach (Rigidbody r in rigidbodies) {
                var planetaryG = r.gameObject.AddComponent<PlanetaryGravity>();
                planetaryG.planet = this;
			}
		}
	}
}
